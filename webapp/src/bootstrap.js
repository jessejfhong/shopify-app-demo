import './main.css';
import { Elm } from './Main.elm';

const app = Elm.Main.init({
    flags: window.location.origin
});
