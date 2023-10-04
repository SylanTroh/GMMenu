using UdonSharp;
using UnityEditor;
using UnityEngine;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
#endif

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GMMenu : UdonSharpBehaviour
    {
        //Modules
        [Header("Don't Touch")]

        [SerializeField]
        private GMMenuToggle gmMenuToggle;
        public GMMenuToggle GMMenuToggle { get { return gmMenuToggle; } }
        [SerializeField]
        private MessageSyncManager messageSyncManager;
        public MessageSyncManager MessageSyncManager { get { return messageSyncManager; } }
        [SerializeField]
        private VoiceModeManager voiceModeManager;
        public VoiceModeManager VoiceModeManager { get { return voiceModeManager; } }
        [SerializeField]
        private Teleporter teleporter;
        public Teleporter Teleporter { get { return teleporter; } }
        [SerializeField]
        private PlayerPermissions playerPermissions;
        public PlayerPermissions PlayerPermissions { get { return playerPermissions; } }
        [SerializeField]
        private PlayerMover playerMover;
        public PlayerMover PlayerMover { get { return playerMover; } }
        [SerializeField]
        private WatchCamera watchCamera;
        public WatchCamera WatchCamera { get { return watchCamera; } }
        [SerializeField]
        private MenuOpenSettings menuOpenSettings;
        public MenuOpenSettings MenuOpenSettings { get { return menuOpenSettings; } }
        [SerializeField]
        private PlayerSelector playerSelector;
        public PlayerSelector PlayerSelector { get { return playerSelector; } }
        [SerializeField]
        private PlayerListViewport playerListViewport;
        public PlayerListViewport PlayerListViewport { get { return playerListViewport; } }
        [SerializeField]
        private AlertListViewport alertListViewport;
        public AlertListViewport AlertListViewport { get { return alertListViewport; } }
    }
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [InitializeOnLoad]
    public class GMMenuInitialize : Editor, IVRCSDKBuildRequestedCallback
    {
        private static bool SetSerializedProperties()
        {
            GMMenu[] gmMenu = FindObjectsOfType<GMMenu>();
            if (gmMenu.Length > 1)
            {
                Debug.LogError("[EditorUtilities] More than one GMMenu in Scene");
                return false;
            }
            //Object with Serialized Property(s)
            GMMenuPart[] behaviours = FindObjectsOfType<GMMenuPart>();
            foreach (GMMenuPart behaviour in behaviours)
            {
                SerializedObject serializedObject = new SerializedObject(behaviour);
                SerializedProperty property = serializedObject.FindProperty(nameof(GMMenuPart.gmMenu));
                property.objectReferenceValue = gmMenu[0];
                serializedObject.ApplyModifiedProperties();
            }
            return true;
        }
        //
        //Run On Play
        //
        static GMMenuInitialize()
        //Rename Static Constructor to match Class name
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;
            SetSerializedProperties();
        }
        //
        // Run On Build
        //
        public int callbackOrder => 0;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType != VRCSDKRequestedBuildType.Scene) return false;
            return SetSerializedProperties();
        }
    }
#endif
}