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
}