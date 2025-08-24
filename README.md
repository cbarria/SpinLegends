# SpinLegends — Real‑time Multiplayer Spinning Tops (Unity + Photon)

Fast, phone‑friendly arena battles with networked spinning tops. Built with Unity and Photon PUN.

## Highlights
- Late‑join safe: master creates room‑owned players; new clients see everyone instantly
- Smooth collisions (linear + angular impact) and continuous respawn (carmageddon style)
- Mobile‑ready UI fixes (no stretching/zoom), touch joystick focus/recovery, Android debug overlay
- Floating health bars per player + optional top HUD health slider
- Lightweight status line: Connecting / Joining / Creating / Players: N
- Simple scoreboard (per‑player points on KO, now including score display)

## Recent Updates
- Fixed duplicate death registrations (debounce on RegisterDeath)
- Added score display to SimpleScoreboard without breaking existing kills/deaths
- Improved player name synchronization across clients (especially for master client)
- Enhanced disconnection handling: properly remove player objects and health bars
- General stability: reduced destruction delays, better event handling for explosions/sounds

## Quick Start
1) Unity 2022.3 LTS+, Photon PUN (set your AppId)
2) Open the scene at **Assets/Scenes/SpinLegends.unity**, press Play (PC) or Build & Run (Android)
3) Controls: Joystick to move, Spin to start, Jump to hop

## What’s in the box
- Networking
  - `NetworkManager`: connect, join/create room, master‑authoritative room spawns
  - `PlayerSpawnManager`: spawns room objects and transfers ownership to each actor
  - Robust late‑join and respawn flow (no duplicates)
- Gameplay
  - `PlayerController`: spin, movement, health, KO → request respawn
  - `NetworkPlayerCollision`: better side hits (relative velocity + angular contribution)
- UI
  - Floating `HealthBar` per player via `HealthBarManager`
  - Optional global `HealthSlider` (assign to `GameManager.healthBar`)
  - `ConnectionStatusUI`: compact status line (Connecting / Joining t/T / Creating / Players: N)
  - `ScoreManager` + `SimpleScoreboard`: simple KO score broadcast by master, now with score
  - Android overlay debug (optional) and joystick focus/recovery helpers

## Android Notes
- Keep Canvas in Screen Space Overlay; don’t force `Screen.SetResolution`
- If you use the global health slider on device, remove the Handle and set Transition=None
- Disable any auto‑setup scripts that recreate UI in runtime (e.g., `AutoSetup`)

## Structure (key files)
```
Assets/Scripts/
  Managers/
    NetworkManager.cs
    PlayerSpawnManager.cs
    GameManager.cs
    HealthBarManager.cs
    ScoreManager.cs
  Player/
    PlayerController.cs
    NetworkPlayerCollision.cs
  UI/
    HealthBar.cs
    ConnectionStatusUI.cs
    SimpleScoreboard.cs
```

## Credits
- Code & live fixes: CB + GPT‑assisted pairing (mixed authorship)
- Photon PUN by Exit Games

Enjoy and keep it spinning! ✨
