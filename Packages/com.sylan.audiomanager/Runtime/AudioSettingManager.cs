using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Sylan.AudioManager
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AudioSettingManager : UdonSharpBehaviour
    {
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

        string DefaultAudioSettingID = string.Empty;
        int DefaultAudioSettingPriority = int.MaxValue;

        [Header("Set default AudioSetting")]
        [SerializeField] private float voiceGain = DEFAULT_VOICE_GAIN;
        [SerializeField] private float voiceRangeNear = DEFAULT_VOICE_RANGE_NEAR;
        [SerializeField] private float voiceRangeFar = DEFAULT_VOICE_RANGE_FAR;
        [SerializeField] private float volumetricRadius = DEFAULT_VOICE_VOLUMETRIC_RADIUS;
        [SerializeField] private bool voiceLowpass = DEFAULT_VOICE_LOWPASS;

        public AudioZoneManager AudioZoneManager { get => _AudioZoneManager; private set { _AudioZoneManager = value; } }
        [HideInInspector, SerializeField] private AudioZoneManager _AudioZoneManager;
        public const string AudioZoneManagerPropertyName = nameof(_AudioZoneManager);

        //Key:playerID -> DataList [ settingID[], settingPriority[], audioSettings[] ]
        private DataDictionary _AudioSettingDict = new DataDictionary();

        //
        // Set Player Voice
        //
        private static void SetPlayerVoice(VRCPlayerApi player, float voiceGain, float voiceNear, float voiceFar, float voiceVolumetricRadius, bool voiceLowpass)
        {
            if (!Utilities.IsValid(player)) return;
            if (!player.IsValid()) return;

            player.SetVoiceGain(voiceGain);
            player.SetVoiceDistanceNear(voiceNear);
            player.SetVoiceDistanceFar(voiceFar);
            player.SetVoiceVolumetricRadius(voiceVolumetricRadius);
            player.SetVoiceLowpass(voiceLowpass);
        }
        private static void SetPlayerVoice(VRCPlayerApi player, DataList audioSetting)
        {
            if (audioSetting == null) return;
            SetPlayerVoice(
                player,
                audioSetting[VOICE_GAIN_INDEX].Float,
                audioSetting[RANGE_NEAR_INDEX].Float,
                audioSetting[RANGE_FAR_INDEX].Float,
                audioSetting[VOLUMETRIC_RADIUS_INDEX].Float,
                audioSetting[VOICE_LOWPASS_INDEX].Boolean
                );
        }

        //
        // Manage _AudioSettingDict By player
        //
        private DataList GetPlayerAudioSettings(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return null;
            if (!player.IsValid()) return null;

            if (!_AudioSettingDict.TryGetValue((DataToken)player.playerId, TokenType.DataList, out DataToken value))
            {
                Debug.LogError("[AudioManager] Failed to get AudioSettings for " + player.displayName + "-" + player.playerId.ToString());
                return null;
            }
            return value.DataList;
        }
        private void InitPlayerAudioSettingDict(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return;
            if (!player.IsValid()) return;

            if (_AudioSettingDict.TryGetValue((DataToken)player.playerId, TokenType.DataDictionary, out DataToken value))
            {
                Debug.Log("[AudioManager] AudioSettigs already initialized for " + player.displayName + "-" + player.playerId.ToString());
                return;
            }

            DataList DefaultAudioSettings = new DataList();
            DefaultAudioSettings.Add((DataToken)voiceGain);
            DefaultAudioSettings.Add((DataToken)voiceRangeNear);
            DefaultAudioSettings.Add((DataToken)voiceRangeFar);
            DefaultAudioSettings.Add((DataToken)volumetricRadius);
            DefaultAudioSettings.Add((DataToken)voiceLowpass);
            DataList DefaultDictEntry = new DataList();
            DefaultDictEntry.Add((DataToken) new DataList());
            DefaultDictEntry.Add((DataToken) new DataList());
            DefaultDictEntry.Add((DataToken) new DataList());
            DefaultDictEntry[SETTING_ID_INDEX].DataList.Add((DataToken)DefaultAudioSettingID);
            DefaultDictEntry[SETTING_PRIORITY_INDEX].DataList.Add((DataToken)DefaultAudioSettingPriority);
            DefaultDictEntry[SETTING_INDEX].DataList.Add((DataToken)DefaultAudioSettings);

            _AudioSettingDict.SetValue(key: (DataToken)player.playerId, value: (DataToken)DefaultDictEntry);
            Debug.Log("[AudioManager] Initialize AudioSettings for " + player.displayName + "-" + player.playerId.ToString());
        }
        private DataList RemovePlayerAudioSettingDict(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return null;
            if (!player.IsValid()) return null;

            if (!_AudioSettingDict.Remove(key: (DataToken)player.playerId, out DataToken value))
            {
                Debug.LogError("[AudioManager] Failed to remove AudioSettingDict for " + player.displayName + "-" + player.playerId.ToString());
            }
            Debug.Log("[AudioManager] Removed AudioSettingDict for " + player.displayName + "-" + player.playerId.ToString());
            return value.DataList;
        }
        public override void OnPlayerJoined(VRCPlayerApi joiningPlayer)
        {
            if (joiningPlayer == Networking.LocalPlayer)
            {
                VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
                VRCPlayerApi.GetPlayers(players);
                foreach (var player in players)
                {
                    InitPlayerAudioSettingDict(player);
                }
            }
            else
            {
                InitPlayerAudioSettingDict(joiningPlayer);
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            RemovePlayerAudioSettingDict(player);
        }
        //
        //Manage _AudioSettingDict[player] by settingID
        //
        private bool ValidateAudioSetting(DataList audioSetting)
        {
            bool isValid =
                (audioSetting != null) &&
                (audioSetting.Count == 5) &&
                (audioSetting.TryGetValue(VOICE_GAIN_INDEX, TokenType.Float, out DataToken discard)) &&
                (audioSetting.TryGetValue(RANGE_NEAR_INDEX, TokenType.Float, out discard)) &&
                (audioSetting.TryGetValue(RANGE_FAR_INDEX, TokenType.Float, out discard)) &&
                (audioSetting.TryGetValue(VOLUMETRIC_RADIUS_INDEX, TokenType.Float, out discard)) &&
                (audioSetting.TryGetValue(VOICE_LOWPASS_INDEX, TokenType.Boolean, out discard));

            if (!isValid)
            {
                Debug.LogError("[AudioManager] Invalid Audio Setting");
                return false;
            }
            return true;
        }
        public void AddAudioSetting(VRCPlayerApi player, string settingID, int priority, DataList audioSetting)
        {
            if (!Utilities.IsValid(player)) return;
            if (!player.IsValid()) return;
            if (player == Networking.LocalPlayer) return;

            if (!ValidateAudioSetting(audioSetting)) return;
            _AddAudioSetting(player, settingID, priority, audioSetting);
        }
        private void _AddAudioSetting(VRCPlayerApi player, string settingID, int priority, DataList audioSetting)
        {
            if (audioSetting == null) return;

            DataList list = GetPlayerAudioSettings(player);
            if (!Utilities.IsValid(list)) return;

            if (!list.TryGetValue(SETTING_ID_INDEX, TokenType.DataList, out DataToken token)) return;
            DataList settingIDList = token.DataList;
            if (!list.TryGetValue(SETTING_PRIORITY_INDEX, TokenType.DataList, out token)) return;
            DataList priorityList = token.DataList;
            if (!list.TryGetValue(SETTING_INDEX, TokenType.DataList, out token)) return;
            DataList SettingList = token.DataList;

            if (settingIDList.Contains((DataToken)settingID)) return;

            int index = -1;

            for (int i = 0; i < priorityList.Count; i++)
            {
                if (!priorityList.TryGetValue(i, TokenType.Int, out DataToken p)) continue;
                if (priority < p.Int)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                list[SETTING_ID_INDEX].DataList.Add((DataToken)settingID);
                list[SETTING_PRIORITY_INDEX].DataList.Add((DataToken)priority);
                list[SETTING_INDEX].DataList.Add((DataToken)audioSetting);
            }
            else
            {
                list[SETTING_ID_INDEX].DataList.Insert(index, (DataToken)settingID);
                list[SETTING_PRIORITY_INDEX].DataList.Insert(index, (DataToken)priority);
                list[SETTING_INDEX].DataList.Insert(index, (DataToken)audioSetting);
            }
        }
        public bool RemoveAudioSetting(VRCPlayerApi player, string settingID)
        {
            if (!Utilities.IsValid(player)) return false;
            if (!player.IsValid()) return false;
            if (player == Networking.LocalPlayer) return false;

            DataList list = GetPlayerAudioSettings(player);
            if (!Utilities.IsValid(list)) return false;

            if (!list.TryGetValue(SETTING_ID_INDEX, TokenType.DataList, out DataToken token)) return false;
            DataList settingIDList = token.DataList;
            if (!list.TryGetValue(SETTING_PRIORITY_INDEX, TokenType.DataList, out token)) return false;
            DataList priorityList = token.DataList;
            if (!list.TryGetValue(SETTING_INDEX, TokenType.DataList, out token)) return false;
            DataList SettingList = token.DataList;

            int index = settingIDList.IndexOf((DataToken)settingID);
            if (index == -1) return false;
            else
            {
                list[SETTING_ID_INDEX].DataList.RemoveAt(index);
                list[SETTING_PRIORITY_INDEX].DataList.RemoveAt(index);
                list[SETTING_INDEX].DataList.RemoveAt(index);
                return true;
            }
        }
        public void ClearAudioSettings(VRCPlayerApi player)
        {
            InitPlayerAudioSettingDict(player);
        }
        //
        //Update Audio Settings
        //
        public void UpdateAudioSettings(VRCPlayerApi triggeringPlayer)
        {
            if (!Utilities.IsValid(triggeringPlayer)) return;
            if (!triggeringPlayer.IsValid()) return;
            if (triggeringPlayer == Networking.LocalPlayer) return;

            //If someone else caused the update, update triggering player
            ApplyAudioSetting(triggeringPlayer);
        }
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
