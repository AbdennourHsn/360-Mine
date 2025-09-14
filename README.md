# Unity 360° Walkthrough — 3D Scan + Image Transitions (URP)

Short prototype that blends **360° images** with a **3D environment scan** to simulate “3D” transitions between capture points. Includes custom shaders, basic collision feedback, and a small in-game UI to tune behavior in real time.

---

## ✨ Features

- **360° → 360° transitions** with a 3D feel (camera tween + image blending)
- **3D scan shown during transitions** (semi-transparent “bridge” for visual continuity)
- **Custom shaders**  
  - Euler rotation controls for 360 cubemaps  
  - Image **blend/switch**  
  - **Stretch** parameter to suggest motion  
  - **Fake fade** to reveal/hide the environment cleanly
- **Collision awareness**: distance from camera → colliders computed at runtime
- **In-game UI** (basic Unity UI)  
  - Cursor sensitivity  
  - Transition speed  
  - **Reset Scene** button  
  - Live **camera–collider distance** readout
- **Anchor points**: estimated (no ground-truth anchors provided) and used to align each 360° image to the scan

---

## 🧩 Tech & Dependencies

- **Unity**: tested with **2022.3 LTS** (URP).  
- **Render Pipeline**: **URP** (Universal Render Pipeline).  
- **Tweening**: **DOTween** (free) for smooth camera moves.  
- (Optional) **TextMeshPro** for UI text.

---


---

## 🚀 Quick Start

1. **Clone** the repo and open with **Unity 2022.3+**.
2. Make sure the project is set up with **URP** (Graphics → set URP Render Pipeline Asset).
3. **Import DOTween** (Asset Store → DOTween → Setup DOTween…).
4. Open **`Scenes/1.unity`**.
5. Press **Play**:
   - Use the **UI sliders** to adjust **cursor sensitivity** and **transition speed**.
   - Click an **Anchor** in the scene (or use provided UI/keys) to trigger a transition.
   - Press **Reset Scene** to go back to the initial state.

---

## 🛠️ How It Works

- **Anchors**: Each 360° image corresponds to an estimated anchor in the 3D scan (since no official anchor data was provided).  
- **Transition**:  
  1) Camera tweens to the target anchor (DOTween).  
  2) Shader blends from the current 360° image to the next.  
  3) **3D scan** is shown in transparency during the move to maintain spatial continuity.  
  4) A light **stretch** is applied to the 360° projection toward the camera angle to enhance the “depth” feeling.  
- **Collision Metric**: A script computes and displays the **distance from camera → nearest collider** to guide interaction thresholds.
- **Environment Fade**: A custom “fake fade” path hides/shows the environment cleanly around image switches.

---

## 🎮 Controls

- **UI Sliders**: Cursor sensitivity, Transition speed  
- **Button**: Reset Scene  
- **On-screen readout**: Camera ↔ Collider distance  
- **Mouse/Click**: Select anchors (or hover/click on mesh targets if enabled)

---

## 📦 Build Targets

- **Desktop**: Works out of the box.  
- **Web (planned)**: WebGL build + a small **React.js** wrapper/page for hosting a live demo.  
  > Note: Not shipped yet due to time constraints.

---

## ⚠️ Limitations

- **Perfect alignment** is hard without ground-truth anchor correspondences.  
- The **stretch** is a perceptual helper, not real parallax.  
- Performance depends on scan complexity and device.

---

## 🗺️ Roadmap

- Authoring tool to **pick/solve anchors** more precisely  
- Improved **parallax** (depth cues) for 360s  
- **WebGL + React** demo page  
- Better mobile controls & UX polish  

---

## 🙏 Credits

- 3D scan & 360° images: **[Provider / Client Name]**  
- Tweening: **DOTween**  
- Unity **URP**






