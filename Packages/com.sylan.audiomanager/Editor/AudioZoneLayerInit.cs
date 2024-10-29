#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class AudioZoneLayerInit : EditorWindow
{
    public const string layerName = "AudioZones";

    private int layerIndex = -1;

    [MenuItem("Tools/Sylan/Initialize AudioZone Layer")]
    private static void ShowWindow()
    {
        GetWindow(typeof(AudioZoneLayerInit));
    }

    private void OnGUI()
    {
        if (FindAudioZoneLayer() != -1)
        {
            GUILayout.Label("AudioZone Layer already exists");
            return;
        }
        layerIndex = EditorGUILayout.IntField("Layer Index", layerIndex);

        if (GUILayout.Button("Find Empty Layer"))
        {
            FindEmptyLayer();
        }

        if (GUILayout.Button("Initialize Layer"))
        {
            if (layerIndex == -1)
            {
                Debug.LogError("Please enter a valid layer index.");
                return;
            }

            if (LayerMask.LayerToName(layerIndex) != "")
            {
                if (EditorUtility.DisplayDialog("Layer Already Exists",
                    "The layer already exists. Are you sure you want to overwrite its settings?", "Yes", "No"))
                {
                    Initialize();
                }
            }
            else
            {
                Initialize();
            }
        }
    }
    private void FindEmptyLayer()
    {
        for (int i = 22; i < 32; i++)
        {
            if (LayerMask.LayerToName(i) == "")
            {
                layerIndex = i;
                Debug.Log("Found empty layer at index " + i + ".");
                return;
            }
        }

        Debug.LogWarning("No empty layer found after index 21.");
    }

    private static int FindAudioZoneLayer()
    {
        int layerIndex = -1;
        for (int i = 22; i < 32; i++)
        {
            if (LayerMask.LayerToName(i) == "AudioZones")
            {
                layerIndex = i;
                Debug.Log("Found AudioZones layer at index " + i + ".");
            }
        }

        Debug.LogWarning("No AudioZones layer found after index 21.");
        return layerIndex;
    }

    private void Initialize()
    {
        // Create the layer if it doesn't exist
        if (LayerMask.LayerToName(layerIndex) == "")
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            layers.GetArrayElementAtIndex(layerIndex).stringValue = "Layer " + layerIndex;
            SerializedProperty layer = layers.GetArrayElementAtIndex(layerIndex);
            layer.stringValue = layerName; // Set the name of the new layer

            tagManager.ApplyModifiedProperties();

            // Set the collision matrix for the layer
            for (int i = 0; i < 32; i++)
            {
                if (i == layerIndex) continue;
                Physics.IgnoreLayerCollision(layerIndex, i, false);
            }
        }
    }
}
#endif