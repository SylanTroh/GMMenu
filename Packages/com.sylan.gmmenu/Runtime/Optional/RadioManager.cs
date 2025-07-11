#if SYLAN_AUDIOMANAGER_VERSION || (COMPILER_UDONSHARP && SYLAN_AUDIOMANAGER)
// See VoiceModeManager for an explanation for the condition above.
#define AUDIOMANAGER
#endif

using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Sylan.AudioManager
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RadioManager : UdonSharpBehaviour
    {
        public GameObject RadioButton;

        public const int RADIO_CHANNEL_PRIORITY = 900;
        public const string RADIO_CHANNEL_SETTING_ID = "RADIOVOICESETTINGS";

        private Radio[] radios;
        [HideInInspector] public Radio localRadio;

#if AUDIOMANAGER
        [HideInInspector, SerializeField] public AudioSettingManager audioSettingManager;
        public const string AudioSettingManagerPropertyName = nameof(audioSettingManager);

        [Header("Set AudioSetting when in the same channel")]
        [SerializeField] private float voiceGain = 0.0f;
        [SerializeField] private float voiceRangeNear = 999999.0f;
        [SerializeField] private float voiceRangeFar = 1000000.0f;
        [SerializeField] private float volumetricRadius = AudioSettingManager.DEFAULT_VOICE_VOLUMETRIC_RADIUS;
        [SerializeField] private bool voiceLowpass = false;

        //Key:playerID -> int channelNumber
        DataDictionary _RadioChannelDict = new DataDictionary();
        DataList RadioChannelAudioSettings = new DataList()
        {
            0.0f, //Voice Gain
            999999.0f, //Voice Range Near
            1000000.0f, //Voice Range Far
            AudioSettingManager.DEFAULT_VOICE_VOLUMETRIC_RADIUS,
            false
        };
#endif

        private void DestroySelf()
        {
            Destroy(RadioButton);
            Destroy(gameObject);
        }

#if !AUDIOMANAGER
        private void Start()
        {
            DestroySelf();
        }
#else
        private void Start()
        {
            if (audioSettingManager == null)
            {
                DestroySelf();
                radios = new Radio[0]; // Prevent potential errors since Destroy is not instant.
                return;
            }
            RadioChannelAudioSettings[AudioSettingManager.VOICE_GAIN_INDEX] = voiceGain;
            RadioChannelAudioSettings[AudioSettingManager.RANGE_NEAR_INDEX] = voiceRangeNear;
            RadioChannelAudioSettings[AudioSettingManager.RANGE_FAR_INDEX] = voiceRangeFar;
            RadioChannelAudioSettings[AudioSettingManager.VOLUMETRIC_RADIUS_INDEX] = volumetricRadius;
            RadioChannelAudioSettings[AudioSettingManager.VOICE_LOWPASS_INDEX] = voiceLowpass;

            radios = GetComponentsInChildren<Radio>();
        }

        //
        // Manage AudioZoneDict By Player
        //
        public int GetPlayerRadioChannel(VRCPlayerApi player)
        {
            DataDictionary dict = _RadioChannelDict;

            if (!Utilities.IsValid(player)) return -1;
            if (!player.IsValid()) return -1;

            if (!dict.TryGetValue((DataToken)player.playerId, TokenType.Int, out DataToken value))
            {
                Debug.LogError("[RadioManager] Failed to get Channel for " + player.displayName + "-" + player.playerId.ToString());
                return -1;
            }
            return value.Int;
        }
        public void InitPlayerRadioChannel(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return;
            if (!player.IsValid()) return;

            if (_RadioChannelDict.TryGetValue((DataToken)player.playerId, TokenType.Int, out DataToken value))
            {
                Debug.Log("[RadioManager] RadioChannel already initialized for " + player.displayName + "-" + player.playerId.ToString());
                return;
            }
            _RadioChannelDict.SetValue(key: (DataToken)player.playerId, value: 0);
            Debug.Log("[RadioManager] Initialize RadioChannel for " + player.displayName + "-" + player.playerId.ToString());
        }
        public int RemovePlayerRadioChannel(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return -1;
            if (!player.IsValid()) return -1;

            if (!_RadioChannelDict.Remove(key: (DataToken)player.playerId, out DataToken value))
            {
                Debug.LogError("[RadioManager] Failed to remove RadioChannel for " + player.displayName + "-" + player.playerId.ToString());
            }
            Debug.Log("[RadioManager] Removed RadioChannel for " + player.displayName + "-" + player.playerId.ToString());

            return value.Int;
        }
        public override void OnPlayerJoined(VRCPlayerApi joiningPlayer)
        {
            if (joiningPlayer == Networking.LocalPlayer)
            {
                VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
                VRCPlayerApi.GetPlayers(players);
                foreach (var player in players)
                {
                    InitPlayerRadioChannel(player);
                }
            }
            else
            {
                InitPlayerRadioChannel(joiningPlayer);
            }
            if (!Networking.IsOwner(gameObject)) return;
            SetRadioOwnership(joiningPlayer);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            RemovePlayerRadioChannel(player);
            if (!Networking.IsOwner(gameObject)) return;
            RevokeRadioOwnership(player);
        }
        private void SetRadioOwnership(VRCPlayerApi player)
        {
            foreach (Radio radio in radios)
            {
                if (radio.owner != null) continue;

                radio.owner = player;
                radio.RequestSerialization();
                return;
            }
        }
        private void RevokeRadioOwnership(VRCPlayerApi player)
        {
            foreach (Radio radio in radios)
            {
                if (radio.owner != player) continue;

                radio.owner = null;
                radio.RequestSerialization();
            }
        }
        public int GetLocalRadioChannel()
        {
            if (!Utilities.IsValid(localRadio)) return Radio.CHANNEL_NULL;
            return localRadio.channel;
        }
        //
        //Manage AudioZoneDict[player]
        //
        public void EnterRadioChannel(VRCPlayerApi player, int channelID)
        {
            _RadioChannelDict.SetValue(player.playerId, channelID);
        }
        public void ExitRadioChannel(VRCPlayerApi player)
        {
            _RadioChannelDict.SetValue(player.playerId, 0);
        }
        public bool InRadioChannel(VRCPlayerApi player, int channelID)
        {
            int value = GetPlayerRadioChannel(player);
            if (value == -1) return false;
            return channelID == value;
        }
        public bool SharesRadioChannelWith(VRCPlayerApi player1, VRCPlayerApi player2)
        {
            int value1 = GetPlayerRadioChannel(player1);
            int value2 = GetPlayerRadioChannel(player2);

            if (value1 <= 0 || value2 <= 0) return false;

            return value1 == value2;
        }
        //
        //Update Audio Settings
        //
        public void UpdateRadioChannelSetting(VRCPlayerApi triggeringPlayer)
        {
            if (triggeringPlayer == null) return;
            if (!triggeringPlayer.IsValid()) return;

            if (triggeringPlayer != Networking.LocalPlayer)
            {
                //If someone else caused the update, update triggering player
                ApplyRadioChannelSetting(triggeringPlayer);
                audioSettingManager.ApplyAudioSetting(triggeringPlayer);
            }
            else
            {
                //If the local player caused the update, update all players
                VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
                VRCPlayerApi.GetPlayers(players);
                foreach (VRCPlayerApi player in players)
                {
                    ApplyRadioChannelSetting(player);
                    audioSettingManager.ApplyAudioSetting(player);
                }
            }
        }
        public void ApplyRadioChannelSetting(VRCPlayerApi player)
        {
            if (!player.IsValid()) return;
            if (player == Networking.LocalPlayer) return;
            if (Networking.LocalPlayer.SharesRadioChannelWith(player, this))
            {
                //Debug.Log("[AudioManager] Shares AudioZone with" + player.displayName + ".");
                audioSettingManager.AddAudioSetting(player, RADIO_CHANNEL_SETTING_ID, RADIO_CHANNEL_PRIORITY, RadioChannelAudioSettings);
            }
            else
            {
                //Debug.Log("[AudioManager] Does not share AudioZone with " + player.displayName + ".");
                audioSettingManager.RemoveAudioSetting(player, RADIO_CHANNEL_SETTING_ID);
            }
        }

        public void SetChannel1()
        {
            if (!Utilities.IsValid(localRadio)) return;
            if (localRadio.channel == 1) localRadio.ExitChannel();
            else localRadio.EnterChannel(1);
        }
#endif
    }

#if AUDIOMANAGER
    public static class RadioManagerExtensions
    {
        //
        //Extensions for VRCPlayerAPI
        //
        public static void EnterRadioChannel(this VRCPlayerApi player, RadioManager radioManager, int channelID)
        {
            radioManager.EnterRadioChannel(player, channelID);
        }
        public static void ExitRadioChannel(this VRCPlayerApi player, RadioManager radioManager)
        {
            radioManager.ExitRadioChannel(player);
        }
        public static bool InRadioChannel(this VRCPlayerApi player, RadioManager radioManager, int channelID)
        {
            return radioManager.InRadioChannel(player, channelID);
        }
        public static bool SharesRadioChannelWith(this VRCPlayerApi player1, VRCPlayerApi player2, RadioManager radioManager)
        {
            return radioManager.SharesRadioChannelWith(player1, player2);
        }
        //
        //
        //
    }
#endif
}
