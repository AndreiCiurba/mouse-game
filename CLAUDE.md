# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project state

This repository currently contains only `README.md` (the design/spec doc) — no Unity project has been scaffolded yet. The immediate task is **Milestone 1**: a basic first-person player controller (capsule) in a simple Unity test scene, built with clean architecture that can later support mobile input, climbing, the mouse character, and cat AI. Do not jump ahead to later milestones before earlier ones are working.

## What this project is

An **Android-only first-person 3D game** (Unity 6, C#) where the player is a tiny mouse exploring a human house — walking/sprinting/jumping/climbing over furniture, avoiding cats that hunt via a patrol/chase/search state machine, with a lightweight noise-based stealth mechanic. First target is a single playable kitchen level: traverse from the floor up to a countertop, grab cheese, escape, while avoiding a cat.

Tech stack: Unity 6, C#, Blender (custom models), Unity Input System, Unity CharacterController, Unity NavMesh, Unity Animator, Android build target.

**Do not introduce a backend, database, networking, Firebase, AWS, or any cloud service** — this is a fully local/offline prototype.

## Build/test commands

No build system exists yet (no `.sln`/`.csproj`/Unity project files present). Once the Unity project is scaffolded:
- Building and running happens through the Unity Editor (Play mode) — there is no CLI build step to run for iteration.
- Verify compilation after each major milestone (Unity Editor console must show no compile errors) before moving on.
- Test on an actual Android device frequently once mobile input (Milestone 3) is in place — the Editor's keyboard/mouse controls and touch controls must stay behind a shared input abstraction, not diverge.

## Development approach (from README)

Work through milestones **in order**, keeping a playable build at every stage:

1. Player prototype — capsule with keyboard/mouse WASD look, move, sprint, jump, gravity, collision, falling/landing. Keep movement code modular so mobile input can be swapped in later.
2. Traversal prototype — primitive test room (floor/walls/boxes/chair/table/shelf); jumping, climbing/mantling, falling. Climbing starts simple: detect climbable surface → verify valid destination → show climb indication → move player onto surface. Use a `Climbable` component/tag. Do not build complex Assassin's Creed-style climbing.
3. Mobile controls — Unity Input System, left-side virtual joystick (movement), right-side touch drag (camera), buttons for jump/climb/sprint. Input must be abstracted so keyboard and mobile feed the same player controller.
4. Mouse character — low-poly mouse modeled in Blender (body, head, ears, nose, eyes, tail), imported into Unity. Still first-person; the mouse body need not be fully visible on-screen. Keep it visually small relative to the environment.
5. Cat AI — NavMesh-based enemy with a state machine: Idle → Patrol → Sees/Hears Player → Chase → (player escapes) Search → (found again) Chase → (caught) Game Over. Keep states modular (Idle, Patrol, InvestigateNoise, Chase, Search, Attack) for future expansion. No machine learning.
6. Sound/stealth system — noise levels (walking = quiet, sprinting/jumping/landing = noticeable, knocking a prop = loud); cats detect noise within a radius and investigate.

First complete level: one kitchen room, objective "steal the cheese and escape," traversal path from under a cabinet → box → chair → table → countertop, avoiding the cat, with win/lose states.

## Architecture

Prefer composition over monolithic classes — do not put all gameplay logic into one MonoBehaviour.

```
GameManager
    ├── PlayerController
    │      ├── Movement
    │      ├── Camera
    │      ├── Jump
    │      ├── Climb
    │      └── Input
    ├── CatAI
    │      ├── StateMachine
    │      ├── Navigation
    │      ├── Vision
    │      └── Hearing
    ├── InteractionSystem
    ├── ObjectiveSystem
    ├── AudioSystem
    └── UI
```

Suggested `Assets/` layout (create as needed, don't pre-build unused folders):

```
Assets/
├── Art/{Models,Materials,Textures,Animations}/
├── Audio/
├── Prefabs/{Player,Enemies,Environment,Props}/
├── Scenes/{Prototype,Kitchen}/
├── Scripts/{Player,AI,Interaction,Environment,Game,Input}/
├── UI/
└── ScriptableObjects/
```

## Art direction

Stylized low-poly aesthetic. Priority order: gameplay > readability > performance > consistent art style > visual polish. Use Unity primitives/placeholders until gameplay works; save Blender effort for assets that matter (the mouse, key props). Don't spend significant time on detailed assets during prototyping.

## Android performance

Target ~60 FPS on a mid-range Android device. Watch polygon count, texture sizes, draw calls, lighting/shadows, physics objects, particles, memory, and post-processing. Use the Unity Profiler; avoid unnecessary expensive effects.

## Working conventions for this repo

- Inspect existing project structure and reuse existing systems before adding new ones.
- Keep components modular; avoid unnecessary dependencies.
- Prefer simple implementations first, build incrementally, and don't build the complete game in one pass.
- Android is the primary target throughout, even when iterating in-Editor with keyboard/mouse.
- Document important setup/configuration steps as they're introduced (e.g., Input System action maps, NavMesh bake settings, Android build settings).
