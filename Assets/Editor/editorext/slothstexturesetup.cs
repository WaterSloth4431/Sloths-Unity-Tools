using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public class slothstexturesetup : EditorWindow
{
    private TextureImporterCompression compressionMode = TextureImporterCompression.Compressed;
    private int compressionQuality = 50;

    private static readonly Dictionary<TextureImporterCompression, string> CompressionNameMap = new Dictionary<TextureImporterCompression, string>()
    {
        {TextureImporterCompression.Uncompressed, "Uncompressed"},
        {TextureImporterCompression.CompressedLQ, "Compressed LQ - Low Quality"},
        {TextureImporterCompression.Compressed, "Compressed - Normal Quality"},
        {TextureImporterCompression.CompressedHQ, "Compressed HQ - High Quality"},
    };

    private static readonly int[] MaxTextureSizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };

    //[MenuItem("Tools/Sloth's Texture Setup")]
    public static void ShowWindow()
    {
        GetWindow<slothstexturesetup>("Sloth's Texture Setup");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Tool batch-applies Crunch compression, automatically scales max texture size based on resolution, and configures normal maps using filename detection.", MessageType.Info);
        EditorGUILayout.LabelField("Crunch Compression Settings", EditorStyles.boldLabel);

        List<TextureImporterCompression> availableOptions = Enum.GetValues(typeof(TextureImporterCompression))
            .Cast<TextureImporterCompression>()
            .Where(option => option != TextureImporterCompression.Uncompressed)
            .ToList();

        string[] displayedOptions = availableOptions.Select(option => CompressionNameMap[option]).ToArray();

        int selectedIndex = availableOptions.IndexOf(compressionMode);

        selectedIndex = EditorGUILayout.Popup("Compression Mode", selectedIndex, displayedOptions);

        if (selectedIndex >= 0 && selectedIndex < availableOptions.Count)
        {
            compressionMode = availableOptions[selectedIndex];
        }

        compressionQuality = EditorGUILayout.IntSlider("Compression Quality", compressionQuality, 0, 100);

        if (GUILayout.Button("Apply"))
        {
            ProcessSelectedTextures();
        }
    }

    private void ProcessSelectedTextures()
    {
        foreach (UnityEngine.Object obj in Selection.objects)
        {
            if (obj is Texture2D texture)
            {
                string assetPath = AssetDatabase.GetAssetPath(texture);
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (importer != null)
                {
                    //Apply Crunch Compression
                    importer.textureCompression = compressionMode;
                    importer.crunchedCompression = true;
                    importer.compressionQuality = compressionQuality;

                    // List and Scale texture size
                    importer.maxTextureSize = 16384;
                    AssetDatabase.ImportAsset(assetPath);

                    int width = texture.width;
                    int height = texture.height;
                    Debug.Log(texture.name + " Size: " + width + "x" + height);

                    int maxSize = Mathf.Max(width, height);
                    int newMaxSize = FindClosestMaxSize(maxSize);
                    importer.maxTextureSize = newMaxSize;

                    //Set Normal Maps
                    if (texture.name.EndsWith("_nml") || texture.name.EndsWith("_nm")
                    || texture.name.EndsWith("_normal") || texture.name.EndsWith("_NM")
                    || texture.name.EndsWith("_NML") || texture.name.EndsWith("_NORMAL"))
                    {

                        if (importer.textureType != TextureImporterType.NormalMap)
                        {
                            importer.textureType = TextureImporterType.NormalMap;
                        }
                    }
                    importer.SaveAndReimport();
                }
                else
                {
                    Debug.LogWarning("Could not retrieve TextureImporter for " + texture.name);
                }
            }
        }
    }

    private static int FindClosestMaxSize(int size)
    {
        int closestSize = MaxTextureSizes[0];
        int smallestDifference = Mathf.Abs(size - closestSize);

        for (int i = 1; i < MaxTextureSizes.Length; i++)
        {
            int difference = Mathf.Abs(size - MaxTextureSizes[i]);
            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closestSize = MaxTextureSizes[i];
            }
        }

        return closestSize;
    }
}