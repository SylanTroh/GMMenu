
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MessageHUD : UdonSharpBehaviour
    {
        [NotNull] Transform hudOffset;
        [NotNull] Canvas canvas;
        [NotNull] RectTransform canvasTransform;

        [NotNull] MessageSyncManager messageSyncManager;
        [NotNull] private PlayerPermissions permissions;

        public GameObject hudRoll;
        public GameObject hudQuestion;
        public GameObject hudEmergency;

        void Start()
        {
            permissions = Utils.Modules.PlayerPermissions(transform);
            messageSyncManager = Utils.Modules.MessageSyncManager(transform);
            hudOffset = transform.Find("HUDOffset");
            canvas = hudOffset.Find("HUDCanvas").GetComponent<Canvas>();
            canvasTransform = canvas.GetComponent<RectTransform>();
            SetCanvasTransform();

            SendCustomEventDelayedSeconds("EnableNewMessageListener", 0.0f);
            SendCustomEventDelayedSeconds("EnablePermissionEventListener", 0.0f);
        }
        public void EnableNewMessageListener()
        {
            messageSyncManager.AddListener(this);
        }
        public void EnablePermissionEventListener()
        {
            permissions.AddListener(this);
        }
        void Update()
        {
            UpdatePosition();
            UpdateRotation();
        }
        void UpdatePosition()
        {
            var headlocation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            transform.position = headlocation.position;
        }
        public void UpdateRotation()
        {
            var headlocation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            transform.rotation = headlocation.rotation;
        }
        void SetCanvasTransform()
        {
            canvasTransform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            canvasTransform.localScale = new Vector3(0.00125f, 0.00125f, 0.00125f);
            hudOffset.localPosition = new Vector3(0.42f, -0.34f, 1.0f);
            hudOffset.localRotation.SetLookRotation(-hudOffset.localPosition, new Vector3(0, 1, 0)); 
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
                    break;
                case MessageData.MESSAGE_QUESTION:
                    hudQuestion.SetActive(true);
                    break;
                case MessageData.MESSAGE_ROLL:
                    hudRoll.SetActive(true);
                    break;
                default:
                    break;
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
                    break;
                case MessageData.MESSAGE_QUESTION:
                    hudQuestion.SetActive(true);
                    break;
                case MessageData.MESSAGE_ROLL:
                    hudRoll.SetActive(true);
                    break;
                default:
                    break;
            }
        }
        void UpdateHUD()
        {
            if (permissions.getPermissionLevel() == PlayerPermissions.PERMISSION_GM)
            {
                UpdateHUDGM();
                return;
            }
            UpdateHUDLocal();
        }
        void DisableHUD()
        {
            foreach (Transform t in canvas.transform)
            {
                t.gameObject.SetActive(false);
            }
        }
        public void OnNewMessage()
        {
            UpdateHUD();
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
