import { AuthenticationContext, adalFetch, withAdalLogin } from 'react-adal';

// Determine Environment
let full = window.location.host;
let parts = full.split('.');
let subdomain = parts[0];
let hostname = subdomain.split(':')[0];
let envClientId = '81e95df6-44ee-4c9c-b4be-170787b4a611';

//if (hostname.includes('beta')) {
//   envClientId = '1649eb38-f96c-4d9c-b88b-2b5a99d229c8';
//}

// Configure AAD for Environment
export const endpoint = envClientId;

export const adalConfig: any = {
   tenant: 'pactcloud.onmicrosoft.com',
   clientId: envClientId,
   instance: 'https://login.microsoftonline.com/',
   endpoints: { api: endpoint },
   cacheLocation: 'localStorage'
};

export const authContext = new AuthenticationContext(adalConfig);

export const adalApiFetch = (url: any, options: any = {}) =>
   adalFetch(authContext, adalConfig.endpoints.api, fetch, url, options);

export const withAdalLoginApi = withAdalLogin(authContext, adalConfig.endpoints.api);

export const bearerToken = authContext.getCachedToken(adalConfig.clientId);

export const access_token = authContext.CONSTANTS.ACCESS_TOKEN

