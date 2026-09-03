# Project: First-Person Mouse Mobile Game

## Goal

Build an **Android-only first-person 3D game** where the player is a tiny mouse exploring a human house.

Core fantasy:

* First-person mouse perspective
* Walk / sprint / jump / climb
* House furniture becomes traversal/environment
* Cats chase and hunt the player
* Stealth/hiding can be important
* Objectives such as finding/stealing food and escaping
* Stylized/low-poly visual direction
* Mobile touchscreen controls

The first milestone is a **playable prototype**, not a polished game.

---

## Tech Stack

* **Unity 6**
* **C#**
* **Blender** for custom 3D models
* Unity Input System
* Unity Character Controller
* Unity NavMesh for enemy navigation
* Unity Animator for character animation
* Git/GitHub
* Android build target

Do NOT introduce a backend, database, networking, Firebase, AWS, etc. for the initial version.

---

# Development Strategy

Build incrementally in this order:

### 1. Player prototype

Create a basic first-person controller using a capsule.

Requirements:

* WASD/keyboard support for development
* Mouse camera look
* Movement
* Sprint
* Jump
* Gravity
* Collision
* Falling/landing

Keep movement code modular so it can later support mobile input.

---

### 2. Traversal prototype

Create a simple test room using primitive objects:

* floor
* walls
* boxes
* chair
* table
* shelf

Implement:

* jumping
* climbing/mantling
* falling
* basic traversal

Use a simple `Climbable` component/tag.

Initial climbing can be simple:

1. Detect climbable surface/ledge.
2. Verify a valid destination.
3. Show climb indication.
4. Move player onto the surface.

Do NOT implement complex Assassin's Creed-style climbing initially.

---

### 3. Mobile controls

Implement Android touchscreen controls:

```text
LEFT SIDE
Virtual joystick
→ movement

RIGHT SIDE
Touch drag
→ camera

Buttons
→ Jump
→ Climb
→ Sprint
```

Use Unity's Input System and keep input abstracted so keyboard and mobile controls feed the same player controller.

Test frequently on an actual Android device.

---

### 4. Mouse

Create a simple stylized low-poly mouse in Blender.

Model:

* body
* head
* ears
* nose
* eyes
* tail

Import into Unity.

The game remains first-person. The mouse body does not need to be fully visible from the camera.

Make the mouse visually small relative to the environment.

---

### 5. Cat AI

Create a basic cat enemy.

Use Unity NavMesh.

Implement a simple state machine:

```text
IDLE
  ↓
PATROL
  ↓
SEES / HEARS PLAYER
  ↓
CHASE
  ↓
PLAYER ESCAPES → SEARCH
  ↓
PLAYER FOUND → CHASE
  ↓
PLAYER CAUGHT → GAME OVER
```

Keep AI modular so more states can be added later:

* Idle
* Patrol
* InvestigateNoise
* Chase
* Search
* Attack

No machine learning.

---

### 6. Sound/stealth system

Add basic noise levels:

```text
Walking      = quiet
Sprinting    = noticeable
Jumping      = noticeable
Landing      = noticeable
Knocking prop = loud
```

Cats can detect noises within a radius and investigate.

This should create a lightweight stealth mechanic.

---

# First Complete Level

Build only **one room**, preferably a kitchen.

Objective:

> Steal/find a piece of cheese and escape.

Example traversal:

```text
START
under cabinet
    ↓
box
    ↓
chair
    ↓
table
    ↓
countertop
    ↓
avoid cat
    ↓
reach cheese
    ↓
escape
```

The environment should demonstrate:

* small-player scale
* jumping
* climbing
* traversal
* cat AI
* stealth
* objective interaction
* win/lose states

---

# Art Direction

Use a **stylized low-poly aesthetic**.

Prioritize:

1. Gameplay
2. Readability
3. Performance
4. Consistent art style
5. Visual polish

Do not spend significant time creating detailed assets during prototyping.

Use primitive Unity objects/placeholders until gameplay works.

Blender should be used for important custom assets.

---

# Android Performance

Target approximately **60 FPS on a reasonable mid-range Android device**.

Keep an eye on:

* polygon count
* texture sizes
* draw calls
* lighting
* shadows
* physics objects
* particles
* memory
* post-processing

Use Unity Profiler during development.

Avoid unnecessary expensive effects.

---

# Suggested Project Structure

```text
Assets/
├── Art/
│   ├── Models/
│   ├── Materials/
│   ├── Textures/
│   └── Animations/
│
├── Audio/
│
├── Prefabs/
│   ├── Player/
│   ├── Enemies/
│   ├── Environment/
│   └── Props/
│
├── Scenes/
│   ├── Prototype/
│   └── Kitchen/
│
├── Scripts/
│   ├── Player/
│   ├── AI/
│   ├── Interaction/
│   ├── Environment/
│   ├── Game/
│   └── Input/
│
├── UI/
│
└── ScriptableObjects/
```

Keep systems modular and avoid putting all gameplay logic into one MonoBehaviour.

---

# MVP Architecture

```text
GameManager
    │
    ├── PlayerController
    │      ├── Movement
    │      ├── Camera
    │      ├── Jump
    │      ├── Climb
    │      └── Input
    │
    ├── CatAI
    │      ├── StateMachine
    │      ├── Navigation
    │      ├── Vision
    │      └── Hearing
    │
    ├── InteractionSystem
    │
    ├── ObjectiveSystem
    │
    ├── AudioSystem
    │
    └── UI
```

Prefer composition/components over a giant player or enemy class.

---

# Important Development Rule

Do not attempt to build the complete game immediately.

The milestones should be:

```text
Milestone 1
Capsule can walk/jump
        ↓
Milestone 2
Traversal/climbing works
        ↓
Milestone 3
Works with Android controls
        ↓
Milestone 4
Mouse character
        ↓
Milestone 5
Cat can chase player
        ↓
Milestone 6
One complete kitchen level
        ↓
Milestone 7aude
Polish/art/performance
```

At every milestone, maintain a **playable build**.

---

# Claude Code Instructions

Act as the technical lead for this project.

Before implementing large systems:

1. Inspect the existing project structure.
2. Reuse existing systems where possible.
3. Keep components modular.
4. Avoid unnecessary dependencies.
5. Prefer simple implementations initially.
6. Build incrementally.
7. After each major milestone, verify the project compiles.
8. Keep Android as the primary target.
9. Do not add backend/networking unless explicitly requested.
10. Document important setup/configuration steps.

The immediate goal is to create **Milestone 1: a basic first-person player controller in a simple Unity test scene**, with clean architecture that can later support mobile input, climbing, the mouse character, and cat AI.
