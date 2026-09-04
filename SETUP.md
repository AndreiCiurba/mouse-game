# Setup — Building and Testing the Scene

Everything gameplay-related is built by one-click Editor tools
(`Assets/Editor/*.cs`, under the **Mouse Game** menu) rather than by hand.
Each tool saves the scene to disk when it finishes and is safe to re-run
(idempotent — re-running just re-syncs values/positions, it won't create
duplicates). If you reposition anything by hand afterward (dragging in the
Scene view), press **Ctrl+S** — the tools only auto-save at the moment they
run, not on later manual edits.

## 1. Install Unity & create the project (Editor)

Covered in [`UNITY_INSTALL_GUIDE.md`](./UNITY_INSTALL_GUIDE.md) — install
Unity Hub, install the Unity 6 LTS Editor with Android Build Support, then
create the `mouse-game` project pointed at this folder.

## 2. Full build order

Run these in order, once, from a fresh/empty scene:

1. **Mouse Game → Build Test Room** — floor + 4 walls, `x: -4..4, z: -4..4`.
2. **Mouse Game → Build MVP Scene (Player + Objective)** — `Player`
   (mouse-scale CharacterController + primitive mouse model + camera +
   scripts), `GameManager` with the objective UI, `Cheese`.
3. **Mouse Game → Build Kitchen Level** — the actual target level: furniture
   traversal path, cheese moved onto the countertop, escape zone.
4. **Mouse Game → Build Stairs Test (Milestone 2)** — generic stairs
   mechanic test, separate from the kitchen path.
5. **Mouse Game → Build Hiding Spots** — cover blocks that break line of
   sight.
6. **Mouse Game → Build Knockable Props** — clutter that emits a loud noise
   when bumped.
7. **Mouse Game → Build Cat AI (Milestone 5)** — run this **last** among the
   gameplay builders: it bakes the NavMesh from whatever's in the scene at
   that moment, and detects/guards the Kitchen Level path if it already
   exists. Running it last means one bake sees all the final geometry, no
   re-bake step needed.
8. **Mouse Game → Build Mobile Controls (Milestone 3)** — only if you're
   testing touch controls; screen-space UI, order-independent otherwise.

If you ever re-run an earlier tool afterward (e.g. `Build MVP Scene` again
to pick up a Player fix), it's safe — `Build Kitchen Level`'s Cheese/Cat
placements are preserved rather than reset. Re-running `Build Cat AI` after
adding *new* geometry (e.g. running `Build Hiding Spots` again with
different props) does re-bake the NavMesh, which is correct.

## 3. Verification checklist

See the very end of this file for the full "what to test, and how" list
covering everything below in one pass.

---

## Feature notes

Reference for what each system does and why, kept for context — not a
build sequence (see §2 for that).

### Milestone 1/2 — Movement, jump, stairs

`PlayerInputReader`/`PlayerMotor`/`PlayerLook` (`Assets/Scripts/Player`,
`Assets/Scripts/Input`). WASD + mouse look, Space to jump (up to 2 in a row —
a double jump — before you must land), Shift to sprint. Grounding uses a
manual `Physics.CheckSphere` in `PlayerMotor.CheckGrounded()`, not
`CharacterController.isGrounded` (the built-in flickers false on open flat
ground). Stairs (`StairsTestBuilder`) climb automatically via
`CharacterController.stepOffset` — no button, no script, just walking into
them. Jump is otherwise unconditional; whether it lands you on something is
just physics.

### Milestone 3 — Mobile touch controls

Built via Unity UI's pointer events (`VirtualJoystick`/`TouchLookArea`/
`TapButton`/`HoldButton` in `Assets/Scripts/Input`), not the
`com.unity.inputsystem` Action-asset API — the UI event system already
treats mouse and real touch the same way. `PlayerInputReader` merges these
with keyboard/mouse into the same values it always exposed.

Test via **Window → General → Device Simulator** (renders Game view as a
phone screen, mouse acts as touch) — no build/device needed. Left joystick
moves, right-side drag looks around, Jump/Sprint buttons bottom-right.
Keyboard/mouse still work at the same time.

Known rough edges: touch-look sensitivity (`PlayerInputReader.
touchLookSensitivity`) and `HoldButton`'s release-on-`OnPointerExit`
behavior are untested guesses — tune by feel.

### Milestone 4 — Mouse character + true scale

No Blender access here, so the mouse (`Assets/Editor/MouseModelBuilder.cs`)
is a primitive-assembled placeholder — swap in a real model later by
replacing that method's contents. `Player`'s `CharacterController` is real
mouse scale (height `0.2`, radius `0.06`, `skinWidth` explicitly set to
`radius * 0.1` since the default `0.08` would exceed this radius and make
the controller behave very oddly). The **room stayed the same absolute
size** on purpose — a normal-sized room is already huge next to a real
mouse; shrinking the room too would defeat the "small relative to the
environment" point.

### Milestone 5 — Cat AI

**Idle → Patrol → (sees/hears player) → Chase → (gets close) → Attack →
(catches player) → Game Over**, or player escapes the `Attack` windup (get
clear or break line of sight) back to `Chase`. `Chase → (loses player) →
Search → (times out) → Patrol`. See `Assets/Scripts/AI/{CatAI,CatVision,
CatHearing}.cs` and `Assets/Scripts/Game/GameOverManager.cs`.

`Attack` is a brief stationary windup before the actual catch — not an
instant, unavoidable catch the moment the cat gets close. Patrol/chase
speeds are kept below the player's walk/sprint speeds on purpose — the cat
should never out-move a moving player, only catch one that stands still,
gets cornered, or doesn't react during the Attack windup.

NavMesh is baked at Unity's default "Humanoid" scale rather than true
cat-scale (registering a custom agent type has no reliable Editor-script
API) — the cat may not hug walls as tightly as ideal, but should navigate
correctly. `HidingSpotsBuilder`'s cover blocks work with zero extra script —
`CatVision` already treats any solid collider as blocking line of sight.

### Milestone 6 — Sound / stealth

`PlayerMotor` fires `Jumped`/`Landed` events; `NoiseEmitter` (on `Player`)
turns movement into a noise radius: 0 idle, small walking, larger sprinting
or on a brief jump/land pulse, and a louder/longer pulse from
`PropKnocker` bumping a `KnockableProp` (`Build Knockable Props`).
`CatHearing.CanHearPlayer` reads that radius — this covers the README's
full noise spec, including "knocking a prop = loud".

This noise mechanic is silent by design (a gameplay radius, not audio) —
separately, `PlayerAudio` (also on `Player`) plays actual placeholder SFX
(procedurally generated tones/noise-bursts, `Assets/Scripts/Audio/
ProceduralAudio.cs` — no real audio asset files exist in the project) on
the same `PlayerMotor` events.

### First Complete Level — Kitchen

The README's actual target level: cabinet (start) → box → chair → table →
countertop, cheese on the countertop, the cat guarding the path, and a real
**escape** step — reaching the cheese alone no longer ends anything, you
have to get back to the start (`EscapeZone`/`LevelCompleteManager`/
`LevelCompleteUI`, the win-state mirror of `GameOverManager`/`GameOverUI`).

Unlike the stairs test (auto-climbed via `stepOffset`), each hop is a
**genuine jump** — the height gaps (0.12/0.16/0.17/0.17) exceed
`stepOffset` (0.04) and rely on `jumpHeight` (0.22) to clear them. The path
is a straight line along +Z at a constant X (no strafing needed) with a
tight ~0.08m edge-to-edge gap, worked out from the actual jump-arc math
(`jumpHeight`/`gravity` → ~0.22s time-to-apex → ~0.11m of horizontal travel
at `walkSpeed`) rather than eyeballed. This coexists with, doesn't replace,
the generic stairs/hiding-spot/prop test objects elsewhere in the room.

### Quality-of-life: Restart button

Both the Game Over and Level Complete screens include a **Restart** button
(reloads the scene) so you can retry without leaving Play mode. Built by
`CatAIBuilder.BuildRestartButton` (shared with `KitchenLevelBuilder`).

---

## 4. Full test pass — what to test, and how

Run everything below in one Play session after completing the build order
in §2. Nothing here has been live-tested yet — numeric feel (speeds, jump
heights, vision/hearing ranges, timers) is all a first pass and expected to
need tuning; report anything that feels off rather than assuming it's
correct.

**A. Movement basics**
1. WASD moves, mouse looks around (click into the Game view first to lock
   the cursor if it isn't already).
2. Hold Shift — sprint should be visibly faster.
3. Press Space — jump. Press it again in the air — a second jump (double
   jump) should fire. A third press should do nothing until you land.
4. Confirm jump works reliably everywhere (open floor, near walls, near
   stairs) — not just in specific spots.

**B. Stairs (generic mechanic test, near `x=-1.5`)**
5. Walk straight into the stairs — you should rise up each step
   automatically with no key press, continuing onto the landing at the top.

**C. Objective — find the cheese (test-room version, if not overwritten by
the kitchen level's countertop version)**
6. Walk into the `Cheese` object — it should disappear and "Found it!"
   should appear on screen.

**D. Hiding spots & knockable props**
7. Note the 5 cover-block positions and 3 knockable props scattered around
   the room (used properly in step F below, alongside the cat).
8. Walk into a knockable prop — nothing should happen visually, but it
   should make the cat more likely to notice you from a distance if it's
   nearby (see step F).

**E. Mobile controls (optional — skip if only testing keyboard/mouse)**
9. Open **Window → General → Device Simulator**, dock it next to Game.
10. Press Play with that tab active. Confirm it renders in landscape.
11. Drag the left joystick to move, drag the right side to look around, tap
    Jump, hold Sprint. Keyboard/mouse should still work simultaneously.

**F. Cat AI — detection, chase, attack, search**
12. Observe the cat idling briefly, then wandering to random nearby points
    (Patrol).
13. Walk into its vision cone (in front of it, within range) — it should
    turn and chase you.
14. While it's chasing, sprint or jump/land near it from outside its vision
    cone (elsewhere in a separate attempt) — it should be able to hear and
    investigate noise from further away than a quiet walk would allow, and
    a knocked prop should draw it even further/reliably.
15. Let it get close (within its Attack trigger range) — it should stop and
    pause briefly (a telegraphed windup) rather than catching you
    instantly.
16. During that pause, get clear or break line of sight — it should resume
    Chase instead of catching you.
17. Alternatively, stand still / get cornered through the windup — movement
    should freeze and "Caught! Game Over" should appear, with a **Restart**
    button that reloads the scene.
18. Back away while chasing (before it's close enough to attack) to break
    line of sight — it should head to your last known position, search
    briefly, then give up and resume patrolling.
19. Duck behind one of the 5 hiding-spot cover blocks while being chased —
    the cat should lose sight of you and eventually give up, same as step
    18.

**G. Kitchen level — the actual target experience**
20. Head to the furniture path (straight line at `x=-3.3`, `z` increasing).
21. Jump from the floor onto the box, then chair, then table, then
    countertop — each hop should require an actual jump (not just walking
    up), and each gap should be reachable without feeling impossibly tight.
22. Grab the cheese on the countertop — "Found it!" should appear.
23. Walk into the escape zone (near the cabinet, back where you started)
    **before** grabbing the cheese — nothing should happen.
24. Return to the escape zone **after** grabbing the cheese — "You escaped!
    Level Complete!" should appear, movement should freeze, and the
    **Restart** button should work.
25. Try the whole kitchen sequence again while the cat is actively guarding
    the path — confirm it's possible (if difficult) to avoid it using the
    hiding spots and/or timing, not just luck.

**H. Everything together**
26. One full clean run: spawn → explore → maybe find the test-room cheese
    or head straight for the kitchen → avoid or lose the cat at least once
    → complete the kitchen objective → escape → Level Complete. Note
    anything that breaks, feels unfair, or is simply confusing along the
    way — that's exactly the kind of feedback most useful at this point.
