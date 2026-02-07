using UnityEditor;
using UnityEngine;

public class SlothsToolWindow : EditorWindow
{
    [MenuItem("Window/Sloth's Tools")]
    public static void ShowWindow()
    {
        SlothsToolWindow window = GetWindow<SlothsToolWindow>("Sloth's Tools");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Texture & Material Tools", EditorStyles.boldLabel);
        // Add button to open SlothsAutoMaterialSetup window
        if (GUILayout.Button("Sloth's Auto Material Setup"))
        {
            SlothsAutoMaterialSetup.ShowWindow(); // Open SlothsAutoMaterialSetup window
        }
        // Add button to open Sloth's Texture Setup Tool
        if (GUILayout.Button("Sloth's Auto Texture Setup"))
        {
            slothstexturesetup.ShowWindow(); // Sloth's Material Sync Tool window
        }
        // Add button to open Sloth's Material Merger Tool
        if (GUILayout.Button("Sloth's Material Merger Tool"))
        {
            slothsmatmerger.ShowWindow(); // Sloth's Material Merger Tool window
        }
        // Add button to open Sloth's Multi Material Slot Tool
        if (GUILayout.Button("Sloth's Multi Material Slot Tool"))
        {
            SlothMaterialSlotEditor.ShowWindow(); // Sloth's Multi Material Slot Tool
        }
        // Add button to open Sloth's Asset Cleaner Tool
        if (GUILayout.Button("Sloth's Asset Cleaner Tool"))
        {
            UnusedSceneAssetCleanerWindow.ShowWindow(); // Sloth's Material Slot Tool window
        }
        // Add button to open Sloth's CRT to PNG Tool
        if (GUILayout.Button("Sloth's CRT to PNG Tool"))
        {
            RenderTexturePngExporterWindow.ShowWindow(); // Sloth's CRT to PNG Tool
        }
        GUILayout.Label("Collision & LOD Tools", EditorStyles.boldLabel);
        // Add button to open Sloths Collision Tool window
        if (GUILayout.Button("Sloth's Collision Tool"))
        {
            SlothsCollisionTool.ShowWindow(); // Open Sloths Collision Tool window
        }
        // Add button to open SlothsLODManager window
        if (GUILayout.Button("Sloth's Auto LOD Setup"))
        {
            SlothsLODManager.ShowWindow(); // Open SlothsLODManager window
        }
    }
}
