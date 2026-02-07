using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering.HighDefinition;

public class SlothsAutoMaterialSetup : EditorWindow
{
    // Specular color picker field
    private Color specularColor = Color.white;

    //[MenuItem("Tools/Sloth's Auto Material Setup")]
    public static void ShowWindow()
    {
        // Open the editor window
        GetWindow<SlothsAutoMaterialSetup>("Sloth's Auto Material Setup");
    }

    // GUI Layout
    private void OnGUI()
    {

        GUILayout.Space(10);  // Add some space
        GUILayout.Label("Specular Material Setup", EditorStyles.boldLabel);

        // Display a description of how the script works
        EditorGUILayout.HelpBox("This script allows you to set the specular color material type for all selected materials, and set missing maps. It searches for specular maps with the '_spe','_specular' or '_spec' suffix,and normal maps with the '_nml','_normal' or '_nm' suffix in the same folder as the base color map, and if found, applies them. If no specular map or normal map is found, it uses the base map as specular map, and a fallback normal map called 'white_NM'. Both buttons also sets Smoothness to 0.", MessageType.Info);


        // Color picker for Specular Color
        specularColor = EditorGUILayout.ColorField("Specular Color", specularColor);
        EditorGUILayout.HelpBox("recommended Color hex: 808080", MessageType.Info);
  


        // Button to trigger the material setup for specular and normal maps
        if (GUILayout.Button("Setup Materials (Specular Color)"))
        {
            SetSpecularColorMaterials(specularColor);
        }

        GUILayout.Label("Subsurface Scattering Material Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Sets all Selected Materials to Subsurface Scattering. After applied Manually set the diffusion profile to Foliage from TestMap.", MessageType.Info);


        // Button to trigger the material setup for subsurface scattering (SSS)
        if (GUILayout.Button("Setup Materials (Subsurface Scattering)"))
        {
            SetSubsurfaceScatteringMaterials();
        }


        GUILayout.Label("Normal Map Texture Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Sets all normal map textures to the correct type in the project. textures must be in a folder named Textures in the project file. It will find textures with the '_nml', '_nm', or '_normal' suffix and mark them as normal maps.", MessageType.Info);


        // Button to trigger normal map texture setup
        if (GUILayout.Button("Setup Normal Map Textures"))
        {
            SetNormalMaps();
        }
    }

    // Function to set the materials to Specular Color
    static void SetSpecularColorMaterials(Color selectedSpecularColor)
    {
        // Get selected objects in the Project view
        Object[] selectedObjects = Selection.objects;

        foreach (Object obj in selectedObjects)
        {
            // Check if the selected object is a Material
            if (obj is Material)
            {
                Material material = obj as Material;

                // Check if the material uses the HDRP Lit shader
                if (material.shader.name == "HDRP/Lit")
                {
                    // Set the material type to Specular Color
                    material.SetFloat("_MaterialID", 4.0f); // 4 corresponds to Specular Color

                    // Update the keywords to ensure the material is correctly set up
                    material.EnableKeyword("_MATERIAL_FEATURE_SPECULAR_COLOR");
                    material.DisableKeyword("_MATERIAL_FEATURE_ANISOTROPY");
                    material.DisableKeyword("_MATERIAL_FEATURE_IRIDESCENCE");
                    material.DisableKeyword("_MATERIAL_FEATURE_SUBSURFACE_SCATTERING");
                    material.DisableKeyword("_MATERIAL_FEATURE_TRANSLUCENT");

                    // Set the Specular Color
                    material.SetColor("_SpecularColor", selectedSpecularColor);

                    // Set smoothness to 0
                    material.SetFloat("_Smoothness", 0.0f);

                    // Get the base map texture
                    Texture baseMap = material.GetTexture("_BaseColorMap");

                    if (baseMap != null)
                    {
                        // Get the path to the base map texture
                        string baseMapPath = AssetDatabase.GetAssetPath(baseMap);

                        // Get the directory of the base map texture
                        string directory = Path.GetDirectoryName(baseMapPath);

                        // Check for specular map with "_spe", "_specular", or "_spec"
                        string specularMapPath = FindTextureInDirectory(directory, baseMapPath, "_spe", "_specular", "_spec");

                        if (specularMapPath != null)
                        {
                            // Load the specular map texture
                            Texture specularMap = AssetDatabase.LoadAssetAtPath<Texture>(specularMapPath);
                            material.SetTexture("_SpecularColorMap", specularMap);
                            Debug.Log($"Updated specular map for {material.name}: {specularMap.name}");
                        }
                        else
                        {
                            // If no specular map is found, set the SpecularColorMap to the BaseColorMap
                            material.SetTexture("_SpecularColorMap", baseMap);
                            Debug.Log($"No specular map found. Using base map for {material.name}");
                        }

                        // Now handle the normal map
                        Texture normalMap = material.GetTexture("_NormalMap");

                        // If no normal map exists, search for a matching one
                        if (normalMap == null)
                        {
                            // Check for normal map with "_nml", "_normal", or "_nm"
                            string normalMapPath = FindTextureInDirectory(directory, baseMapPath, "_nml", "_normal", "_nm");

                            if (normalMapPath != null)
                            {
                                // Load the normal map texture
                                normalMap = AssetDatabase.LoadAssetAtPath<Texture>(normalMapPath);
                                material.SetTexture("_NormalMap", normalMap);
                                Debug.Log($"Updated normal map for {material.name}: {normalMap.name}");
                            }
                            else
                            {
                                // If no normal map is found, look for "white_NM"
                                string fallbackNormalPath = Path.Combine(directory, "white_NM" + Path.GetExtension(baseMapPath));

                                if (File.Exists(fallbackNormalPath))
                                {
                                    normalMap = AssetDatabase.LoadAssetAtPath<Texture>(fallbackNormalPath);
                                    material.SetTexture("_NormalMap", normalMap);
                                    Debug.Log($"Using fallback normal map for {material.name}: white_NM");
                                }
                                else
                                {
                                    Debug.LogError($"No normal map found for {material.name}, and no fallback normal map (white_NM) found in {directory}.");
                                }
                            }
                        }
                    }

                    // Mark the material as dirty to ensure the changes are saved
                    EditorUtility.SetDirty(material);

                    Debug.Log($"Updated material: {material.name}");
                }
            }
        }

        // Save all modified assets
        AssetDatabase.SaveAssets();
    }

    // Function to set the materials to Subsurface Scattering (SSS)
    static void SetSubsurfaceScatteringMaterials()
    {
        // Get selected objects in the Project view
        Object[] selectedObjects = Selection.objects;

        foreach (Object obj in selectedObjects)
        {
            // Check if the selected object is a Material
            if (obj is Material)
            {
                Material material = obj as Material;

                // Check if the material uses the HDRP Lit shader
                if (material.shader.name == "HDRP/Lit")
                {
                    // Set the material type to Subsurface Scattering (MaterialID 0 corresponds to Subsurface Scattering)
                    material.SetFloat("_MaterialID", 0.0f); // 0 corresponds to Subsurface Scattering

                    // Enable the subsurface scattering feature
                    material.EnableKeyword("_MATERIAL_FEATURE_SUBSURFACE_SCATTERING");

                    // Disable other features that should not be active
                    material.DisableKeyword("_MATERIAL_FEATURE_SPECULAR_COLOR");
                    material.DisableKeyword("_MATERIAL_FEATURE_ANISOTROPY");
                    material.DisableKeyword("_MATERIAL_FEATURE_IRIDESCENCE");
                    material.DisableKeyword("_MATERIAL_FEATURE_TRANSLUCENT");

                    // Set smoothness to 0
                    material.SetFloat("_Smoothness", 0.0f);

                    // Mark the material as dirty to ensure the changes are saved
                    EditorUtility.SetDirty(material);

                    Debug.Log($"Updated material to Subsurface Scattering: {material.name}");
                }
            }
        }

        // Save all modified assets
        AssetDatabase.SaveAssets();
    }

    // Helper function to search for textures with given suffixes in the same directory
    static string FindTextureInDirectory(string directory, string baseMapPath, params string[] suffixes)
    {
        // Iterate through the suffixes
        foreach (string suffix in suffixes)
        {
            // Create the name for the texture by appending the suffix
            string texturePath = Path.Combine(directory, Path.GetFileNameWithoutExtension(baseMapPath) + suffix + Path.GetExtension(baseMapPath));

            // Check if the texture exists and return the first found
            if (File.Exists(texturePath))
                return texturePath;
        }

        return null; // Return null if no texture is found
    }

    // Function to set normal maps for textures
    static void SetNormalMaps()
    {
        // Get all textures in the project
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D");

        foreach (string guid in textureGuids)
        {
            // Get the path of each texture
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Check if the texture is in the "Textures" folder (you can modify this if needed)
            if (path.Contains("Textures"))
            {
                // Get the texture
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                // Check if texture name ends with one of the specified suffixes
                if (texture.name.EndsWith("_nml") || texture.name.EndsWith("_nm") || texture.name.EndsWith("_normal"))
                {
                    // Get texture import settings
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                    if (importer != null)
                    {
                        // Check if it's not already set as a normal map
                        if (importer.textureType != TextureImporterType.NormalMap)
                        {
                            // Set the texture type to normal map
                            importer.textureType = TextureImporterType.NormalMap;
                            importer.SaveAndReimport();

                            Debug.Log("Set texture to normal map: " + texture.name);
                        }
                    }
                }
            }
        }

        // Refresh the Asset Database to reflect the changes
        AssetDatabase.Refresh();
    }
}
