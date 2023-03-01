
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AlertListViewport : UdonSharpBehaviour
    {
        public GameObject AlertTemplate;
        public Transform AlertListContent;
        [NotNull] MessageSyncManager messageSyncManager;
        [NotNull] GMMenuToggle menuToggle;
        [NotNull] Teleporter teleporter;
        [NotNull] WatchCamera watchCamera;

        MessagePanel[] panels = new MessagePanel[0];
 
        private void Start()
        {
            menuToggle = Utils.Modules.GMMenuToggle(transform);
            messageSyncManager = Utils.Modules.MessageSyncManager(transform);
            teleporter = Utils.Modules.Teleporter(transform);
            watchCamera = Utils.Modules.WatchCamera(transform);

            SendCustomEventDelayedSeconds(nameof(EnableMenuToggleListener), 0.0f);
            SendCustomEventDelayedSeconds(nameof(EnableNewMessageListener), 0.0f);
        }

        //Event Listeners
        public void EnableMenuToggleListener()
        {
            menuToggle.AddListener(this);
        }
        public void EnableNewMessageListener()
        {
            messageSyncManager.AddListener(this);
        }

        // Events
        public void OnMenuToggleOn()
        {
            UpdateViewportContinuous();
        }
        public void OnNewMessage()
        {
            UpdateViewport();
        }
        public void OnMessageUpdate()
        {
            UpdateViewport();
        }
        //UI Functions
        public void UpdateViewport()
        {
            MessageData[] messages = messageSyncManager.GetMessages();
            //ClearViewport();
            ShowMessages(messages);
        }
        public void UpdateViewportContinuous()
        {
            UpdateViewport();
            if(menuToggle.MenuState()) SendCustomEventDelayedSeconds(nameof(UpdateViewportContinuous), 5.0f);
        }

        private void ClearViewport()
        {
            foreach (Transform a in AlertListContent)
            {
                Destroy(a.gameObject);
            }
        }

        private void ShowMessages(MessageData[] messages)
        {
            if (!Utilities.IsValid(messages)) return;
            int messageNum = 0;
            foreach (MessageData message in messages)
            {
                if (!Utilities.IsValid(message)) continue;
                if (!Utilities.IsValid(message.owner)) continue;
                //if (message.owner.isLocal) continue;
                if (message.message == MessageData.MESSAGE_NULL) continue;
                if (messageNum >= panels.Length) InstansiateMessagePanel();
                panels[messageNum].DrawPanel(message);
                messageNum++;
            }
            for (int i = messageNum; i < panels.Length; i++)
            {
                panels[i].gameObject.SetActive(false);
            }
        }
        void InstansiateMessagePanel()
        {
            var panel = Instantiate(AlertTemplate, AlertListContent).transform;
            MessagePanel messagePanel = panel.GetComponent<MessagePanel>();
            messagePanel.teleporter = teleporter;
            messagePanel.messageSyncManager = messageSyncManager;
            messagePanel.watchCamera = watchCamera;
            Utils.ArrayUtils.Append(ref panels, messagePanel);
        }
    }
}
