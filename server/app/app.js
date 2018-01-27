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
        this.players = [new Player(getRandomPlayerName(), hostPlayer)];
        this.inRoom = true;
        this.inGame = false;
        this.inFinal = false;
        this.startTime = (new Date()).getTime();
        this.gameState = {};
        this.phoneLines = [];
    }
}

class Player {
    constructor(name, playerId) {
        this.playerId = playerId;
        this.name = name;
        this.color = getRandomPlayerColor();
    }
}

// Gamestate
/*
player
    phoneLineIndex
    xPosition


*/

const PlayerNames = [
    "Birdy Mc Birdface",
    "Mr. Tweet",
    "Ms. Songbird",
    "Birdy Mc Birdface 2",
    "Mr. Tweet 2",
    "Ms. Songbird 2",
    "Birdy Mc Birdface 3",
    "Mr. Tweet 3",
    "Ms. Songbird 3"
];

const PlayerColors = [
    "Blue", 
    "Yellow",
    "Red", 
    "Green"
];

// Hardcoded token to cut away bots crawling semi-sensitive information
// Does not secure anything, just makes sure that "most" of the traffic to the public API
// is originated from the right source (the game). The other option is to authenticate users, overkill
const token = "9QfdXsTwmOPySh1zaB8A";

const MIN_PLAYER_COUNT = 2;
const MAX_PLAYER_COUNT = 4;

module.exports = function() {
    const app = express();
    app.use(bodyParser.json());
    app.use(cors());

    // Create database instance and start server
 //   const adapter = new FileAsync(path.resolve(__dirname, "db.json"));

    // Room servicing
    var rooms = [];

    // Clear old rooms every 2.5 minutes
    // Avoids a memory leak over time
    setInterval(() => {
        var now = (new Date()).getTime();
        rooms = rooms.filter(room => now - room["lastUpdateTimeStamp"] < 5*60*1000);
        rooms = rooms.filter(room => room.players.length > 0);
    }, 2.5*60*1000);

    // API routes

    // Main api page, acts as a ping endpoint
    app.get("/api", function(req, res) {
        res.setHeader("Content-Type", "application/json");
        res.status(200).send({ message: "success" });
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

        res.status(200).send({ message: "success", roomCode: newRoomCode, playerName: newRoom["players"][0]["name"], playerColor: newRoom["players"][0]["color"] });

        console.log("Player " + hostPlayerID + " created room " + newRoomCode);
    });

    app.put("/api/join-room", function(req, res) {
        if (!validateToken(req, res)) return;
        if (!validate(req, res, "PlayerID", false)) return;
        if (!validate(req, res, "RoomID", false)) return;

        if (!validateRoom(req, res, rooms)) return;

        if (!validateRoomStatus(req, res, rooms, true, false, false)) return;

        var wantedRoomId = req.body["RoomID"];
        var roomIndex = rooms.findIndex(room => room["roomCode"] === wantedRoomId);
        var playerId = req.body["PlayerID"];

        // If player is already in room
        if (rooms[roomIndex].players.findIndex(player => player["playerId"] === playerId) !== -1) {
            res.status(400).send({ error: "Player " + playerId + " already in room " + wantedRoomId});
            return;
        }

        var room = getRoom(req, rooms);

        if (room.hostPlayer === null) {
            res.status(400).send({ error: "Room abandoned. "});
            return;
        }

        // Add player to room
        var newPlayer = new Player(getRandomPlayerName(room), playerId);
        rooms[roomIndex].players.push(newPlayer);

        res.status(200).send({ message: "success", playerName: newPlayer["name"], playerColor: newPlayer["color"] })

        console.log("Player " + playerId + " joined room " + wantedRoomId + " (total " + rooms[roomIndex].players.length + " players in room)");
    });

    app.put("/api/leave-room", function(req, res) {
        if (!validateToken(req, res)) return;
        if (!validate(req, res, "PlayerID", false)) return;
        if (!validate(req, res, "RoomID", false)) return;

        if (!validateRoom(req, res, rooms)) return;

        if (!validatePlayerInRoom(req, res, rooms, req.body["RoomID"])) return;

        if (!validateRoomStatus(req, res, rooms, true, false, false)) return;

        var room = getRoom(req, rooms);

        // Remove player from room
        var playerId = req.body["PlayerID"];
        room.players = room.players.filter(player => player["playerId"] !== playerId);

        if (playerId === room.hostPlayer) {
            if (room.players.length > 0) {
                room.hostPlayer = room.players[0]["playerId"];
            } else {
                room.hostPlayer = "";
            }
        }

        res.status(200).send({ message: "success" })

        console.log("Player " + playerId + " left room " + room["roomCode"] + " (total " + room.players.length + " players in room)");
    });

    app.put("/api/room-info", function(req, res) {
        if (!validateToken(req, res)) return;
        if (!validate(req, res, "PlayerID", false)) return;
        if (!validate(req, res, "RoomID", false)) return;

        if (!validateRoom(req, res, rooms)) return;

        if (!validatePlayerInRoom(req, res, rooms, req.body["RoomID"])) return;

        // If room is no longer in room state, tell client to go to game
        if (validateRoomStatus(req, res, rooms, false, true, false, false)) {
            var room = getRoom(req, rooms);
            res.status(200).send({ message: "started", startTime: getSetStartTime(room) });
            return;
        }

        if (!validateRoomStatus(req, res, rooms, true, false, false)) return;

        var room = getRoom(req, rooms);

        // Build an object representing the room data

        var data = {
            players: room.players.map(player => player["name"]),
            host: room.hostPlayer,
            roomCode: room.roomCode
        }

        res.status(200).send({ message: "success", "data": data });
    });

    app.put("/api/start-room", function(req, res) {
        if (!validateToken(req, res)) return;
        if (!validate(req, res, "PlayerID", false)) return;
        if (!validate(req, res, "RoomID", false)) return;

        if (!validateRoom(req, res, rooms)) return;
        if (!validateRoomOwner(req, res, rooms)) return;
        if (!validateRoomStatus(req, res, rooms, true, false, false)) return;

        var room = getRoom(req, rooms);

        if (room.players.length < MIN_PLAYER_COUNT) {
            res.status(400).send({ error: "Too few players in room. Minimum " + MIN_PLAYER_COUNT });
            return;
        }

        if (room.players.length > MAX_PLAYER_COUNT) {
            res.status(400).send({ error: "Too many players in room. Maximum " + MAX_PLAYER_COUNT });
            return;
        }

        // Update room status
        room["inRoom"] = false;
        room["inGame"] = true;

        res.status(200).send({ message: "room started" });

        console.log("Room " + room["roomCode"] + " was started (" + room.players.length + " players)");
    });

/*
    // API routes that touch the DB
    low(adapter)
        .then(db => {

            // define routes that need db connection

            // Set db default values
            return db.defaults({ todo: [] }).write();
        });
*/
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
    if (room.players.findIndex(player => player["playerId"] === playerId) === -1) {
        res.status(400).send({ error: "Player " + playerId + " not in room " + room["roomCode"]});
        return false;
    }

    return true;
}

function validateRoomOwner(req, res, rooms) {
    var room = getRoom(req, rooms);
    if (!validateRoomObject(res, rooms)) return false;

    if (room.hostPlayer !== req.body["PlayerID"]) {
        res.status(400).send({ error: "Player " + req.body["PlayerID"] + " is not the owner of room " + room["roomCode"]});
        return false;
    }

    return true;
}

function validateRoomStatus(req, res, rooms, inRoom, inGame, inFinal, sendResponse = true) {
    var room = getRoom(req, rooms);
    if (!validateRoomObject(res, room)) return false;

    if (room["inRoom"] !== inRoom || room["inGame"] !== inGame || room["inFinal"] !== inFinal) {
        if (sendResponse) {
            res.status(400).send({ error: "Room is in incorrect state: " + 
                "inRoom: " +  room["inRoom"] + ", " +
                "inGame: " +  room["inGame"] + ", " +
                "inFinal: " +  room["inFinal"]
            });
        }
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
    return String("0000" + Math.floor(Math.random()*10000).toString()).slice(-4);
}

function getRandomPlayerName(room = null) {
    if (room === null) {
        return PlayerNames[Math.floor(Math.random() * PlayerNames.length)];
    }

    var playerNames = room["players"].map(player => player["name"]);
    var chosenName = "";

    if (room.players.length >= PlayerNames.length) {
        return "Player " + (Math.floor(Math.random() * 100) + 1).toString();
    }

    // Quick and dirty random name excluding the given names
    do {
        chosenName = PlayerNames[Math.floor(Math.random() * PlayerNames.length)];
    } while (playerNames.indexOf(chosenName) !== -1)

    return chosenName;
}

function getRandomPlayerColor(room = null) {
    if (room === null) {
        return PlayerColors[Math.floor(Math.random() * PlayerColors.length)];
    }

    var playerColors = room["players"].map(player => player["color"]);
    var chosenColor = "";

    if (room.players.length >= PlayerColors.length) {
        console.log("Too many players in room.");
        return PlayerColors[0];
    }

    do {
        chosenColor = PlayerColors[Math.floor(Math.random() * PlayerColors.length)];
    } while (playerColors.indexOf(chosenColor) !== -1)

    return chosenColor;
}

function getSetStartTime(room) {
    if (!room) {
        return;
    }

    room["startName"] = (new Date()).getTime() + 10000;
    return room["startName"];
}