using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class slothsmatmerger : EditorWindow
{
    private List<Material> selectedMaterials = new List<Material>();
    private bool materialsAreIdentical = false;
    private List<string> differences = new List<string>();

    //[MenuItem("Tools/Sloth's Mat Merger")]
    public static void ShowWindow()
    {
        GetWindow<slothsmatmerger>("Sloth's Mat Merger");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Tool that detects identical materials, reports any differences, and merges them into a single shared material while automatically updating all affected renderers in the scene.", MessageType.Info);
        GUILayout.Label("Select Materials to Merge", EditorStyles.boldLabel);

        if (GUILayout.Button("Get Selected Materials"))
        {
            selectedMaterials = Selection.objects.OfType<Material>().ToList();
            CheckMaterialIdenticality();
        }

        if (selectedMaterials.Count > 0)
        {
            GUILayout.Label($"Selected Materials: {selectedMaterials.Count}");

            if (materialsAreIdentical)
            {
                GUILayout.Label("MATERIALS ARE INDENTICAL, MERGE NOW", EditorStyles.boldLabel);
            }
            else
            {
                GUILayout.Label("Materials are Not Identical", EditorStyles.boldLabel);
                if (differences.Count > 0)
                {
                    foreach (var diff in differences)
                    {
                        GUILayout.Label(diff);
                    }
                }
            }


        }
        else
        {
            GUILayout.Label("No Materials Selected");
        }

        if (GUILayout.Button("Merge Materials") && selectedMaterials.Count > 1)
        {
            MergeMaterials();
        }
    }

    private void CheckMaterialIdenticality()
    {
        if (selectedMaterials.Count < 2)
        {
            materialsAreIdentical = false;
            differences.Clear();
            return;
        }

        differences.Clear();
        materialsAreIdentical = true;

        Material reference = selectedMaterials[0];
        for (int i = 1; i < selectedMaterials.Count; i++)
        {
            Material mat = selectedMaterials[i];
            if (!AreMaterialPropertiesEqual(reference, mat, out List<string> currentDifferences))
            {
                materialsAreIdentical = false;
                differences.AddRange(currentDifferences.Select(diff => $"Material {i + 1}: {diff}"));
            }
        }

        if (materialsAreIdentical)
        {
            Debug.Log("Materials are identical.");
        }
        else
        {
            Debug.Log("Materials are not identical. See differences:");
            foreach (var diff in differences)
            {
                Debug.Log(diff);
            }
        }
    }

    private void MergeMaterials()
{
    if (!AreMaterialsIdentical(selectedMaterials))
    {
        Debug.LogError("Selected materials are not identical.");
        return;
    }

    // Use the base texture of the first material for naming the new material
    Texture baseTexture = selectedMaterials[0].GetTexture("_MainTex");
    if (baseTexture == null)
    {
        Debug.LogError("No base texture found in the selected materials.");
        return;
    }

    // Extract the folder path of the selected material
    string selectedMaterialPath = AssetDatabase.GetAssetPath(selectedMaterials[0]);
    string folderPath = Path.GetDirectoryName(selectedMaterialPath);

    // Create the new material name based on the base texture and ensure it's unique
    string baseTextureName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(baseTexture));
    string newMaterialName = $"{baseTextureName}_atlas.mat";
    string newMaterialPath = Path.Combine(folderPath, newMaterialName);

    // Check if the material name already exists and append a number if it does
    int count = 1;
    while (AssetDatabase.LoadAssetAtPath<Material>(newMaterialPath) != null)
    {
        newMaterialName = $"{baseTextureName}_atlas_{count}.mat";
        newMaterialPath = Path.Combine(folderPath, newMaterialName);
        count++;
    }

    // Create the new material and save it in the same folder
    Material masterMaterial = new Material(selectedMaterials[0]);
    masterMaterial.name = newMaterialName;
    AssetDatabase.CreateAsset(masterMaterial, newMaterialPath);

    List<GameObject> affectedObjects = new List<GameObject>();
    foreach (Material mat in selectedMaterials)
    {
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer renderer in renderers)
        {
            if (renderer.sharedMaterials.Contains(mat))
            {
                affectedObjects.Add(renderer.gameObject);
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == mat)
                    {
                        materials[i] = masterMaterial;
                    }
                }
                renderer.sharedMaterials = materials;
            }
        }
    }

    // Optionally, delete the original materials
    foreach (Material mat in selectedMaterials)
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(mat));
    }

    // Reload the asset database after merging materials
    AssetDatabase.Refresh();

    Debug.Log($"Merged {selectedMaterials.Count} materials into one: {newMaterialName}. Affected objects: {affectedObjects.Count}");
}

    private bool AreMaterialsIdentical(List<Material> materials)
    {
        if (materials.Count < 2) return false;

        Material reference = materials[0];
        for (int i = 1; i < materials.Count; i++)
        {
            if (!AreMaterialPropertiesEqual(reference, materials[i], out _))
            {
                return false;
            }
        }
        return true;
    }

    private bool AreMaterialPropertiesEqual(Material mat1, Material mat2, out List<string> differences)
    {
        differences = new List<string>();

        if (mat1.shader != mat2.shader)
        {
            differences.Add("Shader");
            return false;
        }

        Shader shader = mat1.shader;
        int propertyCount = ShaderUtil.GetPropertyCount(shader);
        for (int i = 0; i < propertyCount; i++)
        {
            string propertyName = ShaderUtil.GetPropertyName(shader, i);
            ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(shader, i);

            switch (propertyType)
            {
                case ShaderUtil.ShaderPropertyType.Color:
                    if (mat1.GetColor(propertyName) != mat2.GetColor(propertyName))
                    {
                        differences.Add($"Color {propertyName}");
                        return false;
                    }
                    break;
                case ShaderUtil.ShaderPropertyType.Vector:
                    if (mat1.GetVector(propertyName) != mat2.GetVector(propertyName))
                    {
                        differences.Add($"Vector {propertyName}");
                        return false;
                    }
                    break;
                case ShaderUtil.ShaderPropertyType.Float:
                case ShaderUtil.ShaderPropertyType.Range:
                    if (mat1.GetFloat(propertyName) != mat2.GetFloat(propertyName))
                    {
                        differences.Add($"Float {propertyName}");
                        return false;
                    }
                    break;
                case ShaderUtil.ShaderPropertyType.TexEnv:  // Handle texture properties
                    if (mat1.GetTexture(propertyName) != mat2.GetTexture(propertyName))
                    {
                        differences.Add($"Texture {propertyName}");
                        return false;
                    }
                    break;
            }
        }
        return true;
    }
}