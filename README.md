# NMIMS Hostel Tour

[![Unity Version](https://img.shields.io/badge/Unity-6000.3.26f1%20(Unity%206)-blue.svg)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-URP-orange.svg)](https://unity.com/srp/Universal-Render-Pipeline)
[![Platforms](https://img.shields.io/badge/Platforms-Desktop%20%7C%20WebGL%20%7C%20Meta%20Quest-green.svg)](#-building)
[![Git LFS](https://img.shields.io/badge/Git-LFS%20Enabled-red.svg)](https://git-lfs.github.com/)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)

An interactive 360-degree virtual walkthrough of the **NMIMS Hostel**, built with **Unity 6** and the **Universal Render Pipeline (URP)**. Footage was recorded with a GoPro MAX 360 camera, and the app lets users explore hostel facilities, rooms and corridors in a Street View style, on desktop, in the browser, or in a Meta Quest headset.

---

## 🌟 Features

* **Meta Quest VR support** (`TourVR.unity`): OpenXR build with head tracking and a full controller scheme (buttons, joysticks, and a pointer for the on-screen menu). Controls are read directly through `UnityEngine.XR` `InputDevices`, so they do not depend on XR Interaction Toolkit UI setup.
* **360° curved panorama projection**: a procedurally generated curved mesh (`CurvedScreen.cs`) renders the recorded frames with configurable horizontal and vertical field of view.
* **Node-based navigation**: every frame is a node (`PanoNode.cs`) linked forward and backward along the walking route, with optional clickable arrow hotspots (`Hotspot.cs`) on desktop and web.
* **Area quick-jump menu**: an overlay menu (`AreaMenu.cs`) built with TextMesh Pro, generated from the route so every area and corridor piece gets a button.
* **Look controls**: mouse drag and touch swipe panning with angle clamping (`LimitedLook.cs`), so users never see the empty edges of the footage.
* **Fade transitions**: smooth alpha fade between nodes (`PanoViewer.cs`).
* **One-click tour builder** (`TourBuilder.cs`, `Tour` menu): sets up the scene, fixes texture import settings, and creates and links all ~288 nodes from the folder structure and a short route definition.
* **Web build**: runs in the browser via WebGL with touch support.

---

## 📍 Areas Included in the Tour

Walking order:

1. Entrance
2. Reception
3. Corridor (cut into pieces between places)
4. TV Room 1
5. Sick Bay
6. TV Room 2
7. Saloon
8. Laundry Room
9. Badminton Court
10. Gym
11. Game Room
12. Mess 1
13. Mess 2
14. Corridor (final stretch to the end)

---

## 🛠️ Prerequisites & Setup

### 1. Git LFS (required)
High-resolution frames (`Assets/Frames/*/*.jpg`) and fonts are stored with **Git LFS**. Install it before cloning:

```bash
git lfs version          # check if installed

# Ubuntu/Debian: sudo apt-get install git-lfs
# macOS:         brew install git-lfs
# Windows:       https://git-lfs.github.com/

git lfs install
```

### 2. Clone
```bash
git clone https://github.com/aarushchaudhary/NMIMS-Hostel-Tour.git
cd NMIMS-Hostel-Tour
git lfs pull
```

### 3. Unity
* **Unity 6** (`6000.3.26f1` or later) with URP
* For VR: **Android Build Support** (with OpenJDK and Android SDK & NDK) installed through Unity Hub
* For web: **Web Build Support** installed through Unity Hub

---

## 🚀 Getting Started

1. Open **Unity Hub → Add → Add project from disk** and select the project folder.
2. Open it with Unity `6000.3.26f1` (or a compatible Unity 6 release).
3. Open a scene from `Assets/Scenes/`:
   * **`Main.unity`**: desktop, touch and web version.
   * **`TourVR.unity`**: Meta Quest / OpenXR version.
4. Press **Play** (▶).

---

## 🎮 Controls

### Desktop / Web / Mobile
| Input | Action |
|---|---|
| Left mouse drag / touch drag | Look around |
| Click / tap the yellow ball | Move to the next frame |
| **Back** button | Previous frame |
| Left menu buttons | Jump to any area |

### Meta Quest (TourVR)
| Input | Action |
|---|---|
| Head movement | Look around |
| **A** (right) | Next area |
| **B** (right) | Previous area |
| **Y** (left) | Forward one frame |
| **X** (left) | Back one frame |
| **Left trigger** | Click the menu button the pointer line is aimed at |
| **Joysticks** (either) | Turn and tilt the view |
| **Right grip** | Show / hide the on-screen controls help |

The cyan pointer line from the left controller turns green when it is over a button. The controls help panel appears at the bottom of the floating menu on startup.

---

## 🧰 Tour Builder (Editor Tool)

`Assets/Editor/TourBuilder.cs` adds a **Tour** menu to the Unity Editor.

| Menu item | What it does |
|---|---|
| **Tour → 0 - Setup Scene** | Creates the camera, curved screen, canvas, fade panel, area label, Back button, area menu and viewer. Use only on a fresh desktop scene (do not re-run it in `TourVR`, it rebuilds the objects the VR setup changes). |
| **Tour → 1 - Fix Texture Import Settings** | Applies import settings to every frame (clamp, no mipmaps, 4096 max size, crunch compression). |
| **Tour → 2 - Build Nodes From Frames** | Creates one node per image, links them along the route, and fills in the Viewer's start node and the area menu. Safe to re-run. |

### Defining the route
The walking order is the `Places` list near the top of `TourBuilder.cs`:

```csharp
// (folder, label, corridorCut)
//   corridorCut = last Corridor frame played BEFORE entering this place
//   corridorCut = 0 -> no corridor in between
("TV_Room_1", "TV Room 1", 11),
("Sick_Bay",  "Sick Bay",  15),
...
```

Change the cut numbers, then run **Tour → 2** again. Optional shortcuts and door branches can be added to `ExtraLinks`.

---

## 📦 Building

### Web (WebGL)
1. Install **Web Build Support**, then **File → Build Profiles → Web → Switch Platform**.
2. In Player settings, set Compression Format to **Gzip** and tick **Decompression Fallback**.
3. Add `Main.unity` to the scene list and click **Build And Run**.
4. Host the contents of the output folder on itch.io, Netlify or GitHub Pages.

### Meta Quest
1. Enable **Developer Mode** on the headset (Meta Horizon app) and connect it by USB.
2. **File → Build Profiles → Meta Quest**, add only `TourVR.unity` to the scene list.
3. Click **Build And Run**.

Quest requirements: IL2CPP, ARM64, API level 29+, OpenXR with the Oculus Touch, Touch Pro and Touch Plus controller profiles enabled, and **Active Input Handling: Both**.

---

## 🗂️ Project Structure

```
NMIMS Hostel Tour/
├── Assets/
│   ├── CompositionLayers/           # XR composition layer settings
│   ├── Editor/
│   │   └── TourBuilder.cs           # Scene setup + node generation + route wiring
│   ├── Frames/                      # Panorama frames per area (Git LFS)
│   │   ├── Badminton_Court/
│   │   ├── Corridor/
│   │   ├── Entrance/
│   │   ├── Game_Room/
│   │   ├── Gym/
│   │   ├── Laundry_Room/
│   │   ├── Mess_1/
│   │   ├── Mess_2/
│   │   ├── Reception/
│   │   ├── Saloon/
│   │   ├── Sick_Bay/
│   │   ├── TV_Room_1/
│   │   └── TV_Room_2/
│   ├── Materials/                   # FrameMat, ArrowMat and other materials
│   ├── Prefabs/                     # Arrow hotspot prefab (used by Main scene only)
│   ├── Samples/                     # XR Interaction Toolkit starter assets
│   ├── Scenes/
│   │   ├── Main.unity               # Desktop / touch / web scene
│   │   └── TourVR.unity             # Meta Quest / OpenXR scene
│   ├── Scripts/
│   │   ├── AreaMenu.cs              # Auto-generated area jump menu
│   │   ├── CurvedScreen.cs          # Procedural curved projection mesh
│   │   ├── Hotspot.cs               # Clickable / XR-interactable arrow hotspot
│   │   ├── LimitedLook.cs           # Mouse / touch look with yaw and pitch limits
│   │   ├── PanoNode.cs              # Node data: frame, area name, neighbors
│   │   ├── PanoViewer.cs            # Tour manager: transitions, forward/back, current node
│   │   └── VRControls.cs            # Quest controls: buttons, joysticks, UI pointer, help panel
│   ├── TextMesh Pro/                # TextMesh Pro fonts, resources and shaders
│   ├── XR/                          # OpenXR loaders, settings and runtime profiles
│   └── XRI/                         # XR Interaction Toolkit device simulator settings
├── Packages/                        # Package manifest and lockfile
├── ProjectSettings/                 # Unity project configuration
├── .gitattributes                   # Git LFS and Unity merge settings
├── .gitignore                       # Unity ignore rules
├── LICENSE                          # GNU General Public License v3.0
└── README.md                        # Project documentation
```

---

## 🩺 Troubleshooting

| Problem | Fix |
|---|---|
| Black screen in Play | Check the `CurvedScreen` triangle order, the Viewer's Start Node, and the Console for red errors |
| No text on buttons | **Window → TextMeshPro → Import TMP Essential Resources** |
| Input errors in the Console | **Project Settings → Player → Active Input Handling → Both** |
| Quest controls do nothing | Confirm `VRControls` is on the Viewer, the Touch controller profiles are enabled in OpenXR, and `TourVR` is the scene being built |
| Pointer line never turns green | Check the Canvas is World Space at about `0, -0.1, 2.2` with scale `0.0012` |
| `viewer.Current` compile error | Add `public PanoNode Current => current;` to `PanoViewer.cs` |
| Frames missing after clone | Run `git lfs pull` |

---

## 👤 Author

**Aarush Chaudhary**
GitHub: [@aarushchaudhary](https://github.com/aarushchaudhary)

---

## 📜 License

This project is licensed under the **GNU General Public License v3.0** (GPL-3.0). See the [LICENSE](LICENSE) file for details.
