
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using JetBrains.Annotations;
using UnityEngine.UI;
using System;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerPanel : UdonSharpBehaviour
    {
        [FieldChangeCallback(nameof(player))]
        VRCPlayerApi _player;
        [NonSerialized] public Teleporter teleporter;
        [NonSerialized] public MessageSyncManager messageSyncManager;
        [FieldChangeCallback(nameof(watchCamera))]
        WatchCamera _watchCamera;

        [NotNull] public Text playerName;
        [NotNull] public RawImage image;
        int thumbnailID = 0;

        private void Start()
        {
            SendCustomEventDelayedSeconds("EnableWatchCameraListener", 0.0f);
        }
        public VRCPlayerApi player
        {
            set
            {
                thumbnailID = watchCamera.GetThumbnailID(value);
                _player = value;
            }
            get => _player;
        }
        public WatchCamera watchCamera
        {
            set
            {
                value.AddListener(this);
                _watchCamera = value;
            }
            get => _watchCamera;
        }
        public void DrawPanel(VRCPlayerApi p)
        {
            player = p;
            name = player.playerId.ToString();
            transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            playerName.text = player.displayName;

            WatchCamera.SetThumbnailUV(image, thumbnailID);

            gameObject.SetActive(true);
        }
        //Button Onclick funtions
        public void TeleportToPlayer()
        {
            if (!Utilities.IsValid(player))
            {
                Debug.Log("[MessagePanel]: Invalid Teleport Target");
                return;
            }
            teleporter.TeleportToPlayer(player);
        }
        public void UndoTeleport()
        {
            teleporter.UndoTeleport();
        }
        public void WatchCameraEnable()
        {
            if (!Utilities.IsValid(player))
            {
                Debug.Log("[MessagePanel]: Invalid Watch Target");
                return;
            }
            watchCamera.WatchPlayer(player);
        }
        public void OnUpdateThumbnailID()
        {
            thumbnailID = watchCamera.GetThumbnailID(player);
        }
    }
}
