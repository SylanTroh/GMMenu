
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MessageData : UdonSharpBehaviour
    {
        const int OWNER_NULL = -1;
        const float TIME_NULL = -1.0f;
        const float TIME_UNTIL_STALE = 5*60.0f;

        public const int MESSAGE_NULL = -1;
        public const int MESSAGE_EMPTY = 0;
        public const int MESSAGE_URGENT = 1;
        public const int MESSAGE_ROLL = 2;
        public const int MESSAGE_QUESTION = 3;
        public const int MESSAGE_SILENT = 4;

        MessageSyncManager messageSyncManager;
        [UdonSynced] 
        int _ownerID = OWNER_NULL;

        [FieldChangeCallback(nameof(owner))] 
        VRCPlayerApi _owner;

        [UdonSynced, FieldChangeCallback(nameof(message))] 
        int _message = MESSAGE_NULL;

        public float timeReceived = TIME_NULL;
        public float timeRead = TIME_NULL;

        public bool isReadLocal = true;
        public bool isReadRemote = true;

        void Start()
        {
            messageSyncManager = Utils.Modules.MessageSyncManager(transform);
        }
        public VRCPlayerApi owner
        {
            set
            {
                Debug.Log("Set Message Ownership");
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                _owner = value;
                if (!Utilities.IsValid(value))
                {
                    _ownerID = OWNER_NULL;
                    message = MESSAGE_NULL;
                    timeReceived = TIME_NULL;

                    RequestSerialization();
                    return;
                }
                var id = VRCPlayerApi.GetPlayerId(value);
                if (id != _ownerID) message = MESSAGE_NULL;
                _ownerID = id;
                RequestSerialization();
            }
            get => _owner;
        }
        public int message
        {
            set
            {
                Debug.Log("Set Message: " + value);
                _message = value;
                RequestSerialization();
                isReadLocal = false;
                isReadRemote = false;
                timeReceived = TIME_NULL;
                if (value == MESSAGE_NULL)
                {
                    timeRead = TIME_NULL;
                    messageSyncManager.SendMessageUpdateEvent();
                    return;
                }
                timeReceived = Time.time;
                SendOnNewMessageEvent();
            }
            get => _message;
        }
        public string PrintMessage()
        {
            switch (message)
            {
                case MESSAGE_EMPTY:
                    return "";
                case MESSAGE_URGENT:
                    return owner.displayName + " requires urgent assistance.";
                case MESSAGE_ROLL:
                    return owner.displayName + " needs a roll.";
                case MESSAGE_QUESTION:
                    return owner.displayName + " has a question.";
                case MESSAGE_SILENT:
                    return "Join " + owner.displayName + " silently.";

                default:
                    return "Invalid Messge.";
            }
        }
        public bool IsRead()
        {
            return isReadLocal || isReadRemote;
        }
        public void SyncReadStatus(bool isRead)
        {
            isReadLocal = isRead;
            if (isReadRemote)
            {
                messageSyncManager.SendMessageUpdateEvent();
                return;
            }
            if (isRead)
            {
                SendOnReadRemoteEvent();
                SetReadTime();
                messageSyncManager.SendMessageUpdateEvent();
                SendOnMessageStaleEvent();
                return;
            }
            SendOnUndoReadRemoteEvent();
            ResetReadTime();
        }
        void SetReadTime()
        {
            if(timeRead == TIME_NULL) timeRead = Time.time;
        }
        void ResetReadTime()
        {
            timeRead = TIME_NULL;
        }
        //Events
        public override void OnDeserialization()
        {
            //Set _owner from synced _ownerID
            if (_ownerID == OWNER_NULL)
            {
                _owner = null;
                return;
            }
            _owner = VRCPlayerApi.GetPlayerById(_ownerID);
        }
        public void SendOnNewMessageEvent()
        {
            Debug.Log("[GMMenu]: OnNewMessage");
            messageSyncManager.OnNewMessage(this);
        }
        public void SendOnReadRemoteEvent()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(OnReadRemote));
        }
        public void OnReadRemote()
        {
            if (!isReadLocal) isReadRemote = true;
            messageSyncManager.SendMessageUpdateEvent();
            SetReadTime();
            SendOnMessageStaleEvent();
        }
        public void SendOnUndoReadRemoteEvent()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(OnUndoReadRemote));
        }
        public void OnUndoReadRemote()
        {
            isReadRemote = false;
            if (isReadLocal) SendOnReadRemoteEvent();
            messageSyncManager.SendMessageUpdateEvent();
        }
        public void SendOnMessageStaleEvent()
        {
            //Add one to avoid floating point precision messing with comparison
            SendCustomEventDelayedSeconds(nameof(OnMessageStale), TIME_UNTIL_STALE + 1.0f);
        }
        public void OnMessageStale()
        {
            if (message == MESSAGE_NULL) return;
            if (!IsRead()) return;
            var timePassed = Time.time - timeRead;
            if (timePassed <= TIME_UNTIL_STALE)
            {
                SendCustomEventDelayedSeconds(nameof(OnMessageStale), TIME_UNTIL_STALE - timePassed + 1.0f);
                return;
            }
            message = MESSAGE_NULL;
            messageSyncManager.SendMessageUpdateEvent();
        }
    }
}
