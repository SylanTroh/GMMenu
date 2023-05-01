
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using JetBrains.Annotations;
using UnityEngine.UI;
using System;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MessagePanel : UdonSharpBehaviour
    {
        [FieldChangeCallback(nameof(message))]
        MessageData _message;
        [NonSerialized] public Teleporter teleporter;
        [NonSerialized] public MessageSyncManager messageSyncManager;
        [FieldChangeCallback(nameof(watchCamera))]
        WatchCamera _watchCamera;

        [NotNull] public Text alertMessage;
        [NotNull] public Text timePassed;
        [NotNull] public RawImage image;
        int thumbnailID = 0;

        [NotNull,SerializeField] Image backgroundColor;
        Color activeColor = new Color(217f / 255f, 108f / 255f, 198f / 255f, 191f / 255f);
        Color inactiveColor = new Color(153f / 255f, 76f / 255f, 140f / 255f, 191f / 255f);

        public MessageData message
        {
            set
            {
                thumbnailID = watchCamera.GetThumbnailID(value.owner);
                _message = value;
            }
            get => _message;
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
        public void DrawPanel(MessageData m)
        {
            message = m;
            name = message.owner.playerId.ToString();
            transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            alertMessage.text = "";
            if (message.IsRead())
            { 
                alertMessage.text += "(GM Present)\n";
                backgroundColor.color = inactiveColor;
            }
            else backgroundColor.color = activeColor;

            alertMessage.text += message.PrintMessage();

            timePassed.text = PrettyPrintTime(message);
            SetMarkReadButton();

            WatchCamera.SetThumbnailUV(image, thumbnailID);

            if(!gameObject.activeSelf) gameObject.SetActive(true);
        }
        void SetMarkReadButton()
        {
            transform.Find("VerticalLayout/HorizontalLayout/MarkRead").gameObject.SetActive(!message.isReadLocal);
            transform.Find("VerticalLayout/HorizontalLayout/UndoRead").gameObject.SetActive(message.isReadLocal);
        }
        //Button Onclick funtions
        public void TeleportToPlayer()
        {
            if (!Utilities.IsValid(message.owner))
            {
                Debug.Log("[MessagePanel]: Invalid Teleport Target");
                return;
            }
            teleporter.TeleportToPlayer(message.owner);
        }
        public void UndoTeleport()
        {
            teleporter.UndoTeleport();
        }
        public void MarkRead()
        {
            message.SyncReadStatus(true);
        }
        public void UndoRead()
        {
            message.SyncReadStatus(false);
        }
        public void WatchCameraEnable()
        {
            if (!Utilities.IsValid(message.owner))
            {
                Debug.Log("[MessagePanel]: Invalid Watch Target");
                return;
            }
            watchCamera.WatchPlayer(message.owner);
        }
        string PrettyPrintTime(MessageData message)
        {
            var timePassed = Time.time - message.timeReceived;
            if (timePassed < 60) return Math.Floor(timePassed).ToString() + "s";
            timePassed /= 60;
            if (timePassed < 60) return Math.Floor(timePassed).ToString() + "m";
            timePassed /= 24;
            return Math.Floor(timePassed).ToString() + "h";
        }
        public void OnUpdateThumbnailID()
        {
            thumbnailID = watchCamera.GetThumbnailID(message.owner);
        }
    }
}
