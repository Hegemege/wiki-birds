let path = require("path");

function resolve(dir) {
    return path.join(__dirname, "..", dir);
}

module.exports = {
    entry: "./scripts/server.js",
    target: "node",
    node: {
        __dirname: false,
        __filename: false,
    },
    output: {
        path: resolve("dist"),
        filename: "api.js",
    },
};
