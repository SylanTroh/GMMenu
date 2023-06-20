
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using UnityEngine.UI;
using System;
using System.Threading;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PanelTemplate : GMMenuPart
    {
        [FieldChangeCallback(nameof(player))]
        protected VRCPlayerApi _player;
        [NonSerialized] public Teleporter teleporter;
        [NonSerialized] public MessageSyncManager messageSyncManager;
        [FieldChangeCallback(nameof(watchCamera))]
        protected WatchCamera _watchCamera;

        public RawImage image;
        protected int thumbnailID = 0;

        [SerializeField] protected Image backgroundImage;
        protected Color activeColor = new Color(0.6f, 0.298f, 0.549f, 0.75f);
        protected Color selectedColor = new Color(1.0f, 0.8f, 0.969f, 0.75f);
        protected Color inactiveColor = new Color(0.6f, 0.298f, 0.549f, 0.4f);
        protected bool isActive = true;
        protected bool selected = false;
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
        //Button Onclick funtions
        public void TeleportToPlayer()
        {
            if (!Utilities.IsValid(player))
            {
                Debug.Log("[PlayerPanel]: Invalid Teleport Target");
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
        public void SetBorderColor()
        {
            if(selected)
                backgroundImage.color = selectedColor;
            else if (!isActive)
                backgroundImage.color = inactiveColor;
            else
                backgroundImage.color = activeColor;
        }
    }
}
