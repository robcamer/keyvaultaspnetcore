"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var react_adal_1 = require("react-adal");
// Determine Environment
var full = window.location.host;
var parts = full.split('.');
var subdomain = parts[0];
var hostname = subdomain.split(':')[0];
var envClientId = '81e95df6-44ee-4c9c-b4be-170787b4a611';
//if (hostname.includes('beta')) {
//   envClientId = '1649eb38-f96c-4d9c-b88b-2b5a99d229c8';
//}
// Configure AAD for Environment
exports.endpoint = envClientId;
exports.adalConfig = {
    tenant: 'pactcloud.onmicrosoft.com',
    clientId: envClientId,
    instance: 'https://login.microsoftonline.com/',
    endpoints: { api: exports.endpoint },
    cacheLocation: 'localStorage'
};
exports.authContext = new react_adal_1.AuthenticationContext(exports.adalConfig);
exports.adalApiFetch = function (url, options) {
    if (options === void 0) { options = {}; }
    return react_adal_1.adalFetch(exports.authContext, exports.adalConfig.endpoints.api, fetch, url, options);
};
exports.withAdalLoginApi = react_adal_1.withAdalLogin(exports.authContext, exports.adalConfig.endpoints.api);
exports.bearerToken = exports.authContext.getCachedToken(exports.adalConfig.clientId);
exports.access_token = exports.authContext.CONSTANTS.ACCESS_TOKEN;
//# sourceMappingURL=adalConfig.js.map