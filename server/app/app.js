const express = require("express");
const bodyParser = require("body-parser");
const low = require("lowdb");
const FileAsync = require("lowdb/adapters/FileAsync");
const fs = require("fs");
const path = require("path");
const uuidv4 = require('uuid/v4');
const cors = require('cors')

// Hardcoded token to cut away bots crawling semi-sensitive information
// Does not secure anything, just makes sure that "most" of the traffic to the public API
// is originated from the right source (the game). The other option is to authenticate users, overkill
const token = "9QfdXsTwmOPySh1zaB8A";

module.exports = function() {
    const app = express();
    app.use(bodyParser.json());
    app.use(cors());

    // Create database instance and start server
    const adapter = new FileAsync("db.json");

    // API routes

    // Main api page, acts as a ping endpoint
    app.get("/api", function(req, res) {
        res.setHeader("Content-Type", "text/html; charset=utf-8");
        res.end("Wikibirds API");
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
