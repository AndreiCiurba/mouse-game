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

## Milestone 2 — Traversal (stairs + free jump)

No custom climb script — this version leans entirely on
`CharacterController.stepOffset` (Unity's built-in "walk up small steps
automatically" behavior) plus a normal, always-available jump.

First, re-run **Mouse Game → Build MVP Scene (Player + Objective)** (safe to
re-run) — it now:
- Sets `Player`'s `CharacterController.stepOffset = 0.3` explicitly.
- Sets `PlayerMotor.jumpHeight = 0.9` (a real, always-on jump — no gating).
- Cleans up the old E-climb prototype's leftovers (`ClimbBox`/`ClimbTable`/
  `ClimbPromptText` objects and any now-missing `PlayerClimb`/`ClimbPromptUI`
  script references on `Player`/`GameManager`).

Then use **Mouse Game → Build Stairs Test (Milestone 2)** (from
`Assets/Editor/StairsTestBuilder.cs`) to add a 5-step staircase (0.2m per step,
under the 0.3m step offset) leading up to a landing platform, near
`(-1.5, *, 0..~2.8)`. Drag the whole `Stair01..05` + `StairLanding` group into
your room if it lands outside your walls.

**How it works:** nothing to detect or trigger — just walk into the stairs and
`CharacterController` steps you up each tread as part of normal movement,
carrying you onto the landing once the steps end. Jump works everywhere, all the
time; whether it lands you on top of something is just physics (your jump arc
either reaches a surface or it doesn't, same as any normal jump) — no separate
climb button or ledge-detection logic involved anymore.

**Test:** walk into the stairs — you should rise up each step automatically with
no key press, and continue onto the landing at the top. Separately, try jumping
(Space) at things around the room — it should work everywhere, and land you on
top of anything short enough for the jump arc to clear.
