# Sloths-Unity-Tools
A collection of unity tools for carx drift racing online 

Sloth’s Auto Material Setup (HDRP)

An Editor tool for HDRP/Lit materials that batch-converts selected materials to Specular Color or Subsurface Scattering, auto-fills missing specular/normal maps by filename suffix matching, and can also scan the project to mark normal map textures correctly.

How to Use

Select one or more HDRP/Lit materials in the Project window

Open Sloth’s Auto Material Setup from the Tools menu

Click Setup Materials (Specular Color) or Setup Materials (Subsurface Scattering), and optionally run Setup Normal Map Textures to flag _nml/_nm/_normal textures as Normal Maps

Sloth’s Texture Setup

An Editor tool that batch-applies Crunch compression, automatically scales max texture size based on resolution, and configures normal maps using filename detection.

How to Use

Select one or more textures in the Project window

Open Sloth’s Texture Setup from the Tools menu

Choose a compression mode and quality, then click Apply

Sloth’s Material Merger

An Editor tool that detects identical materials, reports any differences, and merges them into a single shared material while automatically updating all affected renderers in the scene.

How to Use

Select two or more materials in the Project window

Open Sloth’s Material Merger from the Tools menu

Click Get Selected Materials to validate them, then Merge Materials to combine and replace all references

Sloth’s Material Slot Editor

An Editor tool that exposes and allows direct editing of individual material slots on a selected GameObject and all its child renderers, bypassing Unity’s limited default material UI.

How to Use

Select a GameObject in the Scene

Open Sloth’s Material Slot Editor from the Tools menu

Assign or swap materials per slot for the object and its children in the custom UI
