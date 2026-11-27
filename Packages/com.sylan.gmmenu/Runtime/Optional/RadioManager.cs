#if SYLAN_AUDIOMANAGER_VERSION || (COMPILER_UDONSHARP && SYLAN_AUDIOMANAGER)
// See VoiceModeManager for an explanation for the condition above.
#define AUDIOMANAGER
#endif

using Sylan.GMMenu;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using UnityEngine.Serialization;

namespace Sylan.AudioManager
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RadioManager : UdonSharpBehaviour
    {
        [FormerlySerializedAs("RadioButton")]
        [SerializeField] private GameObject radioButton;
        [SerializeField] private GameObject requestRadioButton;

        private const int RADIO_CHANNEL_PRIORITY = 900;
        private const string RADIO_CHANNEL_SETTING_ID = "RADIOVOICESETTINGS";

        public const int ChannelNull = -1;
        public const int ChannelEmpty = 0;

        private Radio _localRadio;

        private void DeactivateSelf()
        {
            Destroy(radioButton);
            Destroy(requestRadioButton);
            gameObject.SetActive(false);
        }

#if !AUDIOMANAGER
        private void Start()
        {
            DeactivateSelf();
        }
#else
        [HideInInspector, SerializeField] public AudioSettingManager audioSettingManager;
        public const string AudioSettingManagerPropertyName = nameof(audioSettingManager);

        [Header("Set AudioSetting when in the same channel")]
        [SerializeField] private float voiceGain = 0.0f;
        [SerializeField] private float voiceRangeNear = 999999.0f;
        [SerializeField] private float voiceRangeFar = 1000000.0f;
        [SerializeField] private float volumetricRadius = AudioSettingManager.DEFAULT_VOICE_VOLUMETRIC_RADIUS;
        [SerializeField] private bool voiceLowpass = false;

        //Key:playerID -> ref Radio Player Object
        private readonly DataDictionary _radioObjectCache = new DataDictionary();

        private readonly DataList _radioChannelAudioSettings = new DataList()
        {
            0.0f, //Voice Gain
            999999.0f, //Voice Range Near
            1000000.0f, //Voice Range Far
            AudioSettingManager.DEFAULT_VOICE_VOLUMETRIC_RADIUS,
            false
        };
        private void Start()
        {
            if (audioSettingManager == null)
            {
                DeactivateSelf();
                return;
            }
            _radioChannelAudioSettings[AudioSettingManager.VOICE_GAIN_INDEX] = voiceGain;
            _radioChannelAudioSettings[AudioSettingManager.RANGE_NEAR_INDEX] = voiceRangeNear;
            _radioChannelAudioSettings[AudioSettingManager.RANGE_FAR_INDEX] = voiceRangeFar;
            _radioChannelAudioSettings[AudioSettingManager.VOLUMETRIC_RADIUS_INDEX] = volumetricRadius;
            _radioChannelAudioSettings[AudioSettingManager.VOICE_LOWPASS_INDEX] = voiceLowpass;
        }

        public void RegisterRadio(VRCPlayerApi player, Radio radio)
        {
            if (Networking.IsOwner(Networking.LocalPlayer, radio.gameObject)) _localRadio = radio;
            GMMenu.Utils.PlayerObjectCache.RegisterPlayerObject(_radioObjectCache, player, radio);
        }

        public int GetLocalRadioChannel()
        {
            if (!Utilities.IsValid(_localRadio)) return ChannelNull;
            return _localRadio.GetChannel();
        }

        public int GetPlayerRadioChannel(VRCPlayerApi player)
        {
            if (GMMenu.Utils.PlayerObjectCache.GetPlayerObject(_radioObjectCache, player, out Radio radio))
            {
                return radio.GetChannel();
            }
            return ChannelNull;
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            GMMenu.Utils.PlayerObjectCache.DeregisterPlayerObject(_radioObjectCache,player);
        }

        //
        // Manage Channel
        //
        public void EnterRadioChannel(int channelID)
        {
            if (!Utilities.IsValid(_localRadio)) return;
            _localRadio.EnterChannel(channelID);
        }
        public void ExitRadioChannel()
        {
            if (!Utilities.IsValid(_localRadio)) return;
            _localRadio.ExitChannel();
        }
        public bool InRadioChannel(VRCPlayerApi player, int channelID)
        {
            int value = GetPlayerRadioChannel(player);
            if (value == ChannelNull) return false;
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
                audioSettingManager.AddAudioSetting(player, RADIO_CHANNEL_SETTING_ID, RADIO_CHANNEL_PRIORITY, _radioChannelAudioSettings);
            }
            else
            {
                //Debug.Log("[AudioManager] Does not share AudioZone with " + player.displayName + ".");
                audioSettingManager.RemoveAudioSetting(player, RADIO_CHANNEL_SETTING_ID);
            }
        }

        public void SetChannel1()
        {
            if (GetLocalRadioChannel() == 1) ExitRadioChannel();
            else EnterRadioChannel(1);
        }

#endif
    }

#if AUDIOMANAGER
    public static class RadioManagerExtensions
    {
        //
        //Extensions for VRCPlayerAPI
        //
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
