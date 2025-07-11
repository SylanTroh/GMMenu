#if SYLAN_AUDIOMANAGER_VERSION || (COMPILER_UDONSHARP && SYLAN_AUDIOMANAGER)
// See VoiceModeManager for an explanation for the condition above.
#define AUDIOMANAGER
#endif

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.AudioManager
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Radio : UdonSharpBehaviour
    {
        public RadioManager _RadioManager;

        public const int CHANNEL_NULL = -1;
        public const int CHANNEL_EMPTY = 0;

#if AUDIOMANAGER
        const int OWNER_NULL = -1;
        [UdonSynced]
        private int _ownerID = OWNER_NULL;

        [FieldChangeCallback(nameof(owner))]
        VRCPlayerApi _owner;

        [UdonSynced]
        [FieldChangeCallback(nameof(channel))]
        private int _channel = CHANNEL_NULL;

        public void ResetVariables()
        {
            channel = CHANNEL_NULL;
        }
        public VRCPlayerApi owner
        {
            set
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                _owner = value;
                channel = channel; // force update after changing owner
                if (!Utilities.IsValid(value))
                {
                    _ownerID = OWNER_NULL;
                    ResetVariables();
                    return;
                }
                var id = VRCPlayerApi.GetPlayerId(value);
                if (id != _ownerID) ResetVariables();
                _ownerID = id;
                if (_owner == Networking.LocalPlayer) _RadioManager.localRadio = this;
            }
            get => _owner;
        }
        public override void OnDeserialization()
        {
            //Set _owner from synced _ownerID
            if (_ownerID == OWNER_NULL)
            {
                _owner = null;
                return;
            }
            _owner = VRCPlayerApi.GetPlayerById(_ownerID);
            channel = channel; // force update after changing owner
            if (!Utilities.IsValid(_owner)) return;
            if (_owner == Networking.LocalPlayer) _RadioManager.localRadio = this;
        }

        public int channel
        {
            set
            {
                _channel = value;
                if (owner == null)
                    return;
                if (channel <= CHANNEL_EMPTY)
                {
                    owner.ExitRadioChannel(_RadioManager);
                    _RadioManager.UpdateRadioChannelSetting(owner);
                    Debug.Log("[RadioManager] " + owner.displayName + "-" + owner.playerId.ToString() + " Exiting Channel " + "-" + gameObject.GetInstanceID());
                }
                else
                {
                    owner.EnterRadioChannel(_RadioManager,channel);
                    _RadioManager.UpdateRadioChannelSetting(owner);
                    Debug.Log("[RadioManager] " + owner.displayName + "-" + owner.playerId.ToString() + " Entering Channel " + "-" + gameObject.GetInstanceID());
                }
            }
            get => _channel;
        }

        public void EnterChannel(int channelID)
        {
            if(Networking.LocalPlayer != owner) return;
            if(!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer,gameObject);
            channel = channelID;
            RequestSerialization();
        }

        public void ExitChannel()
        {
            if(Networking.LocalPlayer != owner) return;
            VRCPlayerApi triggeringPlayer = Networking.LocalPlayer;
            channel = CHANNEL_EMPTY;
            RequestSerialization();
        }
#endif
    }
}
