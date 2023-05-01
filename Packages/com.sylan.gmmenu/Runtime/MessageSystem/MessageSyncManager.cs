
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MessageSyncManager : UdonSharpBehaviour
    {
        private MessageData[] messageData;
        private MessageData[] sortedMessages;
        private MessageData localMessage = null;
        private UdonSharpBehaviour[] NewMessageEventListeners = new UdonSharpBehaviour[0];

        private void Start()
        {
            messageData = Utils.Modules.MessageData(transform);
        }
        //Manage Message Data Ownership
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            SetMessageOwnership(player);
        }
        private void SetMessageOwnership(VRCPlayerApi player)
        {
            foreach (MessageData m in messageData)
            {
                if (m.owner != null) continue;

                m.owner = player;
                return;
            }
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            RevokeMessageOwnership(player);
        }
        private void RevokeMessageOwnership(VRCPlayerApi player)
        {
            foreach (MessageData m in messageData)
            {
                if (m.owner != player) continue;

                m.owner = null;
            }
        }
        //Get MessageData that belongs to a specific player, or a list of players
        public MessageData GetMessageByOwner(VRCPlayerApi player)
        {
            foreach (MessageData m in messageData)
            {
                if (m.owner == player) return m;
            }
            return null;
        }
        public MessageData GetLocalMessage()
        {
            return localMessage;
        }
        public int GetLocalMessageValue()
        {
            if (!Utilities.IsValid(localMessage)) return MessageData.MESSAGE_NULL;
            return localMessage.message;
        }
        int CompareMessageTime(MessageData message1, MessageData message2)
        {
            if (message1.timeReceived == message2.timeReceived) return 0;
            if(message1.timeReceived < message2.timeReceived) return -1;
            return 1;
        }
        public MessageData[] GetMessages()
        {
            return sortedMessages;
        }

        public MessageData[] SortMessages()
        {
            //Terrible code to sort messages, because UDON doesn't support built in C# stuff
            var unreadEmergencies = new MessageData[0];
            var readEmergencies = new MessageData[0];

            var unreadNonEmergencies = new MessageData[0];
            var readNonEmergencies = new MessageData[0];

            foreach (MessageData message in messageData)
            {
                if (!Utilities.IsValid(message)) continue;
                if (message.message == MessageData.MESSAGE_NULL) continue;

                if (message.message == MessageData.MESSAGE_URGENT)
                {
                    if (message.IsRead())
                    {
                        Utils.ArrayUtils.Append(ref readEmergencies, message);
                        continue;
                    }
                    Utils.ArrayUtils.Append(ref unreadEmergencies, message);
                    continue;
                }

                if (message.IsRead())
                {
                    Utils.ArrayUtils.Append(ref readNonEmergencies, message);
                    continue;
                }
                Utils.ArrayUtils.Append(ref unreadNonEmergencies, message);
            }

            QuickSortMessages(unreadEmergencies, 0, unreadEmergencies.Length - 1);
            QuickSortMessages(readEmergencies, 0, readEmergencies.Length - 1);
            QuickSortMessages(unreadNonEmergencies, 0, unreadNonEmergencies.Length - 1);
            QuickSortMessages(readNonEmergencies, 0, readNonEmergencies.Length - 1);

            var sortedMessages = new MessageData
                [unreadEmergencies.Length + readEmergencies.Length +
                unreadNonEmergencies.Length + readNonEmergencies.Length];
            var i = 0;

            unreadEmergencies.CopyTo(sortedMessages, i);
            i += unreadEmergencies.Length;
            readEmergencies.CopyTo(sortedMessages, i);
            i += readEmergencies.Length;
            unreadNonEmergencies.CopyTo(sortedMessages, i);
            i += unreadNonEmergencies.Length;
            readNonEmergencies.CopyTo(sortedMessages, i);

            return sortedMessages;
        }

        //Set or Get contents of a message
        public void SetMessage(VRCPlayerApi player, int message)
        {
            var m = GetMessageByOwner(player);
            if (!Utilities.IsValid(m)) return;
            Networking.SetOwner(Networking.LocalPlayer, m.gameObject);
            if (player.isLocal) localMessage = m;
            m.message = message;
        }
        //Events
        public void OnNewMessage(MessageData m)
        {
            SendNewMessageEvent();
        }
        public void SendNewMessageEvent()
        {
            sortedMessages = SortMessages();
            Utils.Events.SendEvent("OnNewMessage", NewMessageEventListeners);
        }
        public void SendMessageUpdateEvent()
        {
            sortedMessages = SortMessages();
            Utils.Events.SendEvent("OnMessageUpdate", NewMessageEventListeners);
        }
        public void AddListener(UdonSharpBehaviour b)
        {
            Utils.ArrayUtils.Append(ref NewMessageEventListeners, b);
        }
        //Quicksort
        private void QuickSortMessages(MessageData[] arr,int start, int end)
        {
            if (!Utilities.IsValid(arr)) return;

            int i = 0;
            if (start < end)
            {
                i = PartitionMessages(arr,start, end);

                QuickSortMessages(arr, start, i - 1);
                QuickSortMessages(arr, i + 1, end);
            }
        }
        private int PartitionMessages(MessageData[] arr, int start, int end)
        {
            MessageData temp;
            MessageData p = arr[end];
            int i = start - 1;

            for (int j = start; j <= end - 1; j++)
            {
                if (CompareMessageTime(arr[j], p) == -1)
                {
                    i++;
                    temp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = temp;
                }
            }

            temp = arr[i + 1];
            arr[i + 1] = arr[end];
            arr[end] = temp;
            return i + 1;
        }

    }
}
