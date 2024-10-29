#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDKBase;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MMMaellon.LightSync
{
    [CustomEditor(typeof(LightSync), true), CanEditMultipleObjects]

    public class LightSyncEditor : Editor
    {
        public static bool foldoutOpen = false;

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
            {
                return;
            }
            int syncCount = 0;
            int pickupSetupCount = 0;
            int rigidSetupCount = 0;
            int respawnYSetupCount = 0;
            int stateSetupCount = 0;
            foreach (var t in targets)
            {
                var sync = (LightSync)t;
                if (!Utilities.IsValid(sync))
                {
                    continue;
                }
                syncCount++;
                if (sync.pickup != sync.GetComponent<VRC_Pickup>())
                {
                    pickupSetupCount++;
                }
                if (sync.rigid != sync.GetComponent<Rigidbody>())
                {
                    rigidSetupCount++;
                }
                if (Utilities.IsValid(VRC_SceneDescriptor.Instance) && !Mathf.Approximately(VRC_SceneDescriptor.Instance.RespawnHeightY, sync.respawnHeight))
                {
                    respawnYSetupCount++;
                }
                LightSyncState[] stateComponents = sync.GetComponents<LightSyncState>();
                if (sync.customStates.Length != stateComponents.Length)
                {
                    stateSetupCount++;
                }
                else
                {
                    bool errorFound = false;
                    foreach (LightSyncState state in sync.customStates)
                    {
                        if (state == null || state.sync != sync || state.stateID < 0 || state.stateID >= sync.customStates.Length || sync.customStates[state.stateID] != state)
                        {
                            errorFound = true;
                            break;
                        }
                    }
                    if (!errorFound)
                    {
                        if (sync.enterFirstCustomStateOnStart && stateComponents.Length > 0 && stateComponents[0].stateID != 0)
                        {
                            errorFound = true;
                        }
                        else
                        {
                            foreach (LightSyncState state in stateComponents)
                            {
                                if (state != null && (state.sync != sync || state.stateID < 0 || state.stateID >= sync.customStates.Length || sync.customStates[state.stateID] != state))
                                {
                                    errorFound = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!errorFound)
                    {
                        errorFound = sync.enterFirstCustomStateOnStart && sync.state < 0;
                    }
                    if (!errorFound)
                    {
                        errorFound = !sync.enterFirstCustomStateOnStart && sync.state >= 0;
                    }
                    if (errorFound)
                    {
                        stateSetupCount++;
                    }
                }
            }
            if (pickupSetupCount > 0 || rigidSetupCount > 0 || stateSetupCount > 0)
            {
                if (pickupSetupCount == 1)
                {
                    EditorGUILayout.HelpBox(@"Object not set up for VRC_Pickup", MessageType.Warning);
                }
                else if (pickupSetupCount > 1)
                {
                    EditorGUILayout.HelpBox(pickupSetupCount.ToString() + @" Objects not set up for VRC_Pickup", MessageType.Warning);
                }
                if (rigidSetupCount == 1)
                {
                    EditorGUILayout.HelpBox(@"Object not set up for Rigidbody", MessageType.Warning);
                }
                else if (rigidSetupCount > 1)
                {
                    EditorGUILayout.HelpBox(rigidSetupCount.ToString() + @" Objects not set up for Rigidbody", MessageType.Warning);
                }
                if (stateSetupCount == 1)
                {
                    EditorGUILayout.HelpBox(@"States misconfigured", MessageType.Warning);
                }
                else if (stateSetupCount > 1)
                {
                    EditorGUILayout.HelpBox(stateSetupCount.ToString() + @" LightSyncs with misconfigured States", MessageType.Warning);
                }
                if (GUILayout.Button(new GUIContent("Auto Setup")))
                {
                    SetupLightSyncs(targets);
                }
            }
            if (respawnYSetupCount > 0)
            {
                if (respawnYSetupCount == 1)
                {
                    EditorGUILayout.HelpBox(@"Respawn Height is different from the scene descriptor's: " + VRC_SceneDescriptor.Instance.RespawnHeightY, MessageType.Info);
                }
                else if (respawnYSetupCount > 1)
                {
                    EditorGUILayout.HelpBox(respawnYSetupCount.ToString() + @" Objects have a Respawn Height that is different from the scene descriptor's: " + VRC_SceneDescriptor.Instance.RespawnHeightY, MessageType.Info);
                }
                if (GUILayout.Button(new GUIContent("Match Scene Respawn Height")))
                {
                    MatchRespawnHeights(targets);
                }
            }

            EditorGUILayout.Space();
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            foldoutOpen = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutOpen, "Advanced Settings");
            if (foldoutOpen)
            {
                if (GUILayout.Button(new GUIContent("Force Setup")))
                {
                    ForceSetup(targets);
                }
                ShowAdvancedOptions();
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (foldoutOpen)
            {
                EditorGUILayout.Space();
                ShowInternalObjects();
            }
        }

        readonly string[] serializedPropertyNames = {
        "debugLogs",
        "showInternalObjects",
        "enterFirstCustomStateOnStart",
        "unparentInternalDataObject",
        "kinematicWhileAttachedToPlayer",
        "useWorldSpaceTransforms",
        "useWorldSpaceTransformsWhenHeldOrAttachedToPlayer",
        "syncCollisions",
        "syncParticleCollisions",
        "allowOutOfOrderData",
        "takeOwnershipOfOtherObjectsOnCollision",
        "allowOthersToTakeOwnershipOnCollision",
        "positionDesyncThreshold",
        "rotationDesyncThreshold",
        "minimumSleepFrames",
        };

        readonly string[] serializedInternalObjectNames = {
        "data",
        "looper",
        "fixedLooper",
        "lateLooper",
        "rigid",
        "pickup",
        "customStates",
        "_behaviourEventListeners",
        "_classEventListeners",
        };

        IEnumerable<SerializedProperty> serializedProperties;
        IEnumerable<SerializedProperty> serializedInternalObjects;
        public void OnEnable()
        {
            serializedProperties = serializedPropertyNames.Select(propName => serializedObject.FindProperty(propName));
            serializedInternalObjects = serializedInternalObjectNames.Select(propName => serializedObject.FindProperty(propName));
        }
        void ShowAdvancedOptions()
        {
            EditorGUI.indentLevel++;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_state"));
            GUI.enabled = true;
            foreach (var property in serializedProperties)
            {
                if (property != null)
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
            EditorGUI.indentLevel--;
        }

        void ShowInternalObjects()
        {
            EditorGUILayout.LabelField("Internal Objects", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUI.indentLevel++;
            foreach (var property in serializedInternalObjects)
            {
                if (property != null)
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
            EditorGUI.indentLevel--;
            GUI.enabled = true;
        }

        public static void SetupLightSyncs(Object[] objects)
        {
            bool syncFound = false;
            foreach (var obj in objects)
            {
                var sync = (LightSync)obj;
                if (Utilities.IsValid(sync))
                {
                    syncFound = true;
                    sync.AutoSetup();
                }
            }

            if (!syncFound)
            {
                Debug.LogWarningFormat("[LightSync] Auto Setup failed: No LightSync selected");
            }
        }
        public static void ForceSetup(Object[] objects)
        {
            foreach (var obj in objects)
            {
                var sync = (LightSync)obj;
                if (Utilities.IsValid(sync))
                {
                    sync.ForceSetup();
                }
            }
        }

        public static void MatchRespawnHeights(Object[] objects)
        {
            bool syncFound = false;
            foreach (var obj in objects)
            {
                var sync = (LightSync)obj;
                if (Utilities.IsValid(sync))
                {
                    syncFound = true;
                    sync.respawnHeight = VRC_SceneDescriptor.Instance.RespawnHeightY;
                }
            }

            if (!syncFound)
            {
                Debug.LogWarningFormat("[LightSync] Auto Setup failed: No LightSync selected");
            }
        }
        public void OnDestroy()
        {
            if (target == null)
            {
                CleanHelperObjects();
            }
        }

        public void CleanHelperObjects()
        {
            foreach (var data in FindObjectsOfType<LightSyncData>())
            {
                if (data.sync == null || data.sync.data != data)
                {
                    data.StartCoroutine(data.Destroy());
                }
            }
            foreach (var looper in FindObjectsOfType<LightSyncLooperUpdate>())
            {
                if (looper.sync == null || looper.sync.looper != looper)
                {
                    looper.StartCoroutine(looper.Destroy());
                }
            }
            foreach (var stateData in FindObjectsOfType<LightSyncStateData>())
            {
                if (stateData.state == null || stateData.state.data != stateData)
                {
                    stateData.StartCoroutine(stateData.Destroy());
                }
            }
            foreach (var enhancementData in FindObjectsOfType<LightSyncEnhancementData>())
            {
                if (enhancementData.enhancement == null || enhancementData.enhancement.enhancementData != enhancementData)
                {
                    enhancementData.StartCoroutine(enhancementData.Destroy());
                }
            }
        }
    }

    [InitializeOnLoad]
    public class LightSyncBuildHandler : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 0;
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }
            AutoSetup();
        }

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (!EditorPrefs.GetBool(autoSetupKey, true))
            {
                return true;
            }
            ClearOrphanedObjects();
            return AutoSetup();
        }

        public static void SetupLightSyncListener(LightSyncListener listener)
        {
            if (!Utilities.IsValid(listener))
            {
                return;
            }
            if (!IsEditable(listener))
            {
                Debug.LogErrorFormat(listener, "<color=red>[LightSync AutoSetup]: ERROR</color> {0}", "LightSyncListener is not editable");
            }
            else
            {
                listener.AutoSetup();
                new SerializedObject(listener).Update();
                PrefabUtility.RecordPrefabInstancePropertyModifications(listener);
            }
        }
        public static void SetupLightSync(LightSync sync)
        {
            if (!Utilities.IsValid(sync))
            {
                return;
            }
            if (!IsEditable(sync))
            {
                Debug.LogErrorFormat(sync, "<color=red>[LightSync AutoSetup]: ERROR</color> {0}", "LightSync is not editable");
            }
            else
            {
                sync.AutoSetup();
            }
        }
        public static bool IsEditable(Component component)
        {
            return !EditorUtility.IsPersistent(component.transform.root.gameObject) && !(component.gameObject.hideFlags == HideFlags.NotEditable || component.gameObject.hideFlags == HideFlags.HideAndDontSave);
        }
        private const string MenuItemPath = "MMMaellon/LightSync/Automatically run setup";
        private const string autoSetupKey = "MyToggleFeatureEnabled";
        [MenuItem(MenuItemPath)]
        private static void ToggleAutoSetup()
        {
            var autoSetupOn = EditorPrefs.GetBool(autoSetupKey, true);
            autoSetupOn = !autoSetupOn;
            Menu.SetChecked(MenuItemPath, autoSetupOn);
            EditorPrefs.SetBool(autoSetupKey, autoSetupOn);
        }

        [MenuItem(MenuItemPath, true)]
        private static bool ValidateToggleAutoSetup()
        {
            var autoSetupOn = EditorPrefs.GetBool(autoSetupKey, true);
            Menu.SetChecked(MenuItemPath, autoSetupOn);
            return true;
        }

        [MenuItem("MMMaellon/LightSync/Run setup")]
        public static bool AutoSetup()
        {
            Debug.Log("Running LightSync AutoSetup");
            foreach (LightSyncListener listener in GameObject.FindObjectsOfType<LightSyncListener>(true))
            {
                SetupLightSyncListener(listener);
            }
            foreach (LightSync sync in GameObject.FindObjectsOfType<LightSync>(true))
            {
                SetupLightSync(sync);
            }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            return true;
        }

        [MenuItem("MMMaellon/LightSync/Clear Orphaned Internal Objects")]
        public static void ClearOrphanedObjects()
        {
            foreach (LightSyncData data in GameObject.FindObjectsOfType<LightSyncData>(true))
            {
                if (!data)
                {
                    continue;
                }
                if (!data.sync || data.sync.data != data)
                {
                    GameObject.DestroyImmediate(data.gameObject);
                }
            }
            foreach (LightSyncLooper looper in GameObject.FindObjectsOfType<LightSyncLooperUpdate>(true))
            {
                if (!looper)
                {
                    continue;
                }
                if (!looper.sync || looper.sync.looper != looper)
                {
                    GameObject.DestroyImmediate(looper.gameObject);
                }
            }
            foreach (LightSyncEnhancementData data in GameObject.FindObjectsOfType<LightSyncEnhancementData>(true))
            {
                if (!data)
                {
                    continue;
                }
                if (!data.enhancement || data.enhancement.enhancementData != data)
                {
                    GameObject.DestroyImmediate(data.gameObject);
                }
            }
            foreach (LightSyncStateData data in GameObject.FindObjectsOfType<LightSyncStateData>(true))
            {
                if (!data)
                {
                    continue;
                }
                if (!data.state || data.state.stateData != data)
                {
                    GameObject.DestroyImmediate(data.gameObject);
                }
            }
        }

        [MenuItem("MMMaellon/LightSync/Show all hidden gameobjects")]
        public static bool ShowAllHiddenGameObjects()
        {
            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>(true))
            {
                if (obj.hideFlags == HideFlags.HideInHierarchy)
                {
                    obj.hideFlags = HideFlags.None;
                }
            }
            return true;
        }
    }
}
#endif