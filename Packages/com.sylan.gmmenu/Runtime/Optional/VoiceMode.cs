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
        bool isInitialized = false;

        [HideInInspector] public int priority = 2000;

#if AUDIOMANAGER
        const int OWNER_NULL = -1;
        [UdonSynced]
        private int _ownerID = OWNER_NULL;

        [FieldChangeCallback(nameof(owner))]
        VRCPlayerApi _owner;
#endif

        public const int SETTING_NULL = -1;
        public const int SETTING_EMPTY = 0;
        public const int SETTING_TALK = 1;
        public const int SETTING_WHISPER = 2;
        public const int SETTING_YELL = 3;
        public const int SETTING_BROADCAST = 4;

#if AUDIOMANAGER
        [UdonSynced, FieldChangeCallback(nameof(setting))]
        int _setting = SETTING_NULL;
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
            SettingWhisper[0] = voiceModeManager.whisperGain;
            SettingWhisper[1] = voiceModeManager.whisperNearRange;
            SettingWhisper[2] = voiceModeManager.whisperFarRange;
            SettingYell[0] = voiceModeManager.yellGain;
            SettingYell[2] = voiceModeManager.yellFarRange;
        }

        public void ResetVariables()
        {
            setting = SETTING_NULL;
        }
        public VRCPlayerApi owner
        {
            set
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                _owner = value;
                if (!Utilities.IsValid(value))
                {
                    _ownerID = OWNER_NULL;
                    ResetVariables();
                    return;
                }
                var id = VRCPlayerApi.GetPlayerId(value);
                if (id != _ownerID) ResetVariables();
                _ownerID = id;
                if (_owner == Networking.LocalPlayer) voiceModeManager.localVoiceMode = this;
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
            if (!Utilities.IsValid(_owner)) return;
            if (_owner == Networking.LocalPlayer) voiceModeManager.localVoiceMode = this;
            if (!isInitialized)
            {
                SetVoiceMode();
                isInitialized = true;
            }
        }
        public int setting
        {
            set
            {
                _setting = value;
                if (!Utilities.IsValid(owner)) return;
                if (owner == Networking.LocalPlayer) return;
                SetVoiceMode();
            }
            get => _setting;
        }
        public void SetVoiceMode()
        {
            if (setting == SETTING_WHISPER)
            {
                owner.RemoveAudioSetting(voiceModeManager.audioSettingManager, AUDIO_ZONE_SETTING_ID);
                owner.AddAudioSetting(voiceModeManager.audioSettingManager, AUDIO_ZONE_SETTING_ID, priority, SettingWhisper);
                voiceModeManager.audioSettingManager.UpdateAudioSettings(owner);
            }
            else if (setting == SETTING_YELL)
            {
                owner.RemoveAudioSetting(voiceModeManager.audioSettingManager, AUDIO_ZONE_SETTING_ID);
                owner.AddAudioSetting(voiceModeManager.audioSettingManager, AUDIO_ZONE_SETTING_ID, priority, SettingYell);
                voiceModeManager.audioSettingManager.UpdateAudioSettings(owner);
            }
            else if (setting == SETTING_BROADCAST)
            {
                owner.RemoveAudioSetting(voiceModeManager.audioSettingManager, AUDIO_ZONE_SETTING_ID);
                owner.AddAudioSetting(voiceModeManager.audioSettingManager, AUDIO_ZONE_SETTING_ID, int.MinValue, SettingBroadcast);
                voiceModeManager.audioSettingManager.UpdateAudioSettings(owner);
            }
            else
            {
                owner.RemoveAudioSetting(voiceModeManager.audioSettingManager, AUDIO_ZONE_SETTING_ID);
                voiceModeManager.audioSettingManager.UpdateAudioSettings(owner);
            }
        }
#endif
    }
}
