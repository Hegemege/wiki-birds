"use strict";

/**
 * Get package info and server port
 */
const pkg = require("../package.json");

/**
 * Environment configuration (production)
 */
module.exports = {
    //App
    APP_NAME: pkg.name,
    APP_VERSION: pkg.version,
    ENV: process.env.NODE_ENV,
    dev: {
        // API
        API_BASE_URL: "http://localhost:8081",
        API_BASE_PATH: "/api/",

        // Server
        SERVER_PORT: 8081,
        SERVER_TIMEOUT: 120000,
    },
    staging: {
        // API
        API_BASE_URL: "http://localhost:8081",
        API_BASE_PATH: "/api/",

        // Server
        SERVER_PORT: 8081,
        SERVER_TIMEOUT: 120000,
    },
    production: {
        // API

        // Server
        SERVER_PORT: process.env.PORT || 8080,
        SERVER_TIMEOUT: 120000,
    },
};
