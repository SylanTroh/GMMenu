#if VRC_SDK_VRCSDK3 && UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.Core;
using VRCSceneDescriptor = VRC.SDK3.Components.VRCSceneDescriptor;

namespace VRC.SDK3.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VRCSceneDescriptor))]
    public class VRCSceneDescriptorEditor3 : VRCInspectorBase
    {

        private VRCSceneDescriptor sceneDescriptor;

        private SerializedProperty propSpawns;
        private SerializedProperty propSpawnOrder;
        private SerializedProperty propSpawnOrientation;
        private SerializedProperty propReferenceCamera;
        private SerializedProperty propRespawnHeightY;
        private SerializedProperty propObjectBehaviourAtRespawnHeight;
        private SerializedProperty propForbidUserPortals;
        private SerializedProperty propUnityVersion;
        private SerializedProperty propDynamicPrefabs;
        private SerializedProperty propDynamicMaterials;
        private SerializedProperty propNetworkIDs;
        private SerializedProperty propPortraitCameraPositionOffset;
        private SerializedProperty propPortraitCameraRotationOffset;
        private SerializedProperty propInteractPassthrough;

        [NonSerialized] private string[] layerNames;
        private int mask;

        private const string INTERACTION_HELPBOX_LABEL =
            "Interaction through User layers is blocked by default. Use the \"Interact Passthrough\" mask to define layers that will be transparent to interaction (allow interactions to pass through).";
        private const string INTERACTION_HELPBOX_URL = "https://creators.vrchat.com/worlds/layers/#interaction-block-and-passthrough-on-vrchat-layers";
        private const int USER_LAYER_START = 22;
        private const int USER_LAYER_COUNT = 10;



        private void OnEnable()
        {
            sceneDescriptor = (VRCSceneDescriptor)target;

            propSpawns = serializedObject.FindProperty(nameof(VRCSceneDescriptor.spawns));
            propSpawnOrder = serializedObject.FindProperty(nameof(VRCSceneDescriptor.spawnOrder));
            propSpawnOrientation = serializedObject.FindProperty(nameof(VRCSceneDescriptor.spawnOrientation));
            propReferenceCamera = serializedObject.FindProperty(nameof(VRCSceneDescriptor.ReferenceCamera));
            propRespawnHeightY = serializedObject.FindProperty(nameof(VRCSceneDescriptor.RespawnHeightY));
            propObjectBehaviourAtRespawnHeight = serializedObject.FindProperty(nameof(VRCSceneDescriptor.ObjectBehaviourAtRespawnHeight));
            propForbidUserPortals = serializedObject.FindProperty(nameof(VRCSceneDescriptor.ForbidUserPortals));
            propUnityVersion = serializedObject.FindProperty(nameof(VRCSceneDescriptor.unityVersion));
            propDynamicPrefabs = serializedObject.FindProperty(nameof(VRCSceneDescriptor.DynamicPrefabs));
            propDynamicMaterials = serializedObject.FindProperty(nameof(VRCSceneDescriptor.DynamicMaterials));
            propNetworkIDs = serializedObject.FindProperty(nameof(VRCSceneDescriptor.NetworkIDCollection));
            propPortraitCameraPositionOffset = serializedObject.FindProperty(nameof(VRCSceneDescriptor.portraitCameraPositionOffset));
            propPortraitCameraRotationOffset = serializedObject.FindProperty(nameof(VRCSceneDescriptor.portraitCameraRotationOffset));
            propInteractPassthrough = serializedObject.FindProperty(nameof(VRCSceneDescriptor.interactThruLayers));

            PopulateUserLayerNames();
            HierarchyChanged();
            EditorApplication.hierarchyChanged += HierarchyChanged;
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= HierarchyChanged;
        }

        private void HierarchyChanged()
        {
            // This cannot be a RequireComponent because VRC.Core isn't included in VRC.Base.
            if (!sceneDescriptor.GetComponent<PipelineManager>())
            {
                sceneDescriptor.gameObject.AddComponent<PipelineManager>();
            }
            
            if (sceneDescriptor.spawns == null || sceneDescriptor.spawns.Length == 0)
            {
                Undo.RecordObject(sceneDescriptor, "Grabbed new Spawn Position");
                sceneDescriptor.spawns = new[] { sceneDescriptor.transform };
                Debug.LogWarning($"Scene Descriptor spawns were empty, adding a default Spawn.");
            }
        }


        public override void BuildInspectorGUI()
        {
            base.BuildInspectorGUI();
            
            AddField(propSpawns);
            AddField(propSpawnOrder);
            AddField(propSpawnOrientation);
            AddField(propReferenceCamera);
            AddField(propRespawnHeightY);
            AddField(propForbidUserPortals);
            AddField(propObjectBehaviourAtRespawnHeight);
            AddField(propNetworkIDs);
            
            MaskField fieldPassthrough = new ("Interact Passthrough");
            fieldPassthrough.AddToClassList("unity-base-field__aligned");
            fieldPassthrough.choices = layerNames.ToList();
            fieldPassthrough.BindProperty(propInteractPassthrough);
            Root.Add(fieldPassthrough);

            HelpBox helpBox = new (INTERACTION_HELPBOX_LABEL, HelpBoxMessageType.Info);
            
            Button buttonDocs = new () { text = "Docs" };
            buttonDocs.clicked += () => Application.OpenURL(INTERACTION_HELPBOX_URL);
            helpBox.Add(buttonDocs);
            
            Root.Add(helpBox);

        }
        
        private void PopulateUserLayerNames()
        {
            if (layerNames == null)
            {
                layerNames = new string[USER_LAYER_COUNT];
            }

            for (int i = 0; i < USER_LAYER_COUNT; ++i)
            {
                string layerName = LayerMask.LayerToName(USER_LAYER_START + i);
                if (string.IsNullOrWhiteSpace(layerName))
                {
                    layerNames[i] = $"<<layer {USER_LAYER_START + i}>>";
                }
                else
                {
                    layerNames[i] = layerName;
                }
            }
        }
    }
}

#endif