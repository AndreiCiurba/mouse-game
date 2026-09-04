# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project state

The Unity project is scaffolded (Universal 3D/URP template) and all 6 of the
README's numbered milestones plus the "First Complete Level" (kitchen) are built —
see "Development approach" below for how each one diverges from the README's
original sketch (climb button replaced with stairs, primitive placeholders instead
of Blender models, NavMesh baked at Humanoid scale, etc.). Most of it is untested
or only lightly tested; treat "built" as "implemented, not "verified."

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
2. Traversal prototype — primitive test room (floor/walls/boxes/chair/table/shelf); jumping, falling, walking up low ledges. **Done, implemented differently than the README's original sketch**: instead of a dedicated climb button + `Climbable` component/raycast detection (tried, reverted — see git history around `StairsTestBuilder`/removed `PlayerClimb`), traversal is `CharacterController.stepOffset` auto-stepping up stair treads shorter than the offset, plus jump (`PlayerMotor.jumpHeight`, up to `maxJumps` — currently 2, i.e. a double jump — before an actual ground touch is required again) that lands you on anything the arc reaches. No separate climb input exists. Grounding uses a manual `Physics.CheckSphere` in `PlayerMotor.CheckGrounded()`, not `CharacterController.isGrounded` — the built-in flickers false on open flat ground (see git history: this caused "jump only works near geometry" and, when the check was too generous, an unintended infinite-jump exploit). If a future need (e.g. mantling something taller than a jump can clear) brings back an explicit climb action, reintroduce it deliberately rather than assuming the old design is still there.
3. Mobile controls — left-side virtual joystick (movement), right-side touch drag (camera), buttons for jump/sprint (no climb button — see #2). **Done, implemented via Unity UI's pointer event interfaces** (`IPointerDownHandler`/`IDragHandler`/etc. on `VirtualJoystick`/`TouchLookArea`/`TapButton`/`HoldButton` in `Assets/Scripts/Input`), not the `com.unity.inputsystem` Action-asset API — the UI event system already unifies mouse (Editor/Device Simulator) and real touch for free, so a lower-level input API wasn't needed. `PlayerInputReader` merges these with keyboard/mouse into the same `Move`/`Look`/`JumpPressed`/`SprintHeld` values `PlayerMotor`/`PlayerLook` already consume — neither of those scripts changed at all for this milestone.
4. Mouse character — low-poly mouse modeled in Blender (body, head, ears, nose, eyes, tail), imported into Unity. Still first-person; the mouse body need not be fully visible on-screen. Keep it visually small relative to the environment. **Done, with a placeholder instead of a real Blender model** (no Blender access here): `MouseModelBuilder` (Editor tool) assembles body/head/ears/nose/eyes/tail from primitives under `Player/MouseModel`, replacing the plain capsule. The eventual real model can just replace that method's contents (same parent/scale contract) once someone models it. This milestone also rescaled the `Player` to actual mouse size (`CharacterController` height `0.2`, radius `0.06`, `skinWidth` explicitly set to `radius * 0.1` — the default `0.08` skin width exceeds this radius and makes the controller behave very oddly) — walk/sprint/jump/gravity/ground-check values and the stairs (`StairsTestBuilder`) were all rescaled to match. The room (`RoomTestBuilder`) was deliberately left at its original human-scale dimensions — that's what makes the mouse look small "relative to the environment," not shrinking everything together.
5. Cat AI — NavMesh-based enemy with a state machine: Idle → Patrol → Sees/Hears Player → Chase → (player escapes) Search → (found again) Chase → (caught) Game Over. Keep states modular (Idle, Patrol, InvestigateNoise, Chase, Search, Attack) for future expansion. No machine learning. **Done, built via `CatAI`/`CatVision`/`CatHearing`/`GameOverManager` (`Assets/Scripts/AI`, `Assets/Scripts/Game`) + the `Build Cat AI` Editor tool**. States are per-method inside one `CatAI` component (an enum switch), not separate state classes/objects — simplest version that still keeps states distinct; promote to a real state-object pattern only if this switch gets unwieldy. Patrol/chase speeds are kept below `PlayerMotor`'s walk/sprint speeds on purpose — the cat should never out-move a moving player, only catch one that stands still or gets cornered. The NavMesh is baked with Unity's default "Humanoid" agent type (radius 0.5) rather than a custom cat-sized one — registering a new agent type has no reliable Editor-script API, and the room being human-scale already comfortably contains a cat-sized `NavMeshAgent` (radius/height set directly on the agent); the cat just won't hug walls as tightly as a true cat-scale bake would allow. `HidingSpotsBuilder` scatters solid cover blocks (taller than `CatVision`'s eye point) around the room — `CatVision` already treats any solid collider as blocking line of sight, so no extra script was needed for that.
6. Sound/stealth system — noise levels (walking = quiet, sprinting/jumping/landing = noticeable, knocking a prop = loud); cats detect noise within a radius and investigate. **Done, including "knocking a prop"**: `PlayerMotor` exposes `Jumped`/`Landed` events; `NoiseEmitter` (on `Player`) turns movement state into `CurrentNoiseRadius` (0 when idle, small when walking, larger when sprinting or on a brief jump/land pulse) and exposes `EmitNoise()`. `CatHearing.CanHearPlayer` reads that radius instead of the flat proximity check it started as. `PropKnocker` (on `Player`, added by `Build Knockable Props`) detects `OnControllerColliderHit` against a `KnockableProp` and calls `EmitNoise()` with that prop's (louder) radius — the hook existed before anything called it; this is what finally does. This noise mechanic is silent by design (a gameplay radius, not audio) — separately, `PlayerAudio` (also on `Player`) plays actual placeholder SFX on the `Jumped`/`Landed` events via `MouseGame.Audio.ProceduralAudio`, which generates short tones/noise-bursts in code since no real audio asset files exist in the project. Same "primitives until real content" reasoning as the models, applied to audio.

First complete level: one kitchen room, objective "steal the cheese and escape," traversal path from under a cabinet → box → chair → table → countertop, avoiding the cat, with win/lose states. **Done, built via `KitchenLevelBuilder`**: places the furniture path (each hop's height gap exceeds `stepOffset`, so these are genuine jumps, not auto-climbed steps like the stairs test), moves `Cheese` onto the countertop, repositions `Cat` to guard the path, and adds `EscapeZone`/`LevelCompleteManager`/`LevelCompleteUI` (`Assets/Scripts/Interaction`, `Assets/Scripts/Game`, `Assets/Scripts/UI`) — the win-state mirror of `GameOverManager`/`GameOverUI`. `EscapeZone` is a no-op trigger until `ObjectiveManager.ObjectiveComplete` is true, so reaching the cheese alone doesn't end the level; the player has to make it back. This coexists with (doesn't replace) the generic test props (`StairsTestBuilder`, `HidingSpotsBuilder`) elsewhere in the room — those stay useful for isolated mechanic testing.

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
