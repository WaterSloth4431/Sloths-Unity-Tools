using System.IO;
using UnityEditor;
using UnityEngine;

public class RenderTexturePngExporterWindow : EditorWindow
{
    private RenderTexture renderTexture;
    private string outputFolder = "";
    private bool forceCustomRenderTextureUpdate = true;

    //[MenuItem("Tools/Export/RenderTexture to PNG (Window)")]
    public static void ShowWindow()
    {
        var window = GetWindow<RenderTexturePngExporterWindow>("Sloth's RT -> PNG Exporter");
        window.minSize = new Vector2(420, 170);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("RenderTexture / CustomRenderTexture → PNG", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            renderTexture = (RenderTexture)EditorGUILayout.ObjectField(
                "Render Texture",
                renderTexture,
                typeof(RenderTexture),
                allowSceneObjects: false
            );

            forceCustomRenderTextureUpdate = EditorGUILayout.ToggleLeft(
                "Force CustomRenderTexture.Update() before export",
                forceCustomRenderTextureUpdate
            );

            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Output Folder", GUILayout.Width(90));
                EditorGUILayout.SelectableLabel(
                    string.IsNullOrEmpty(outputFolder) ? "(not set)" : outputFolder,
                    EditorStyles.textField,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)
                );

                if (GUILayout.Button("Browse...", GUILayout.Width(90)))
                {
                    string picked = EditorUtility.OpenFolderPanel(
                        "Choose output folder",
                        string.IsNullOrEmpty(outputFolder) ? Application.dataPath : outputFolder,
                        ""
                    );

                    if (!string.IsNullOrEmpty(picked))
                        outputFolder = picked;
                }
            }
        }

        EditorGUILayout.Space(8);

        using (new EditorGUI.DisabledScope(renderTexture == null || string.IsNullOrEmpty(outputFolder)))
        {
            if (GUILayout.Button("Export to PNG", GUILayout.Height(32)))
            {
                ExportToPng();
            }
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.HelpBox(
            "Tip: Select a CustomRenderTexture asset here. This will read its current GPU contents into a Texture2D and write a PNG.",
            MessageType.Info
        );
    }

    private void ExportToPng()
    {
        if (renderTexture == null)
        {
            EditorUtility.DisplayDialog("Export PNG", "Please select a RenderTexture / CustomRenderTexture.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(outputFolder) || !Directory.Exists(outputFolder))
        {
            EditorUtility.DisplayDialog("Export PNG", "Please choose a valid output folder.", "OK");
            return;
        }

        // If it's a CustomRenderTexture, optionally force a refresh
        if (forceCustomRenderTextureUpdate && renderTexture is CustomRenderTexture crt)
        {
            crt.Update();
        }

        string filename = MakeSafeFilename(renderTexture.name) + ".png";
        string outPath = Path.Combine(outputFolder, filename);

        if (TryExportRenderTextureToPng(renderTexture, outPath))
        {
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Export PNG", $"Exported:\n{outPath}", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Export PNG", "Export failed. Check Console for details.", "OK");
        }
    }

    private static bool TryExportRenderTextureToPng(RenderTexture rt, string filePath)
    {
        if (rt == null) return false;

        int width = rt.width;
        int height = rt.height;

        if (width <= 0 || height <= 0)
        {
            Debug.LogWarning($"Skipping '{rt.name}' because it has invalid size: {width}x{height}");
            return false;
        }

        if (!rt.IsCreated())
            rt.Create();

        // RGBA32 is safest for PNG (supports alpha)
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false, linear: false);

        RenderTexture prev = RenderTexture.active;
        try
        {
            RenderTexture.active = rt;

            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply(updateMipmaps: false, makeNoLongerReadable: false);

            byte[] pngBytes = tex.EncodeToPNG();
            if (pngBytes == null || pngBytes.Length == 0)
            {
                Debug.LogError($"Failed to encode PNG for '{rt.name}'");
                return false;
            }

            File.WriteAllBytes(filePath, pngBytes);
            Debug.Log($"Exported '{rt.name}' -> {filePath}");
            return true;
        }
        catch (System.SystemException e)
        {
            Debug.LogError($"Export failed for '{rt.name}': {e}");
            return false;
        }
        finally
        {
            RenderTexture.active = prev;
            Object.DestroyImmediate(tex);
        }
    }

    private static string MakeSafeFilename(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Trim();
    }
}
