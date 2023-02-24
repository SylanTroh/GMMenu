
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerActionsPanel : UdonSharpBehaviour
    {
        [NotNull] MessageSyncManager messageSyncManager;
        [SerializeField, NotNull] Text status;
        void Start()
        {
            messageSyncManager = Utils.Modules.MessageSyncManager(transform);

            SendCustomEventDelayedSeconds("EnableNewMessageListener", 0.0f);
        }
        public void EnableNewMessageListener()
        {
            messageSyncManager.AddListener(this);
        }
        public void SetMessageEmergency()
        {
            if(messageSyncManager.GetLocalMessageValue() == MessageData.MESSAGE_URGENT)
                messageSyncManager.SetMessage(Networking.LocalPlayer, MessageData.MESSAGE_NULL);
            else
                messageSyncManager.SetMessage(Networking.LocalPlayer, MessageData.MESSAGE_URGENT);
            SetStatus();
        }
        public void SetMessageRoll()
        {
            if (messageSyncManager.GetLocalMessageValue() == MessageData.MESSAGE_ROLL)
                messageSyncManager.SetMessage(Networking.LocalPlayer, MessageData.MESSAGE_NULL);
            else
                messageSyncManager.SetMessage(Networking.LocalPlayer, MessageData.MESSAGE_ROLL);
            SetStatus();
        }
        public void SetMessageQuestion()
        {
            if (messageSyncManager.GetLocalMessageValue() == MessageData.MESSAGE_QUESTION)
                messageSyncManager.SetMessage(Networking.LocalPlayer, MessageData.MESSAGE_NULL);
            else
                messageSyncManager.SetMessage(Networking.LocalPlayer, MessageData.MESSAGE_QUESTION);
            SetStatus();
        }
        public void SetMessageSilent()
        {
            if (messageSyncManager.GetLocalMessageValue() == MessageData.MESSAGE_SILENT)
                messageSyncManager.SetMessage(Networking.LocalPlayer, MessageData.MESSAGE_NULL);
            else
                messageSyncManager.SetMessage(Networking.LocalPlayer, MessageData.MESSAGE_SILENT);
            SetStatus();
        }
        public void OnMessageUpdate()
        {
            SetStatus();
        }
        void SetStatus()
        {
            if(messageSyncManager.GetLocalMessageValue() == MessageData.MESSAGE_NULL)
            {
                status.text = "";
                return;
            }
            if (!messageSyncManager.GetLocalMessage().IsRead())
            {
                status.text = PrintStatus(messageSyncManager.GetLocalMessageValue());
                status.text += "Waiting for GM";
                return;
            }
            if(Time.time-messageSyncManager.GetLocalMessage().timeRead < 60.0f)
            {
                status.text = "GM Present";
                return;
            }
            status.text = "";
        }
        public string PrintStatus(int message)
        {
            switch (message)
            {
                case MessageData.MESSAGE_EMPTY:
                    return "";
                case MessageData.MESSAGE_URGENT:
                    return "Urgent assistance requested: ";
                case MessageData.MESSAGE_ROLL:
                    return "Need a roll: ";
                case MessageData.MESSAGE_QUESTION:
                    return "Question: ";
                case MessageData.MESSAGE_SILENT:
                    return "Join Silently: ";

                default:
                    return "Invalid Messge.";
            }
        }
    }
}