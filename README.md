# NMIMS Hostel Tour

[![Unity Version](https://img.shields.io/badge/Unity-6000.3.26f1%20(Unity%206)-blue.svg)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-URP-orange.svg)](https://unity.com/srp/Universal-Render-Pipeline)
[![Git LFS](https://img.shields.io/badge/Git-LFS%20Enabled-red.svg)](https://git-lfs.github.com/)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)

An interactive 360-degree virtual walkthrough of the **NMIMS Hostel** built with **Unity 6** and the **Universal Render Pipeline (URP)**. This application lets users explore various hostel facilities, rooms, and corridors through seamless panoramic viewing and interactive hotspot navigation.

---

## 🌟 Features

* **VR & OpenXR Support**: Immersive Virtual Reality walkthrough support (`TourVR.unity`) powered by Unity XR Interaction Toolkit and OpenXR with controller ray/direct interactor navigation.
* **360° Curved Panorama Projection**: Dynamically generated curved mesh cylinder (`CurvedScreen.cs`) that renders high-resolution panoramic frames with configurable field-of-view and curvature.
* **Node-Based Spatial Navigation**: Interconnected node graph system (`PanoNode.cs`) allowing natural forward/backward travel between locations with animated directional hotspot arrows (`Hotspot.cs`) and XR interaction (`XRSimpleInteractable`).
* **Area Quick-Jump Menu**: On-screen overlay menu (`AreaMenu.cs`) built with TextMesh Pro and Unity UI to quickly teleport to key hostel zones.
* **Smooth Camera & Look Controls**: Intuitive mouse drag and touch swipe panning controls with customizable sensitivity and angle clamping (`LimitedLook.cs`).
* **Fade Transitions**: Smooth alpha fading when transitioning between panorama nodes (`PanoViewer.cs`).
* **Automated Tour Builder Utility**: Custom Unity Editor tool (`TourBuilder.cs`) under the `Tour` menu that automatically sets up scenes, generates navigation nodes, configures materials, and wires the entire route.

---

## 📍 Areas Included in the Tour

* **Entrance & Reception**
* **Corridors** (segmented connecting path across all wings)
* **TV Room 1 & TV Room 2**
* **Sick Bay**
* **Saloon & Grooming Area**
* **Laundry Room**
* **Badminton Court**
* **Gymnasium**
* **Game Room / Recreation Area**
* **Mess 1 & Mess 2 (Dining Halls)**

---

## 🛠️ Prerequisites & Setup

### 1. Git LFS (Required)
This project uses **Git LFS** (Large File Storage) for high-resolution panoramic frames (`Assets/Frames/*/*.jpg`) and font files.

Ensure Git LFS is installed before cloning:
```bash
# Check if Git LFS is installed
git lfs version

# If not installed, install it:
# Ubuntu/Debian: sudo apt-get install git-lfs
# macOS: brew install git-lfs
# Windows: Download from https://git-lfs.github.com/

git lfs install
```

### 2. Clone the Repository
Clone the repository and pull all LFS objects:
```bash
git clone https://github.com/aarushchaudhary/NMIMS-Hostel-Tour.git
cd NMIMS-Hostel-Tour
git lfs pull
```

### 3. Unity Version
* **Unity 6** (`6000.3.26f1` or later)
* Universal Render Pipeline (URP)

---

## 🚀 Getting Started

1. Open **Unity Hub**.
2. Click **Add** > **Add project from disk** and select the `NMIMS Hostel Tour` directory.
3. Open the project with Unity Editor `6000.3.26f1` (or compatible Unity 6 release).
4. In the Project window, navigate to `Assets/Scenes/`:
   * Double-click **`Main.unity`** for desktop/screen preview.
   * Double-click **`TourVR.unity`** for immersive XR / VR headset experience.
5. Press **Play** (▶) in the Unity Editor to explore the tour.

### Controls
* **Desktop / Mobile**:
  * **Left Mouse Button / Touch**: Click and drag to look around.
  * **Arrow Hotspots**: Click / tap on glowing arrows on the floor to navigate forward or backward.
  * **Left Navigation Menu**: Click any destination button to jump directly to that area.
* **VR Headset**:
  * **Head Tracking**: Look around in 360 degrees.
  * **XR Controllers**: Aim ray at hotspot arrows and pull the trigger/select button to teleport.

---

## 🗂️ Project Structure

```
NMIMS Hostel Tour/
├── Assets/
│   ├── CompositionLayers/           # XR composition layer settings
│   ├── Editor/
│   │   └── TourBuilder.cs           # Editor automation for generating nodes & scenes
│   ├── Frames/                      # 360° photo/render frame sequences (Git LFS)
│   │   ├── Badminton_Court/
│   │   ├── Corridor/
│   │   ├── Entrance/
│   │   ├── Game_Room/
│   │   ├── Mess_1/
│   │   ├── Reception/
│   │   ├── Saloon/
│   │   └── TV_Room_2/
│   ├── Materials/                   # Shaders, skyboxes, and UI materials
│   ├── Prefabs/                     # Reusable navigation arrows & UI elements
│   ├── Samples/                     # XR Interaction Toolkit starter assets & models
│   ├── Scenes/
│   │   ├── Main.unity               # Desktop/touch interactive tour scene
│   │   └── TourVR.unity             # VR / OpenXR interactive tour scene
│   ├── Scripts/
│   │   ├── AreaMenu.cs              # Dynamic UI sidebar for quick teleportation
│   │   ├── CurvedScreen.cs          # Procedural curved projection mesh generator
│   │   ├── Hotspot.cs               # Clickable/XR-interactable hotspot trigger
│   │   ├── LimitedLook.cs           # Mouse/touch camera panning with pitch/yaw clamping
│   │   ├── PanoNode.cs              # Node data representing an individual panoramic spot
│   │   └── PanoViewer.cs            # Core tour manager handling transitions & textures
│   ├── TextMesh Pro/                # TextMesh Pro fonts, resources, and shaders
│   ├── XR/                          # OpenXR loaders, settings, and runtime profiles
│   └── XRI/                         # XR Interaction Toolkit device simulator settings
├── Packages/                        # Unity Package Manager manifest & lockfile
├── ProjectSettings/                 # Unity project configuration and quality settings
├── .gitattributes                   # Git LFS & Unity merge driver attributes
├── .gitignore                       # Unity-specific ignore rules
├── LICENSE                          # GNU General Public License v3.0
└── README.md                        # Project documentation
```

---

## 📜 License

This project is licensed under the **GNU General Public License v3.0** (GPL-3.0). See the [LICENSE](LICENSE) file for complete details.
