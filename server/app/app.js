const express = require("express");
const bodyParser = require("body-parser");
const low = require("lowdb");
const FileAsync = require("lowdb/adapters/FileAsync");
const fs = require("fs");
const path = require("path");
const uuidv4 = require('uuid/v4');
const cors = require('cors')

// Room model
class Room {
    constructor(roomCode, hostPlayer) {
        this.spawnTimeStamp = (new Date()).getTime();
        this.lastUpdateTimeStamp = this.spawnTimeStamp;
        this.roomCode = roomCode;
        this.hostPlayer = hostPlayer;
        this.players = [hostPlayer];
        this.inRoom = true;
        this.inGame = false;
        this.inFinal = false;
        this.gameState = {};
        this.phoneLines = [];
    }
}

// Gamestate
/*
player
    phoneLineIndex
    xPosition


*/

// Hardcoded token to cut away bots crawling semi-sensitive information
// Does not secure anything, just makes sure that "most" of the traffic to the public API
// is originated from the right source (the game). The other option is to authenticate users, overkill
const token = "9QfdXsTwmOPySh1zaB8A";

const MIN_PLAYER_COUNT = 1;
const MAX_PLAYER_COUNT = 4;

module.exports = function() {
    const app = express();
    app.use(bodyParser.json());
    app.use(cors());

    // Create database instance and start server
    const adapter = new FileAsync("db.json");

    // Room servicing
    var rooms = [];

    // Clear old rooms every 2.5 minutes
    // Avoids a memory leak over time
    setInterval(() => {
        var now = (new Date()).getTime();
        rooms = rooms.filter(room => now - room["lastUpdateTimeStamp"] < 5*60*1000);
    }, 2.5*60*1000);

    // API routes

    // Main api page, acts as a ping endpoint
    app.get("/api", function(req, res) {
        res.setHeader("Content-Type", "text/html; charset=utf-8");
        res.end("Wikibirds API");
    });

    // Rooms
    app.put("/api/new-room", function(req, res) {
        if (!validateToken(req, res)) return;
        if (!validate(req, res, "PlayerID", false)) return;

        var hostPlayerID = req.body["PlayerID"];

        var newRoomCode = "";

        do {
            newRoomCode = getRandomRoomCode();
        } while (rooms.findIndex(room => room["roomCode"] === newRoomCode) !== -1)

        var newRoom = new Room(newRoomCode, hostPlayerID);

        rooms.push(newRoom);

        res.status(200).send({ message: "success", roomCode: newRoomCode });

        console.log("Player " + hostPlayerID + " created room " + newRoomCode);
    });

    app.put("/api/join-room", function(req, res) {
        if (!validateToken(req, res)) return;
        if (!validate(req, res, "PlayerID", false)) return;
        if (!validate(req, res, "RoomID", false)) return;

        if (!validateRoom(req, res, rooms)) return;

        var wantedRoomId = req.body["RoomID"];
        var roomIndex = rooms.findIndex(room => room["roomCode"] === wantedRoomId);
        var playerId = req.body["PlayerID"];

        // If player is already in room
        if (rooms[roomIndex].players.indexOf(playerId) !== -1) {
            res.status(403).send({ error: "Player " + playerId + " already in room " + wantedRoomId});
            return;
        }

        // Add player to room
        rooms[roomIndex].players.push(playerId);

        res.status(200).send({ message: "success" })

        console.log("Player " + playerId + " joined room " + wantedRoomId + " (total " + rooms[roomIndex].players.length + " players in room)");
    });

    app.put("/api/start-room", function(req, res) {
        if (!validateToken(req, res)) return;
        if (!validate(req, res, "PlayerID", false)) return;
        if (!validate(req, res, "RoomID", false)) return;


        if (!validateRoom(req, res, rooms)) return;
        if (!validateRoomOwner(req, res, rooms)) return;
        if (!validateRoomStatus(req, res, rooms, true, false, false)) return;

        res.status(200).send({ message: "room started" });

        var room = getRoom(req, rooms);

        // Update room status
        room["inRoom"] = false;
        room["inGame"] = true;

        console.log("Room " + room["roomCode"] + " was started (" + room.players.length + " players)");
    });

    // API routes that touch the DB
    low(adapter)
        .then(db => {

            // define routes that need db connection

            // Set db default values
            return db.defaults({ todo: [] }).write();
        });

    return app;
};

function validateToken(req, res) {
    if (req.body["Token"] !== token) {
        res.status(403).send({ error: "Invalid token in request body"});
        return false;
    }

    return true;
}

function isNumeric(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}

/*
 * Validates a given field of the request body.
 * The body must contain the field, and if numeric = True,
 * the field string must resolve to a valid number.
 */
function validate(req, res, field, numeric) {
    if (!req.body[field]) {
        res.status(400).send({ error: field + " not found in request body" });
        return false;
    }

    if (numeric && !isNumeric(req.body[field])) {
        res.status(400).send({ error: field + " is not numeric" });
        return false;
    }

    return true;
}

function validateRoom(req, res, rooms) {
    var wantedRoomId = req.body["RoomID"];
    var roomIndex = rooms.findIndex(room => room["roomCode"] === wantedRoomId);

    if (roomIndex === -1) {
        res.status(404).send({ error: "Room " + wantedRoomId + " not found."});
        return false;
    }

    return true;
}

function validatePlayerInRoom(req, res, rooms, roomId) {
    var playerId = req.body["PlayerID"];

    var room = getRoom(req, rooms);
    if (!validateRoomObject(res, room)) return false;

    // If player is not in room
    if (room.players.indexOf(playerId) === -1) {
        res.status(403).send({ error: "Player " + playerId + " not in room " + room["roomCode"]});
        return false;
    }

    return true;
}

function validateRoomOwner(req, res, rooms) {
    var room = getRoom(req, rooms);
    if (!validateRoomObject(res, rooms)) return false;

    return room.hostPlayer === req.body["PlayerID"];
}

function validateRoomStatus(req, res, rooms, inRoom, inGame, inFinal) {
    var room = getRoom(req, rooms);
    if (!validateRoomObject(res, room)) return false;

    if (room["inRoom"] !== inRoom || room["inGame"] !== inGame || room["inFinal"] !== inFinal) {
        res.status(403).send({ error: "Room is in incorrect state: " + 
            "inRoom: " +  room["inRoom"] + ", " +
            "inGame: " +  room["inGame"] + ", " +
            "inFinal: " +  room["inFinal"]
        });
        return false;
    }

    return true;
}

function getRoom(req, rooms) {
    var wantedRoomId = req.body["RoomID"];
    var roomIndex = rooms.findIndex(room => room["roomCode"] === wantedRoomId);

    if (roomIndex === -1) return null;

    return rooms[roomIndex];
}

function validateRoomObject(res, room) {
    if (room === null) {
        res.status(404).send({ error : "Room not found" });
        return false;
    }

    return true;
}

function getRandomRoomCode(currentRoomCodes) {
    return "1234"; // TEMP
    return Math.floor(Math.random()*10000).toString().padStart(4, "0");
}