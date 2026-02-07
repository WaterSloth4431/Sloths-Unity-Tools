using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

public class SlothsLODManager : EditorWindow
{
    private string masterParentName = "";
    private string path = string.Empty;
    private bool includeSubdirectories = true;
    private float maxCullRatio = 1.0f;
    
    //[MenuItem("Tools/Sloth's Auto LOD Setup Tools")]
    public static void ShowWindow()
    {
        SlothsLODManager window = GetWindow<SlothsLODManager>("Sloth's Auto LOD Setup");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Sloth's Auto LOD Setup Tools", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("LOD Group Creation", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Creates LOD Groups for all selected objects. Groups all selected objects into master parent object.", MessageType.Info);
        masterParentName = EditorGUILayout.TextField("Master Parent Name", masterParentName);

        if (GUILayout.Button("Create LOD Group"))
        {
            CreateLODGroup();
        }

        GUILayout.Label("Cull Ratio Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Searches Directory for prefabs with LODS, Sets all LODS found cull ratio to equal object size, slider allow max cull ratio override.", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Select Directory");
        if (GUILayout.Button("Browse"))
        {
            path = EditorUtility.OpenFolderPanel("Select Directory", "", "");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Current Path");
        EditorGUILayout.LabelField(path);
        EditorGUILayout.EndHorizontal();

        includeSubdirectories = EditorGUILayout.Toggle("Include Subdirectories", includeSubdirectories);
        maxCullRatio = EditorGUILayout.Slider("Max Cull Ratio (0 to 1)", maxCullRatio, 0f, 1f);

        if (GUILayout.Button("Apply Cull Ratio"))
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                PerformActionOnPrefabs(path, includeSubdirectories, SetCullRatioForPrefab);
            }
        }
    }

    public void CreateLODGroup()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (string.IsNullOrEmpty(masterParentName))
        {
            masterParentName = "MasterLODParent"; 
        }

        GameObject masterParent = new GameObject(masterParentName);

        foreach (GameObject obj in selectedObjects)
        {
            GameObject parentObj = new GameObject(obj.name + "_LODGroupParent");
            parentObj.transform.position = obj.transform.position;
            parentObj.transform.rotation = obj.transform.rotation;
            parentObj.transform.localScale = obj.transform.localScale;

            obj.transform.parent = parentObj.transform;

            LODGroup lodGroup = parentObj.AddComponent<LODGroup>();

            LOD[] lods = new LOD[1];
            Renderer[] renderers = new Renderer[1];
            renderers[0] = obj.GetComponent<Renderer>(); 

            lods[0] = new LOD(1f, renderers);
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();

            parentObj.transform.SetParent(masterParent.transform);
        }

        SetStatic(masterParent);
        Debug.Log("LOD Groups created under " + masterParentName);
    }

    private void SetStatic(GameObject masterParent)
    {
        GameObjectUtility.SetStaticEditorFlags(masterParent, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic);

        foreach (Transform child in masterParent.transform)
        {
            GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic);
        }
    }

    void PerformActionOnPrefabs(string path, bool includeSubdirectories, Action<GameObject> action)
    {
        string[] files = Directory.GetFiles(path, "*.prefab", includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            GameObject prefabGO = PrefabUtility.LoadPrefabContents(file);

            if (prefabGO != null)
            {
                action(prefabGO);

                PrefabUtility.SaveAsPrefabAsset(prefabGO, file);
                PrefabUtility.UnloadPrefabContents(prefabGO);
            }
        }
    }

    void SetCullRatioForPrefab(GameObject prefab)
    {
        LODGroup[] lodGroups = prefab.GetComponentsInChildren<LODGroup>(true);

        if (lodGroups == null || lodGroups.Length <= 0)
        {
            return;
        }

        float referenceSize = 100.0f; // Adjust based on expected object size range

        foreach (var lodGroup in lodGroups)
        {
            LOD[] lods = lodGroup.GetLODs();

            for (int i = 0; i < lods.Length; i++)
            {
                MeshFilter[] meshFilters = lods[i].renderers.Select(r => r.GetComponent<MeshFilter>()).ToArray();
                foreach (var meshFilter in meshFilters)
                {
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        Bounds meshBounds = meshFilter.sharedMesh.bounds;
                        float largestDimension = Mathf.Max(meshBounds.size.x, meshBounds.size.y, meshBounds.size.z);
                        float normalizedCullRatio = Mathf.Clamp01(largestDimension / referenceSize);
                        lods[i].screenRelativeTransitionHeight = Mathf.Min(normalizedCullRatio, maxCullRatio);
                    }
                }
            }

            lodGroup.SetLODs(lods);
        }
    }
}
