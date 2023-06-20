
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using JetBrains.Annotations;
using UnityEngine.UI;
using System;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MessagePanel : PanelTemplate
    {
        [FieldChangeCallback(nameof(message))]
        MessageData _message;

        [SerializeField,NotNull] Text alertMessage;
        [SerializeField,NotNull] Text timePassed;

        [NotNull,SerializeField] Image backgroundColor;

        public MessageData message
        {
            set
            {
                player = value.owner;
                _message = value;
            }
            get => _message;
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
                isActive = false;
            }
            else isActive = true;
            SetBorderColor();

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
        public void MarkRead()
        {
            message.SyncReadStatus(true);
        }
        public void UndoRead()
        {
            message.SyncReadStatus(false);
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
    }
}
