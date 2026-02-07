# Sloths Unity Tools

A collection of Unity **Editor tools** designed to streamline workflows for **CarX Drift Racing Online** and general Unity projects.

Find it in: **Window → Sloth’s Tools**

Website: https://watersloth.carrd.co/
---

## 🧰 Tools Included

### **Sloth’s Auto Material Setup (HDRP)**
An Editor tool for HDRP/Lit materials that batch-converts selected materials to **Specular Color** or **Subsurface Scattering**, auto-fills missing specular and normal maps using filename suffix matching, and can scan the project to correctly mark normal map textures.

**How to Use**
1. Select one or more **HDRP/Lit** materials in the Project window  
2. Open **Sloth’s Auto Material Setup** from the Tools menu  
3. Click **Setup Materials (Specular Color)** or **Setup Materials (Subsurface Scattering)**  
4. Optionally run **Setup Normal Map Textures** to flag `_nml`, `_nm`, or `_normal` textures as Normal Maps  

---

### **Sloth’s Texture Setup**
An Editor tool that batch-applies **Crunch compression**, automatically scales max texture size based on resolution, and configures normal maps using filename detection.

**How to Use**
1. Select one or more textures in the Project window  
2. Open **Sloth’s Texture Setup** from the Tools menu  
3. Choose a compression mode and quality, then click **Apply**

---

### **Sloth’s Material Merger**
An Editor tool that detects identical materials, reports any differences, and merges them into a single shared material while automatically updating all affected renderers in the scene.

**How to Use**
1. Select two or more materials in the Project window  
2. Open **Sloth’s Material Merger** from the Tools menu  
3. Click **Get Selected Materials** to validate them, then **Merge Materials** to combine and replace all references  

> ⚠️ This tool deletes the original materials after merging.

---

### **Sloth’s Material Slot Editor**
An Editor tool that exposes and allows direct editing of individual material slots on a selected GameObject and all its child renderers, bypassing Unity’s limited default material UI.

**How to Use**
1. Select a GameObject in the Scene  
2. Open **Sloth’s Material Slot Editor** from the Tools menu  
3. Assign or swap materials per slot using the custom UI  

---

### **Sloth’s Collision Tool**
An Editor tool that applies a selected collision preset from a **Collisions** prefab to the currently selected objects by copying its `GameMarkerData` values and ensuring each object has a `MeshCollider`.

**How to Use**
1. Select one or more GameObjects in the Scene  
2. Open **Sloth’s Collision Tool** from the Tools menu  
3. Assign (or auto-detect) the **Collisions** prefab  
4. Choose a collision type and click **Apply to Selected Objects**

---

### **Sloth’s LOD Manager**
An Editor tool that batch-creates simple `LODGroup`s for selected objects under a master parent, and can scan prefab folders to auto-set LOD cull transition ratios based on mesh size (with an optional maximum override).

**How to Use**
1. Select one or more GameObjects in the Scene  
2. Open **Sloth’s Auto LOD Setup Tools** from the Tools menu  
3. Use **Create LOD Group** to generate LOD parents  
4. Or browse to a prefab folder and click **Apply Cull Ratio** to update LOD transitions  

---

## ⚠️ Requirements
- Unity 2021.3+ recommended  
- Editor-only tools (no runtime scripts)  
- HDRP required for some material tools  

## 📄 License
MIT License
