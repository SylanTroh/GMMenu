using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MessageHUD : GMMenuPart
    {
        MessageSyncManager messageSyncManager;
        private PlayerPermissions permissions;

        public GameObject hudSilent;
        public GameObject hudRoll;
        public GameObject hudQuestion;
        public GameObject hudEmergency;
        public GameObject hudGMRadio;

        public GameObject hudNewMessage;
        int numberOfBlinks = 0;

        public GameObject hud;
        public Transform messageIcons;

        void Start()
        {
            permissions = gmMenu.PlayerPermissions;
            messageSyncManager = gmMenu.MessageSyncManager;

            messageSyncManager.AddListener(this);
            permissions.AddListener(this);
        }
        void UpdateHUDGM()
        {
            DisableHUD();
            MessageData[] messages = messageSyncManager.GetMessages();
            if (!Utilities.IsValid(messages)) return;
            MessageData firstMessage = null;

            foreach (MessageData message in messages)
            {
                if (message.IsRead()) continue;
                firstMessage = message;
                break;
            }
            if (!Utilities.IsValid(firstMessage)) return;
            switch (firstMessage.message)
            {
                case MessageData.MESSAGE_URGENT:
                    hudEmergency.SetActive(true);
                    hud.SetActive(true);
                    return;
                case MessageData.MESSAGE_QUESTION:
                    hudQuestion.SetActive(true);
                    hud.SetActive(true);
                    return;
                case MessageData.MESSAGE_ROLL:
                    hudRoll.SetActive(true);
                    hud.SetActive(true);
                    return;
                case MessageData.MESSAGE_SILENT:
                    hudSilent.SetActive(true);
                    hud.SetActive(true);
                    return;
                case MessageData.MESSAGE_GMRADIO:
                    hudGMRadio.SetActive(true);
                    hud.SetActive(true);
                    return;
                default:
                    return;
            }
        }
        void UpdateHUDLocal()
        {
            DisableHUD();
            var message = messageSyncManager.GetLocalMessage();
            if (!Utilities.IsValid(message)) return;
            if (message.IsRead()) return;
            switch (message.message)
            {
                case MessageData.MESSAGE_URGENT:
                    hudEmergency.SetActive(true);
                    hud.SetActive(true);
                    return;
                case MessageData.MESSAGE_QUESTION:
                    hudQuestion.SetActive(true);
                    hud.SetActive(true);
                    return;
                case MessageData.MESSAGE_ROLL:
                    hudRoll.SetActive(true);
                    hud.SetActive(true);
                    return;
                case MessageData.MESSAGE_SILENT:
                    hudSilent.SetActive(true);
                    hud.SetActive(true);
                    return;
                case MessageData.MESSAGE_GMRADIO:
                    hudGMRadio.SetActive(true);
                    hud.SetActive(true);
                    return;
                default:
                    return;
            }
        }
        void UpdateHUD()
        {
            if(permissions.getPermissionLevel() == PlayerPermissions.PERMISSION_DISABLED)
            {
                DisableHUD() ;
                return;
            }
            if (permissions.getPermissionLevel() == PlayerPermissions.PERMISSION_GM)
            {
                UpdateHUDGM();
                return;
            }
            UpdateHUDLocal();
        }
        void DisableHUD()
        {
            foreach (Transform t in messageIcons)
            {
                t.gameObject.SetActive(false);
            }
            hud.SetActive(false);
        }
        public void OnNewMessage()
        {
            UpdateHUD();
            if (permissions.getPermissionLevel() < 2) return;

            if (numberOfBlinks != 0)
            {
                numberOfBlinks = Mathf.Min(numberOfBlinks+3,4);
                return;
            }
            numberOfBlinks = 3;
            SendCustomEventDelayedSeconds(nameof(NewMessageOff), 2.0f);
            hudNewMessage.SetActive(true);

        }
        public void NewMessageOff()
        {
            numberOfBlinks--;
            if(numberOfBlinks == 0) hudNewMessage.SetActive(false);
            else SendCustomEventDelayedSeconds(nameof(NewMessageOff), 2.0f);
        }
        public void OnMessageUpdate()
        {
            UpdateHUD();
        }
        public void OnPermissionUpdate()
        {
            UpdateHUD();
        }
    }
}
