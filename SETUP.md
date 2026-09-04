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

## 2-5. Room, player, objective UI, pickup item — automated

Everything is built by one-click Editor tools now (`Assets/Editor/*.cs`, under the
**Mouse Game** menu). Run them in this order — each saves the scene to disk when
it finishes, and each is safe to re-run (idempotent: re-running just re-syncs
values, it won't create duplicates):

1. **Mouse Game → Build Test Room** — floor + 4 walls spanning roughly
   `x: -4..4, z: -4..4`, sized to enclose everything the other tools place.
2. **Mouse Game → Build MVP Scene (Player + Objective)** — `Player`
   (CharacterController + capsule body + camera + scripts), `GameManager` with the
   objective UI Canvas/text, and a `Cheese` pickup near `(1, 0.3, 1)`.
3. **Mouse Game → Build Stairs Test (Milestone 2)** — a 5-step staircase near
   `(-1.5, *, 0..~2.8)` (see below).

If you reposition anything by hand afterward (dragging in the Scene view), press
**Ctrl+S** — the tools only auto-save at the moment they run, not on later manual
edits.

The manual steps below are kept for reference (e.g. if you want to understand what
the tools did, or wire things up differently by hand).

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
automatically" behavior, set to `0.3` by `Build MVP Scene`) plus a normal,
always-available jump (`PlayerMotor.jumpHeight = 0.9`, no gating). Built by
**Mouse Game → Build Stairs Test (Milestone 2)** (step 3 above): a 5-step
staircase (0.2m per step, under the 0.3m step offset) leading up to a landing
platform.

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
