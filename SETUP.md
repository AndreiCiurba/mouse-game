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
(Space, twice in a row for the double jump) at things around the room — a third
press should do nothing until you land.

## Milestone 3 — Mobile touch controls

Built via Unity UI's pointer events (`IPointerDownHandler`/`IDragHandler`/etc.),
not the `com.unity.inputsystem` Action-asset API — the UI event system already
treats mouse and real touch the same way, so this covers both without extra
platform-specific code. See `Assets/Scripts/Input/{VirtualJoystick,
TouchLookArea, TapButton, HoldButton}.cs`. `PlayerInputReader` merges these with
keyboard/mouse into the same values it always exposed — `PlayerMotor` and
`PlayerLook` did not change at all for this milestone.

1. Run **Mouse Game → Build Mobile Controls (Milestone 3)** (after the earlier
   build steps) — adds a left-side joystick, a right-side drag-to-look area, and
   Jump/Sprint buttons (bottom-right) to the existing Canvas, wires them into
   `Player`'s `PlayerInputReader`, and locks **Player Settings → Resolution and
   Presentation → Default Orientation** to Landscape (the whole layout assumes
   landscape; Unity defaults new projects to Portrait, which is why the Device
   Simulator's rotate button otherwise appears to do nothing — a Portrait-locked
   app doesn't rotate on a real device either, and the Simulator matches that).
2. Open **Window → General → Device Simulator** — it renders the Game view as a
   phone screen and turns your mouse into a simulated touch, so you can test
   without a build or a physical device. It should already show landscape; use
   its device dropdown/rotate control if it doesn't.
3. Press Play with the Device Simulator tab active/docked next to Game.

**Test:** drag the left joystick to move, drag anywhere on the right side to
look around, tap Jump, hold Sprint while moving. Keyboard/mouse should *also*
still work at the same time (nothing about Milestone 1/2 controls changed).

**Known rough edges to expect and tune by feel** (all plain fields on the
relevant component, no code restructuring needed):
- Touch-look sensitivity (`PlayerInputReader.touchLookSensitivity`) was picked
  without live testing — likely needs adjusting once you can feel it.
- `HoldButton` releases Sprint on `OnPointerExit` (finger/cursor sliding off the
  button) to avoid a stuck-on bug — this can feel twitchy with a small button;
  enlarge the button or remove that behavior if it's annoying in practice.
- The joystick/button visuals are Unity's built-in placeholder sprite
  (`UI/Skin/Knob.psd`) with transparency — functional, not final art.

Once it feels good on the Device Simulator, the real test is an actual Android
device/build — see `UNITY_INSTALL_GUIDE.md` for Android Build Support (already
installed) and switch **File → Build Settings → Android** when ready for that.

## Milestone 4 — Mouse character + true scale

No Blender access here, so the mouse is a placeholder assembled from primitives
(`Assets/Editor/MouseModelBuilder.cs`) rather than an imported model — same
"primitives until gameplay works" approach as everything else. Swapping in a
real Blender-modeled mouse later just means replacing that method's contents;
nothing else needs to change.

This milestone also rescaled the `Player` to actual mouse size — a real change
in feel, not just visuals:
- `CharacterController`: height `0.2`, radius `0.06` (was human-scale: height
  `2`, radius `0.5`).
- `PlayerMotor`: walk/sprint/jump/gravity/ground-check values all rescaled to
  match (see the field defaults/tooltips in the script).
- Stairs (`StairsTestBuilder`) rescaled proportionally so they're still
  climbable under the new (smaller) step offset.
- The **room stayed the same absolute size** on purpose — that's what makes the
  mouse look small "relative to the environment": a normal-sized room is
  already huge next to a real mouse, so shrinking the room too would defeat the
  point. Only the character (and things sized directly against it, like the
  stairs and the cheese) got smaller.

1. Run **Mouse Game → Build MVP Scene (Player + Objective)** again — rescales
   the existing `Player`/`Cheese` and builds the new `MouseModel` in place of
   the old capsule.
2. Run **Mouse Game → Build Stairs Test (Milestone 2)** again — rebuilds the
   stairs at the new proportional size (old ones, if still in the scene, will
   now look absurdly oversized — delete them if `Build Stairs Test` doesn't
   clean them up for you).
3. Mobile controls (`Build Mobile Controls`) don't need re-running — the touch
   UI is all screen-space, unaffected by world scale.

**Test:** press Play. You should see a small primitive mouse shape instead of a
plain capsule, clearly tiny against the room's walls/floor. Walking/jumping/
stairs/sprint should all still work, just at mouse-appropriate speed and
jump height (much smaller numbers than before — that's intentional). If
movement feels too slow/fast or the camera height looks off, `PlayerMotor` and
the camera's local position (in `MvpSceneBuilder`) are the tunable spots.

## Milestone 5 — Cat AI

State machine: **Idle → Patrol → (sees/hears player) → Chase → (loses player)
→ Search → (times out) → Patrol**, or **Chase → (catches player) → Game
Over**. Seeing the player always wins and jumps straight to Chase from any
state. See `Assets/Scripts/AI/{CatAI,CatVision,CatHearing}.cs` and
`Assets/Scripts/Game/GameOverManager.cs`.

`CatHearing` is currently a flat proximity check (not real noise levels) —
that's Milestone 6's job; it's a deliberate stand-in so the state machine has
somewhere to plug hearing in now.

1. Run **Mouse Game → Build Cat AI (Milestone 5)** (after Build MVP Scene and
   Build Test Room) — bakes a NavMesh over the room, builds `Cat` (NavMeshAgent
   + vision/hearing/state machine + a primitive placeholder model, same
   "primitives until Blender" approach as the mouse) near `(2, 0, -2)`, and
   wires up the Game Over UI/manager to freeze the player and show a message
   on catch.
2. Press Play.

**Test:** the cat should idle briefly, then wander to random nearby points
(Patrol). Walk into its vision cone (in front of it, within range) — it should
turn and chase. Get caught (get close while it's chasing) — movement should
freeze and "Caught! Game Over" should appear. Back away while chasing to break
line of sight — it should head to your last known position, look around
briefly (Search), then give up and resume patrolling.

**Known rough edges:** this is a first pass with no live tuning — vision
range/angle, hearing radius, speeds, and search duration are all plain fields
on `CatAI`/`CatVision`/`CatHearing`, easy to adjust once you've felt it. The
NavMesh is baked at Unity's default "Humanoid" scale rather than true
cat-scale (see the comment atop `CatAIBuilder.cs` for why) — the cat may not
hug walls as tightly as it ideally would, but should still navigate the room
correctly.

### Hiding spots

The room was wide open with nothing to break line of sight, which is most of
why the cat felt unfair. **Mouse Game → Build Hiding Spots** scatters 5 small
cover blocks around the room — CatVision already treats any solid collider as
blocking, so standing behind one just works, no extra wiring. Cover height
(0.26) is taller than the cat's eye point (0.20, set on `CatVision.eye` in
`CatAIBuilder` — previously defaulted to ground level, which didn't match
where the model's eyes actually are).

**Important ordering:** run **Build Hiding Spots**, then re-run **Build Cat AI
(Milestone 5)** — the NavMesh was baked before these obstacles existed, so the
cat's pathing won't know to route around them until it's rebaked.

## Milestone 6 — Sound / stealth

`PlayerMotor` now fires `Jumped`/`Landed` events; `NoiseEmitter` (on `Player`,
wired automatically by `Build MVP Scene`) turns movement into a noise radius:
0 while idle, small while walking, larger while sprinting, and a brief larger
pulse on jump/land. `CatHearing.CanHearPlayer` reads that radius instead of
the flat proximity check it started as — no changes needed anywhere else,
that's exactly what having a stable method signature there was for.

"Knocking a prop = loud" from the README isn't wired up — there's no
prop-interaction system yet to trigger it. `NoiseEmitter.EmitNoise(radius)` is
the hook for whenever one exists; `CatHearing` won't need to change.

**Audio:** the noise system above is silent by design (it's a gameplay radius
for the cat's hearing, not sound effects). Actual audible footstep/jump/land
SFX are a separate addition — `PlayerAudio` (also on `Player`) plays short
procedurally generated placeholder tones (`Assets/Scripts/Audio/
ProceduralAudio.cs`) on the same `PlayerMotor` events, since no real audio
asset files exist in the project. Swap in real recorded clips later by
assigning them in `PlayerAudio` instead of the generated ones.

No new build step — re-run **Build MVP Scene** to add `NoiseEmitter`/
`PlayerAudio` to an existing `Player` if not already there (safe/idempotent).

### Knockable props ("knocking a prop = loud")

Run **Mouse Game → Build Knockable Props** — scatters 3 small cylinder props
around the room and adds `PropKnocker` to `Player`. Bumping into one (walking
into it) emits a louder, longer noise pulse than sprinting does — `CatHearing`
doesn't need any changes to react to it, same as the walk/sprint/jump/land
noise. **Test:** walk into a prop near the cat (but outside its vision cone)
— it should be more likely to notice than an equivalent walk without a prop.

**Test:** stand still near the cat's patrol path — it shouldn't notice you
from sound alone at normal patrol distances (only sight, or if you're inside
`walkNoiseRadius`/`sprintNoiseRadius` of it). Sprint past it (not in its
vision cone) — it should be able to hear and investigate from further away
than a walk would allow. Jump/land near it — same, a brief noticeable pulse.
Noise radii (`walkNoiseRadius`, `sprintNoiseRadius`, `jumpNoiseRadius`,
`landNoiseRadius` on `NoiseEmitter`) are untested guesses, easy to tune once
you've felt it.

## First Complete Level — Kitchen

The README's actual target level: cabinet (start) → box → chair → table →
countertop, cheese on the countertop, the cat guarding the path, and a real
**escape** step — reaching the cheese alone no longer ends anything, you have
to get back to the start.

Unlike the stairs test (auto-climbed via `stepOffset`), each hop here is a
**genuine jump** — the height gaps (0.12/0.16/0.17/0.17) exceed `stepOffset`
(0.04) and rely on `PlayerMotor.jumpHeight` (0.22) to actually clear them.
This coexists with, doesn't replace, the generic stairs/hiding-spot test props
elsewhere in the room.

1. Run **Mouse Game → Build Kitchen Level** (after Build MVP Scene, Build Test
   Room, and ideally Build Cat AI so the cat gets repositioned to guard the
   path too) — builds the furniture path near `(-3.5..-2.1, *, 2..3.4)`, moves
   `Cheese` onto the countertop, adds an invisible `EscapeZone` trigger near
   the cabinet, and repositions `Cat` to `(-2.85, 0, 2.9)`.
2. If you'd already built `Cat AI` before this, re-run **Build Cat AI**
   afterward too — it re-bakes the NavMesh, which should now account for the
   new furniture blocking/shaping paths through that area.
3. Press Play.

**Test:** climb the path (box → chair → table → countertop) via jumping —
each gap should be reachable but require an actual jump, not just walking
into it. Grab the cheese ("Found it!" should appear, same as before). Try
walking into the escape zone *before* grabbing the cheese — nothing should
happen. Then return to the escape zone after grabbing it — "You escaped!
Level Complete!" should appear and movement should freeze. Avoid/lose the cat
somewhere in there for the full experience.

**Known rough edges:** the furniture positions/gap sizes are a first pass with
no live testing — if a jump doesn't quite reach, nudge the relevant block
closer together or shorten the gap in `KitchenLevelBuilder.Path`, or bump
`PlayerMotor.jumpHeight` slightly. The escape zone's collider is invisible on
purpose (so it doesn't look like a piece of blocking furniture) — if that's
confusing in practice, give it a placeholder material instead of disabling
its renderer.

## Quality-of-life: Restart button

Both the Game Over and Level Complete screens now include a **Restart**
button (reloads the scene) so you can retry without leaving Play mode —
useful for a long testing session covering everything at once. Built by
`CatAIBuilder.BuildRestartButton` (shared with `KitchenLevelBuilder`); no
separate build step, it's part of `Build Cat AI` / `Build Kitchen Level`.
