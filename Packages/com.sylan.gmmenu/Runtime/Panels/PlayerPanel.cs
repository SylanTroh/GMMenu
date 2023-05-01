
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
        [NonSerialized, FieldChangeCallback(nameof(playerPermissions))] 
        public PlayerPermissions _playerPermissions;

        [FieldChangeCallback(nameof(watchCamera))]
        WatchCamera _watchCamera;

        [NotNull] public Text playerName;
        [NotNull] public RawImage image;
        int thumbnailID = 0;

        [NotNull, SerializeField] GameObject summonButton, confirmSummonButton;
        public VRCPlayerApi player
        {
            set
            {
                thumbnailID = watchCamera.GetThumbnailID(value);
                _player = value;
            }
            get => _player;
        }
        public PlayerPermissions playerPermissions
        {
            set
            {
                _playerPermissions = value;
                _playerPermissions.AddListener(this);
                confirmSummonButton.SetActive(false);
                if (_playerPermissions.getPermissionLevel() >= 2) summonButton.SetActive(true);
                else summonButton.SetActive(false);
            }
            get => _playerPermissions;
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

            if(!gameObject.activeSelf) gameObject.SetActive(true);
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
        public void SummonPlayer()
        {
            if (!Utilities.IsValid(player))
            {
                Debug.Log("[PlayerPanel]: Invalid Summon Target");
                return;
            }
            teleporter.SummonPlayer(player);
            UnConfirmSummon();
        }
        public void ConfirmSummon()
        {
            summonButton.SetActive(false);
            confirmSummonButton.SetActive(true);
            SendCustomEventDelayedSeconds(nameof(UnConfirmSummon), 5.0f);
        }
        public void UnConfirmSummon()
        {
            summonButton.SetActive(true);
            confirmSummonButton.SetActive(false);
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
        public void OnPermissionUpdate()
        {
            confirmSummonButton.SetActive(false);
            if (_playerPermissions.getPermissionLevel() >= 2) summonButton.SetActive(true);
            else summonButton.SetActive(false);
        }
    }
}
