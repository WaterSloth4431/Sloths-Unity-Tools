using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SlothMaterialSlotEditor : EditorWindow
{
    //[MenuItem("Tools/Sloth/Material Slot Editor")]
    public static void ShowWindow()
    {
        GetWindow<SlothMaterialSlotEditor>("Sloth's Material Slot Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Tool that exposes and allows direct editing of individual material slots on a selected GameObject and all its child renderers, bypassing Unity’s limited default material UI.", MessageType.Info);
        var obj = Selection.activeGameObject;

        if (!obj)
        {
            EditorGUILayout.HelpBox("Select a GameObject in the scene.", MessageType.Info);
            return;
        }

        GUILayout.Label("Selected: " + obj.name, EditorStyles.boldLabel);
        GUILayout.Space(6);

        var renderers = obj.GetComponentsInChildren<Renderer>(true);

        foreach (var r in renderers)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label(r.GetType().Name + " : " + r.name, EditorStyles.boldLabel);

            var mats = r.sharedMaterials;

            for (int i = 0; i < mats.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label($"Slot {i}", GUILayout.Width(55));

                mats[i] = (Material)EditorGUILayout.ObjectField(mats[i], typeof(Material), false);

                EditorGUILayout.EndHorizontal();
            }

            if (GUI.changed)
            {
                Undo.RecordObject(r, "Change Material Slot");
                r.sharedMaterials = mats;
                EditorUtility.SetDirty(r);
            }

            EditorGUILayout.EndVertical();
        }
    }
}
