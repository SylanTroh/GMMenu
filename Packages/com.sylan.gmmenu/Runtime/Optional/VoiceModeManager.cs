
using Sylan.AudioManager;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VoiceModeManager : GMMenuPart
    {
        [SerializeField] GameObject VoiceModeButtons;

#if SYLAN_AUDIOMANAGER
        [HideInInspector,SerializeField] public AudioSettingManager audioSettingManager;
        public const string AudioSettingManagerPropertyName = nameof(audioSettingManager);
#else
        [HideInInspector, SerializeField] public UdonBehaviour audioSettingManager = null;
        public const string AudioSettingManagerPropertyName = null;
#endif

        private UdonSharpBehaviour[] VoiceModeChangedEventListeners = new UdonSharpBehaviour[0];

        [HideInInspector] public VoiceMode localVoiceMode;
        private VoiceMode[] audioSettings;
        private DataDictionary audioSettingsIndex = new DataDictionary();

        [Header("Lower number means higher priority", order = 0)]
        [Space(-10, order = 1)]
        [Header("Audiozones have priority 1000", order = 2)]
        public int priority = 2000;
        public float whisperGain = 15f;
        public float whisperFarRange = 5f;
        public float whisperNearRange = 0.5f;
        public float yellGain = 15f;
        public float yellFarRange = 40f;

        void Start()
        {
            audioSettings = GetComponentsInChildren<VoiceMode>();
            foreach (VoiceMode voiceMode in audioSettings)
            {
                voiceMode.priority = priority;
            }
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            SetAudioSettingOwnership(player);
        }
        private void SetAudioSettingOwnership(VRCPlayerApi player)
        {
            for (int i = 0; i < audioSettings.Length; i++)
            {
                VoiceMode voiceMode = audioSettings[i];
                if (voiceMode.owner != null) continue;

                voiceMode.owner = player;
                voiceMode.RequestSerialization();
                audioSettingsIndex.SetValue(player.playerId, i);
                return;
            }
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            RevokeAudioSettingOwnership(player);
        }
        private void RevokeAudioSettingOwnership(VRCPlayerApi player)
        {
            for (int i = 0; i < audioSettings.Length; i++)
            {
                VoiceMode voiceMode = audioSettings[i];
                if (voiceMode.owner != player) continue;

                voiceMode.owner = null;
                voiceMode.RequestSerialization();
                audioSettingsIndex.Remove(player.playerId);
            }
        }
        public VoiceMode GetLocalSetting()
        {
            return localVoiceMode;
        }
        public int GetLocalSettingValue()
        {
            if (!Utilities.IsValid(localVoiceMode)) return VoiceMode.SETTING_NULL;
            return localVoiceMode.setting;
        }
        public int GetPlayerSettingValue(VRCPlayerApi player)
        {
            if (audioSettingsIndex.TryGetValue(player.playerId, TokenType.Int, out DataToken value))
            {
                int i = value.Int;
                return audioSettings[i].setting;
            }
            return VoiceMode.SETTING_NULL;
        }
        public void SetTalk()
        {
            if (!Utilities.IsValid(localVoiceMode)) return;
            if (!Networking.IsOwner(localVoiceMode.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, localVoiceMode.gameObject);
            localVoiceMode.setting = VoiceMode.SETTING_TALK;
            localVoiceMode.RequestSerialization();
            SendVoiceModeChangedEvent();
        }
        public void SetWhisper()
        {
            if (!Utilities.IsValid(localVoiceMode)) return;
            if (!Networking.IsOwner(localVoiceMode.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, localVoiceMode.gameObject);
            localVoiceMode.setting = VoiceMode.SETTING_WHISPER;
            localVoiceMode.RequestSerialization();
            SendVoiceModeChangedEvent();
        }
        public void SetYell()
        {
            if (!Utilities.IsValid(localVoiceMode)) return;
            if (!Networking.IsOwner(localVoiceMode.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, localVoiceMode.gameObject);
            localVoiceMode.setting = VoiceMode.SETTING_YELL;
            localVoiceMode.RequestSerialization();
            SendVoiceModeChangedEvent();
        }
        public void SetBroadcast()
        {
            if (!Utilities.IsValid(localVoiceMode)) return;
            if (!Networking.IsOwner(localVoiceMode.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, localVoiceMode.gameObject);
            localVoiceMode.setting = VoiceMode.SETTING_BROADCAST;
            localVoiceMode.RequestSerialization();
            SendVoiceModeChangedEvent();
        }
        //Events
        public void SendVoiceModeChangedEvent()
        {
            Utils.Events.SendEvent("OnVoiceModeChanged", VoiceModeChangedEventListeners);
        }
        public void AddListener(UdonSharpBehaviour b)
        {
            Utils.ArrayUtils.Append(ref VoiceModeChangedEventListeners, b);
        }
    }
}
