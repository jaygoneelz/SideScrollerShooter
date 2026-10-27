# Browser edition

A dependency-free Canvas 2D adaptation of the MonoGame game for static hosting. It shares the desktop textures, sound effects, music and world configuration, while its simulation is implemented in JavaScript.

Serve this directory through HTTP, not `file://`. Open `index.html`. The game supports keyboard and multi-touch controls, auto-pause on focus loss, sound enabled by the player, fullscreen where available, and a local top-ten table. Records are specific to the current browser/device and are not an online leaderboard.

`core.js` contains the deterministic simulation, collision responses, enemy FSM and score normalization. `game.js` owns Canvas rendering, DOM controls, audio and browser persistence. Runtime has no external JavaScript dependencies or network service integrations.

Run `node --test Web/tests/core.test.mjs` from the repository root for simulation checks. Deploy `index.html`, `style.css`, `game.js`, `core.js` and `assets/` together. The portfolio copy lives at `games/sidescroller-shooter/` in `jaygoneelz.github.io`.

Keyboard: WASD/arrows move, hold Space to fire, P pauses, Escape pauses/returns, M toggles audio and F3 toggles bounds. On touch devices, hold the direction pad and FIRE button. Hidden tabs and lost focus pause gameplay instead of consuming lives while away.
