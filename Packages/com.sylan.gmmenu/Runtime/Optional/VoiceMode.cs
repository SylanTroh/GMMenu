#if SYLAN_AUDIOMANAGER_VERSION || (COMPILER_UDONSHARP && SYLAN_AUDIOMANAGER)
// See VoiceModeManager for an explanation for the condition above.
#define AUDIOMANAGER
#endif

#if AUDIOMANAGER
using Sylan.AudioManager;
#endif
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VoiceMode : GMMenuPart
    {
        [HideInInspector] public int priority = 2000;

        public const int SETTING_NULL = -1;
        public const int SETTING_EMPTY = 0;
        public const int SETTING_TALK = 1;
        public const int SETTING_WHISPER = 2;
        public const int SETTING_YELL = 3;
        public const int SETTING_BROADCAST = 4;

#if AUDIOMANAGER
        [UdonSynced] private int _voiceModeSetting = SETTING_NULL;
#endif
        
        public VoiceModeManager voiceModeManager;
        public const string AUDIO_ZONE_SETTING_ID = "GMMENUAUDIOSETTING";

#if AUDIOMANAGER
        DataList SettingWhisper = new DataList()
        {
            (DataToken)15.0f, //Voice Gain
            (DataToken)0.0f, //Voice Range Near
            (DataToken)5.0f, //Voice Range Far
            (DataToken)AudioSettingManager.DEFAULT_VOICE_VOLUMETRIC_RADIUS,
            (DataToken)false
        };
        DataList SettingYell = new DataList()
        {
            (DataToken)15.0f, //Voice Gain
            (DataToken)0.0f, //Voice Range Near
            (DataToken)30.0f, //Voice Range Far
            (DataToken)AudioSettingManager.DEFAULT_VOICE_VOLUMETRIC_RADIUS,
            (DataToken)false //Voice Lowpass
        };
        DataList SettingBroadcast = new DataList()
        {
            (DataToken)0.0f, //Voice Gain
            (DataToken)999999.0f, //Voice Range Near
            (DataToken)1000000.0f, //Voice Range Far
            (DataToken)AudioSettingManager.DEFAULT_VOICE_VOLUMETRIC_RADIUS,
            (DataToken)false //Voice Lowpass
        };

        private void Start()
        {
            if (voiceModeManager.audioSettingManager == null) return;
            voiceModeManager.RegisterVoiceMode(Networking.GetOwner(gameObject),this);
            priority = voiceModeManager.priority;
            
            SettingWhisper[0] = voiceModeManager.whisperGain;
            SettingWhisper[1] = voiceModeManager.whisperNearRange;
            SettingWhisper[2] = voiceModeManager.whisperFarRange;
            SettingYell[0] = voiceModeManager.yellGain;
            SettingYell[2] = voiceModeManager.yellFarRange;
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if(player != Networking.GetOwner(gameObject)) return;
            SetVoiceMode();
        }
        
        public override void OnDeserialization()
        {
            Debug.Log("[VoiceMode] OnDeserialization");
            SetVoiceMode();
        }

        public int GetVoiceMode() => _voiceModeSetting;

        public void SetVoiceMode(int value)
        {
            _voiceModeSetting = value;
            SetVoiceMode();
        }

        public void SetVoiceMode()
        {
            if (voiceModeManager.audioSettingManager == null) return;
            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            owner.RemoveAudioSetting(voiceModeManager.audioSettingManager, AUDIO_ZONE_SETTING_ID);

            switch (_voiceModeSetting)
            {
                case SETTING_WHISPER:
                    owner.AddAudioSetting(voiceModeManager.audioSettingManager, AUDIO_ZONE_SETTING_ID, priority, SettingWhisper);
                    break;
                case SETTING_YELL:
                    owner.AddAudioSetting(voiceModeManager.audioSettingManager, AUDIO_ZONE_SETTING_ID, priority, SettingYell);
                    break;
                case SETTING_BROADCAST:
                    owner.AddAudioSetting(voiceModeManager.audioSettingManager, AUDIO_ZONE_SETTING_ID, int.MinValue, SettingBroadcast);
                    break;
            }

            voiceModeManager.audioSettingManager.UpdateAudioSettings(owner);
        }
#endif
    }
}
