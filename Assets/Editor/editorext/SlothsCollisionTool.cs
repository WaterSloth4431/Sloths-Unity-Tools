using UnityEngine;
using UnityEditor;
using System.IO;

public class SlothsCollisionTool : EditorWindow
{
    public GameObject collisionsPrefab;
    private string[] collisionTypes;
    private int selectedTypeIndex = 0;

    public static void ShowWindow()
    {
        GetWindow<SlothsCollisionTool>("Sloth's Collision Tool");
    }

    private void OnEnable()
    {
        AutoSelectCollisionsPrefab();
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Tool that applies a selected collision preset from a Collisions prefab to the currently selected objects by copying its GameMarkerData values and ensuring each object has a MeshCollider.", MessageType.Info);
        collisionsPrefab = (GameObject)EditorGUILayout.ObjectField("Collisions Prefab", collisionsPrefab, typeof(GameObject), false);
        
        if (collisionsPrefab != null)
        {
            LoadCollisionTypes();
            selectedTypeIndex = EditorGUILayout.Popup("Collision Type", selectedTypeIndex, collisionTypes);
            
            if (GUILayout.Button("Apply to Selected Objects"))
            {
                ApplyGameMarkerData();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please assign the Collisions prefab.", MessageType.Warning);
        }
    }

    private void AutoSelectCollisionsPrefab()
    {
        string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
        string scriptFolder = Path.GetDirectoryName(scriptPath);
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:GameObject", new[] { scriptFolder });
        
        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (obj.name == "Collisions")
            {
                collisionsPrefab = obj;
                break;
            }
        }
    }

    private void LoadCollisionTypes()
    {
        int childCount = collisionsPrefab.transform.childCount;
        collisionTypes = new string[childCount];
        
        for (int i = 0; i < childCount; i++)
        {
            collisionTypes[i] = collisionsPrefab.transform.GetChild(i).name;
        }
    }

    private void ApplyGameMarkerData()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected.");
            return;
        }

        Transform selectedCollision = collisionsPrefab.transform.Find(collisionTypes[selectedTypeIndex]);
        if (selectedCollision == null)
        {
            Debug.LogError("Selected collision type not found in prefab.");
            return;
        }

        foreach (GameObject obj in Selection.gameObjects)
        {
            GameMarkerData markerData = obj.GetComponent<GameMarkerData>();
            if (markerData == null)
            {
                markerData = obj.AddComponent<GameMarkerData>();
            }

            // Copy parameters from the corresponding collision type object
            GameMarkerData referenceData = selectedCollision.GetComponent<GameMarkerData>();
            if (referenceData != null)
            {
                EditorUtility.CopySerialized(referenceData, markerData);
            }
            
            // Add MeshCollider if not already present
            if (obj.GetComponent<MeshCollider>() == null)
            {
                obj.AddComponent<MeshCollider>();
            }
            
            Debug.Log($"Applied GameMarkerData from {collisionTypes[selectedTypeIndex]} and added MeshCollider to {obj.name}");
        }
    }
}
