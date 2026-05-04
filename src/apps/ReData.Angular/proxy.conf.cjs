const apiTarget =
  process.env.services__redata_demoapp__http__0 ??
  process.env["services__redata-demoapp__http__0"] ??
  process.env.services__redata_demoapp__https__0 ??
  process.env["services__redata-demoapp__https__0"] ??
  "http://localhost:5223";

module.exports = {
  "/api": {
    target: apiTarget,
    secure: false,
    logLevel: "debug",
    pathRewrite: {
      "^/api": "/api"
    }
  }
};
