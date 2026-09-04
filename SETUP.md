# MVP 1 Setup — Move Around a Room + Find the Item

This covers taking the repo from "just scripts" to a playable scene. Steps marked
**(Editor)** must be done by hand in the Unity Editor — Claude Code can't drive the
Editor GUI directly.

## 1. Install Unity & create the project (Editor)

Covered in [`UNITY_INSTALL_GUIDE.md`](./UNITY_INSTALL_GUIDE.md) — install Unity
Hub, install the Unity 6 LTS Editor with Android Build Support, then create the
`mouse-game` project pointed at this folder.

Once open, the Console should show `Assets/Scripts/...` importing with no errors
(there's nothing to wire up yet, so no errors expected here).

## 2. Build the test room (Editor)

In the default `SampleScene` (rename it to `Prototype` under `Assets/Scenes/` if you
like — matches the suggested project structure):

1. Delete the sample objects you don't need, keep the **Directional Light**.
2. **Floor:** GameObject → 3D Object → Plane. Scale to roughly `(3, 1, 3)` so it's a
   reasonably sized room floor.
3. **Walls:** GameObject → 3D Object → Cube, ×4. Scale/position them around the
   floor's edges to box the room in (thin, tall cubes). Exact dimensions don't
   matter for MVP.
4. Optionally add a couple of Cube "furniture" placeholders (box, table) sitting
   around — not required for movement-only MVP, but harmless to rough in now
   since Milestone 2 (climbing) will want them.

## 3-5. Player, objective UI, pickup item — automated

Instead of doing steps 3-5 by hand, use the `Mouse Game → Build MVP Scene (Player +
Objective)` menu item (from `Assets/Editor/MvpSceneBuilder.cs`). It builds the
`Player` (CharacterController + capsule body + camera + scripts), the `GameManager`
with the objective UI Canvas/text, and a `Cheese` pickup, wiring every Inspector
reference for you. It only touches those objects — build the room (step 2) by hand
first. Safe to re-run if you tweak scripts and want to re-wire.

The `Cheese` pickup lands near the world origin (`(1, 0.3, 1)`); drag it in the
Scene view afterward if that's outside your room or inside a wall.

The manual steps below are kept for reference (e.g. if you want to understand what
the tool did, or wire things up differently by hand).

## 3. Set up the player (Editor)

1. GameObject → Create Empty, name it `Player`. Position it above the floor,
   e.g. `(0, 1, 0)`.
2. Add components to `Player`:
   - **Character Controller** (leave defaults; radius ~0.5, height ~2 is fine for
     a placeholder capsule scale).
   - `PlayerInputReader` (from `Assets/Scripts/Input`)
   - `PlayerMotor` (from `Assets/Scripts/Player`)
   - Tag it **Player** (Inspector top-left Tag dropdown → Player; this is what
     `CollectibleItem` checks against).
3. For a visible capsule body: GameObject → 3D Object → Capsule as a **child** of
   `Player`, positioned so its center matches the CharacterController's center.
   Remove the Capsule's own Capsule Collider (the parent's Character Controller
   already handles collision) to avoid double colliders.
4. Add a **Camera** as a child of `Player`, positioned near the top of the capsule
   (e.g. local position `(0, 0.7, 0)`) to act as the eyes. Delete the old default
   `Main Camera` in the scene if this new one replaces it (keep only one camera
   tagged `MainCamera`).
5. On the Camera, add `PlayerLook` (from `Assets/Scripts/Player`):
   - **Player Body** → drag the `Player` root object in.
   - **Input** → drag the `Player` object in (it holds `PlayerInputReader`).

Press Play: WASD should move you, mouse should look around, Space should jump,
Left Shift should sprint.

## 4. Set up the objective UI (Editor)

1. GameObject → UI → Text (this auto-creates a Canvas + EventSystem if none
   exist yet). Rename the Text to `FoundMessageText`, set its text to
   `Found it!`, center it near the top of the screen, and note it isn't required
   to be visible by default.
2. Create an empty child under the Canvas named `FoundMessagePanel`, move the
   `FoundMessageText` under it (or just use the Text object itself as the panel
   if you'd rather skip the extra nesting).
3. Add an empty GameObject `GameManager` (or reuse an existing manager object) and
   add `ObjectiveUI` (from `Assets/Scripts/UI`) to it:
   - **Found Message Panel** → the panel/object to toggle.
   - **Found Message Text** → the `Text` component.
4. On the same or another manager object, add `ObjectiveManager` (from
   `Assets/Scripts/Game`):
   - **Objective UI** → drag the object holding `ObjectiveUI` in.

## 5. Set up the pickup item (Editor)

1. GameObject → 3D Object → Sphere (or Cube), name it `Cheese` (or `Ring`).
   Scale it down small (e.g. `0.2`), place it somewhere reachable in the room.
2. On its Collider, check **Is Trigger**.
3. Add `CollectibleItem` (from `Assets/Scripts/Interaction`):
   - **Objective Manager** → drag the object holding `ObjectiveManager` in.

## 6. Verify

Press Play:
- Move with WASD, look with the mouse, jump with Space, sprint with Left Shift.
- Walk into the pickup item — it should disappear and "Found it!" should appear
  on screen (and log to the Console).

That's the full MVP loop: move around a room, find the item.

## Milestone 2 — Traversal / climbing

Use **Mouse Game → Build Traversal Test (Milestone 2)** (from
`Assets/Editor/TraversalTestBuilder.cs`, run after "Build MVP Scene"). It:

- Adds `PlayerClimb` (from `Assets/Scripts/Player`) to the existing `Player`.
- Adds a "Press E to climb" prompt (wired to the objective Canvas).
- Adds two `Climbable`-tagged test props near `(-1.5, *, 0..1)`: `ClimbBox` (0.5m
  tall) and `ClimbTable` (1m tall, staggered so you can climb the box, then the
  table from on top of it).

Drag `ClimbBox`/`ClimbTable` into your room if that position is outside your walls
or inside another object.

**How it works:** `PlayerClimb` raycasts forward at roughly waist height; if it
hits a collider with a `Climbable` component, it probes downward from above to
find the ledge's surface height. If that height is between 0.35m and 1.5m above
your feet and there's clear space to stand, it shows the prompt — press **E** to
slide up onto it over ~0.3s.

To make any other object (a chair, a shelf, custom furniture) climbable later,
just add the `Climbable` component (from `Assets/Scripts/Environment`) to it —
no other wiring needed.

**Test:** walk up to `ClimbBox`, wait for "Press E to climb", press **E** — you
should slide up onto it. Then walk to `ClimbTable`'s edge and climb again. If the
prompt doesn't appear, you're either too far/too close, at the wrong height, or
not facing it squarely — these detection numbers (`wallCheckDistance`,
`minLedgeHeight`, `maxLedgeHeight` on `PlayerClimb`) are easy to tune in the
Inspector once you see how it feels.

Also re-run **Build MVP Scene** once (safe/idempotent) after pulling this update —
it now enforces `PlayerMotor.jumpHeight = 0.3` on the existing `Player`, so a
plain jump can no longer clear the climbable ledges and skip the climb system
entirely.

**Debugging the detection:** select `Player` in the Hierarchy while in Play mode.
`PlayerClimb` draws Scene-view gizmos for its raycasts — yellow/red line for the
forward wall check, cyan/magenta line for the downward ledge probe, green sphere
when a valid ledge is found (red if found but blocked). It also logs the reason
to the Console once per state change (`debugLogging` on the component, on by
default) — e.g. "hit 'ClimbBox' but it has no Climbable component" tells you
immediately what to fix.
