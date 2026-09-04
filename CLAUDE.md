# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project state

The Unity project is scaffolded (Universal 3D/URP template) and Milestone 1 (player
movement + a find-the-item objective) is working. Milestone 2 (traversal) is in
progress, implemented as stairs (`CharacterController.stepOffset`) plus an
always-on jump — see "Development approach" below for how this diverges from the
README's original climb-button design. Do not jump ahead to later milestones
before earlier ones are solid.

Most of `Assets/Scripts/**` scene wiring (Player, camera, objective UI, pickup
item, test props) is done via one-click Editor tools in `Assets/Editor/` (`Mouse
Game` menu) rather than by hand — see `SETUP.md` for what each one builds and in
what order to run them. Prefer extending those tools over asking for manual
Inspector wiring, since re-running a tool is the update path being used here.

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

1. Player prototype — capsule with keyboard/mouse WASD look, move, sprint, jump, gravity, collision, falling/landing. Keep movement code modular so mobile input can be swapped in later. **Done** (`PlayerInputReader`/`PlayerMotor`/`PlayerLook`).
2. Traversal prototype — primitive test room (floor/walls/boxes/chair/table/shelf); jumping, falling, walking up low ledges. **Implemented differently than the README's original sketch**: instead of a dedicated climb button + `Climbable` component/raycast detection (tried, reverted — see git history around `StairsTestBuilder`/removed `PlayerClimb`), traversal is `CharacterController.stepOffset` auto-stepping up stair treads shorter than the offset, plus an unconditional jump (`PlayerMotor.jumpHeight`) that lands you on anything the arc reaches, same as any ordinary jump. No separate climb input exists. If a future need (e.g. mantling something taller than a jump can clear) brings back an explicit climb action, reintroduce it deliberately rather than assuming the old design is still there.
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
