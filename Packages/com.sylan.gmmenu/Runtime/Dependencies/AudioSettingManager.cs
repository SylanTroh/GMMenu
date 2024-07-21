using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
#if SYLAN_AUDIOMANAGER
using Sylan.AudioManager
#endif

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AudioSettingManager : UdonSharpBehaviour
    {

#if SYLAN_AUDIOMANAGER
        [HideInInspector,SerializeField] public AudioSettingManager audioSettingManager;
        public const string AudioSettingManagerPropertyName = nameof(audioSettingManager);
#else
        [HideInInspector, SerializeField] public AudioSettingManager audioSettingManager = null;
        public const string AudioSettingManagerPropertyName = null;
#endif

        public const int DEFAULT_PRIORITY = 0;
        public const float DEFAULT_VOICE_GAIN = 15.0f;
        public const float DEFAULT_VOICE_RANGE_NEAR = 0.0f;
        public const float DEFAULT_VOICE_RANGE_FAR = 15.0f;
        public const float DEFAULT_VOICE_VOLUMETRIC_RADIUS = 0.0f;
        public const bool DEFAULT_VOICE_LOWPASS = true;

        public const int VOICE_GAIN_INDEX = 0;
        public const int RANGE_NEAR_INDEX = 1;
        public const int RANGE_FAR_INDEX = 2;
        public const int VOLUMETRIC_RADIUS_INDEX = 3;
        public const int VOICE_LOWPASS_INDEX = 4;

        public const int SETTING_ID_INDEX = 0;
        public const int SETTING_PRIORITY_INDEX = 1;
        public const int SETTING_INDEX = 2;

        public void AddAudioSetting(VRCPlayerApi player, string settingID, int priority, DataList audioSetting) { }

        public bool RemoveAudioSetting(VRCPlayerApi player, string settingID) { }

        public void ClearAudioSettings(VRCPlayerApi player);

        //
        //Update Audio Settings
        //
        public abstract void UpdateAudioSettings(VRCPlayerApi triggeringPlayer)

        public void ApplyAudioSetting(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return;
            if (!player.IsValid()) return;
            if (player == Networking.LocalPlayer) return;

            DataList list = GetPlayerAudioSettings(player);
            if (!Utilities.IsValid(list)) return;

            //VRCJson.TrySerializeToJson(list, JsonExportType.Minify, out DataToken result1);
            //Debug.Log(result1.ToString());

            if (!list.TryGetValue(SETTING_ID_INDEX, TokenType.DataList, out DataToken token)) return;
            DataList settingIDList = token.DataList;
            if (!list.TryGetValue(SETTING_PRIORITY_INDEX, TokenType.DataList, out token)) return;
            DataList priorityList = token.DataList;
            if (!list.TryGetValue(SETTING_INDEX, TokenType.DataList, out token)) return;
            DataList SettingList = token.DataList;

            //Get Highest Priority Setting
            if (!list[SETTING_INDEX].DataList.TryGetValue(0, TokenType.DataList, out token)) return;

            DataList audioSetting = token.DataList;
            if (!ValidateAudioSetting(audioSetting)) return;

            SetPlayerVoice(player, audioSetting);

            string debugString = "[AudioManager] Setting " + player.displayName + "-" + player.playerId.ToString() + " Audio:";
            debugString += " SettingID:" + list[SETTING_ID_INDEX].DataList[0].String;
            debugString += ", VoiceGain:" + audioSetting[VOICE_GAIN_INDEX].Float.ToString();
            debugString += ", VoiceNear:" + audioSetting[RANGE_NEAR_INDEX].Float.ToString();
            debugString += ", VoiceFar:" + audioSetting[RANGE_FAR_INDEX].Float.ToString();
            debugString += ", VolumetricRadius:" + audioSetting[VOLUMETRIC_RADIUS_INDEX].Float.ToString();
            debugString += ", Lowpass:" + audioSetting[VOICE_LOWPASS_INDEX].Boolean.ToString();

            Debug.Log(debugString);
        }
    }
    public static class AudioSettingManagerExtensions
    {
        //
        //Extensions for VRCPlayerAPI
        //
        public static void AddAudioSetting(this VRCPlayerApi player, AudioSettingManager settingManager, string settingID, int priority, DataList audioSetting)
        {
            settingManager.AddAudioSetting(player, settingID, priority, audioSetting);
        }
        public static bool RemoveAudioSetting(this VRCPlayerApi player, AudioSettingManager settingManager, string settingID)
        {
            return settingManager.RemoveAudioSetting(player, settingID);
        }
        public static void ClearAudioSettings(this VRCPlayerApi player, AudioSettingManager settingManager)
        {
            settingManager.ClearAudioSettings(player);
        }
        //
        //
        //
    }
}
