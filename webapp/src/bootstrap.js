import './main.css';
import createApp from '@shopify/app-bridge';
import { getSessionToken } from "@shopify/app-bridge/utilities";
import { Elm } from './Main.elm';

// const appBridge = createApp({
//     apiKey: '', // looper app
//     host: new URLSearchParams(location.search).get('host'),
//     forceRedirect: true
// });

const app = Elm.Main.init({
    flags: window.location.origin
});

// app.ports.requestSessionToken.subscribe(async () => {
//     const sessionToken = await getSessionToken(appBridge);
//     app.ports.sessionToken.send(sessionToken);
// });
