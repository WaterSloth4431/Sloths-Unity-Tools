using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class UnusedSceneAssetCleanerWindow : EditorWindow
{
    private DefaultAsset texturesFolder;
    private DefaultAsset materialsFolder;

    //[MenuItem("Tools/Unused Scene Asset Cleaner")]
    public static void ShowWindow()
    {
        var window = GetWindow<UnusedSceneAssetCleanerWindow>("Sloth's Unused Asset Cleaner");
        window.minSize = new Vector2(420, 170);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Sloth's Unused Scene Asset Cleaner", EditorStyles.boldLabel);
        GUILayout.Space(6);

        EditorGUILayout.HelpBox(
            "Scans the OPEN scene for used Materials/Textures.\n" +
            "Moves unused assets from the selected folders into sibling 'Unused' folders.",
            MessageType.Info
        );

        GUILayout.Space(6);

        texturesFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            new GUIContent("Textures Folder", "Folder under Assets/ to scan for textures"),
            texturesFolder,
            typeof(DefaultAsset),
            false
        );

        materialsFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            new GUIContent("Materials Folder", "Folder under Assets/ to scan for materials"),
            materialsFolder,
            typeof(DefaultAsset),
            false
        );

        GUILayout.FlexibleSpace();

        using (new EditorGUI.DisabledScope(texturesFolder == null && materialsFolder == null))
        {
            if (GUILayout.Button("Run Cleanup", GUILayout.Height(32)))
            {
                RunCleanup();
            }
        }
    }

    private void RunCleanup()
    {
        // Unity 2023+: FindObjectsOfType is deprecated; use FindObjectsByType
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        HashSet<Material> usedMaterials = new HashSet<Material>();
        HashSet<Texture> usedTextures = new HashSet<Texture>();

        foreach (Renderer renderer in renderers)
        {
            var mats = renderer.sharedMaterials;
            if (mats == null) continue;

            foreach (Material mat in mats)
            {
                if (mat == null) continue;

                usedMaterials.Add(mat);

                Shader shader = mat.shader;
                if (shader == null) continue;

                int propCount = ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < propCount; i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        string propName = ShaderUtil.GetPropertyName(shader, i);
                        Texture tex = mat.GetTexture(propName);
                        if (tex != null) usedTextures.Add(tex);
                    }
                }
            }
        }

        int movedTextures = 0;
        int movedMaterials = 0;

        if (texturesFolder != null)
        {
            string texPath = AssetDatabase.GetAssetPath(texturesFolder);
            if (!IsValidProjectFolder(texPath))
            {
                EditorUtility.DisplayDialog("Invalid Textures Folder", "Please select a folder under Assets/.", "OK");
                return;
            }

            movedTextures = MoveUnusedAssets<Texture>(
                "t:Texture",
                texPath,
                usedTextures,
                "Unused Textures"
            );
        }

        if (materialsFolder != null)
        {
            string matPath = AssetDatabase.GetAssetPath(materialsFolder);
            if (!IsValidProjectFolder(matPath))
            {
                EditorUtility.DisplayDialog("Invalid Materials Folder", "Please select a folder under Assets/.", "OK");
                return;
            }

            movedMaterials = MoveUnusedAssets<Material>(
                "t:Material",
                matPath,
                usedMaterials,
                "Unused Materials"
            );
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Cleanup Complete",
            $"Moved:\n- {movedTextures} unused textures\n- {movedMaterials} unused materials",
            "OK"
        );
    }

    private static bool IsValidProjectFolder(string assetPath)
    {
        return !string.IsNullOrEmpty(assetPath)
               && assetPath.StartsWith("Assets")
               && AssetDatabase.IsValidFolder(assetPath);
    }

    private static int MoveUnusedAssets<T>(
        string assetFilter,
        string scanPath,
        HashSet<T> usedAssets,
        string unusedFolderName
    ) where T : Object
    {
        string[] guids = AssetDatabase.FindAssets(assetFilter, new[] { scanPath });

        string parentDir = Path.GetDirectoryName(scanPath).Replace("\\", "/");
        string unusedFolderPath = parentDir + "/" + unusedFolderName;

        if (!AssetDatabase.IsValidFolder(unusedFolderPath))
        {
            AssetDatabase.CreateFolder(parentDir, unusedFolderName);
        }

        int movedCount = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null) continue;

            if (!usedAssets.Contains(asset))
            {
                string fileName = Path.GetFileName(assetPath);
                string targetPath = unusedFolderPath + "/" + fileName;

                string result = AssetDatabase.MoveAsset(assetPath, targetPath);
                if (string.IsNullOrEmpty(result))
                {
                    movedCount++;
                }
                else
                {
                    Debug.LogWarning($"Failed to move {assetPath} -> {targetPath}\n{result}");
                }
            }
        }

        Debug.Log($"Moved {movedCount} unused {typeof(T).Name}(s) to {unusedFolderPath}");
        return movedCount;
    }
}
