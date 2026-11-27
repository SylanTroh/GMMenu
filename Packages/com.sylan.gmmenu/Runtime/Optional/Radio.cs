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
        [SerializeField] private RadioManager radioManager;

#if AUDIOMANAGER
        [UdonSynced]
        private int _channel = RadioManager.ChannelNull;

        private void Start()
        {
            if (radioManager.audioSettingManager == null) return;
            radioManager.RegisterRadio(Networking.GetOwner(gameObject),this);
        }

        public VRCPlayerApi Owner() => Networking.GetOwner(gameObject);

        public override void OnDeserialization()
        {
            UpdateChannel();
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (player != Networking.GetOwner(gameObject)) return;
            radioManager.UpdateRadioChannelSetting(player);
        }

        public int GetChannel() => _channel;

        public void SetChannel(int value)
        {
            _channel = value;
            UpdateChannel();
        }

        private void UpdateChannel()
        {
            if (radioManager.audioSettingManager == null) return;
            radioManager.UpdateRadioChannelSetting(Owner());
            string action = _channel <= RadioManager.ChannelEmpty ? " Exiting Channel " : " Entering Channel ";
            Debug.Log("[RadioManager] " + Owner().displayName + "-" + Owner().playerId + action + "-" + gameObject.GetInstanceID());
        }

        public void EnterChannel(int channelID)
        {
            if(!Networking.IsOwner(gameObject)) return;
            SetChannel(channelID);
            RequestSerialization();
        }

        public void ExitChannel()
        {
            if(!Networking.IsOwner(gameObject)) return;
            SetChannel(RadioManager.ChannelEmpty);
            RequestSerialization();
        }
#endif
    }
}
