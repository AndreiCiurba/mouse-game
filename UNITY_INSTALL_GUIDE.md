# Unity Install Guide (Windows)

Everything below is the manual, one-time setup needed before the scripts in this
repo can run. Do this in order.

## 1. Download & install Unity Hub

Unity Hub manages Editor versions/modules and projects — you install Unity itself
through it, not as a standalone download.

1. Go to **https://unity.com/download**
2. Click the Windows download button to get `UnityHubSetup.exe`.
3. Run the installer and complete setup.
4. Launch Unity Hub and sign in (create a free Unity ID if you don't have one —
   the Personal plan is free and covers this project).

Docs, if anything above looks different from what you see:
https://docs.unity.com/en-us/hub/install-hub-win-mac

## 2. Install the Unity 6 LTS Editor (with Android support)

1. In Unity Hub, go to the **Installs** tab → **Install Editor**.
2. Pick the latest **Unity 6 LTS** version (e.g. 6000.x LTS / "Unity 6.3 LTS" as of
   writing — take whatever LTS build Hub currently offers).
   - Release info: https://unity.com/releases/unity-6
3. In the module selection screen, check:
   - **Android Build Support**
     - and its sub-items: **Android SDK & NDK Tools**, **OpenJDK**
   - (Leave Windows Build Support checked too — useful for quick desktop testing.)
4. Accept licenses and let it install. This step is the biggest download
   (several GB) — it'll take a while.

## 3. Create the project

1. Unity Hub → **Projects** tab → **New Project**.
2. Choose the **3D (URP)** template.
3. **Project name:** `mouse-game`
4. **Location:** `C:\Users\Andrei\Desktop\playground` (the parent folder of this
   repo) — so the new project lands exactly on this existing `mouse-game` folder
   and picks up the `Assets/Scripts/...` files already committed here.
   - If Hub refuses to target a non-empty folder, create the project anywhere
     else and let Claude know the path — the generated `Assets/`, `Packages/`,
     `ProjectSettings/` folders can be merged into this repo afterward.
5. Click **Create project**.

Once the Editor opens, you're set up — continue with the scene-wiring steps in
[`SETUP.md`](./SETUP.md).

## Reference links

- [Unity download page](https://unity.com/download)
- [Unity Hub install docs](https://docs.unity.com/en-us/hub/install-hub-win-mac)
- [Unity 6 releases](https://unity.com/releases/unity-6)
- [Unity 6 release/support info](https://unity.com/releases/unity-6/support)
