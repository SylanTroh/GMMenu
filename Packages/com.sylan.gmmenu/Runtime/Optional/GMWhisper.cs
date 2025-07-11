#if SYLAN_AUDIOMANAGER_VERSION || (COMPILER_UDONSHARP && SYLAN_AUDIOMANAGER)
// See VoiceModeManager for an explanation for the condition above.
#define AUDIOMANAGER
#endif

#if AUDIOMANAGER
using Sylan.AudioManager;
#endif
using UdonSharp;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GMWhisper : GMMenuPart
    {
        public GMWhisperManager gmWhisperManager;

        public const string SETTING_ID = "GMWhisper";

#if AUDIOMANAGER
        bool isInitialized = false;

        const int OWNER_NULL = -1;
        [UdonSynced]
        int _ownerID = OWNER_NULL;

        [FieldChangeCallback(nameof(owner))]
        VRCPlayerApi _owner;

        [UdonSynced, FieldChangeCallback(nameof(playerIDs))]
        int[] _playerIDs = new int[0];

        DataList SettingBroadcast = new DataList()
        {
            (DataToken)0.0f, //Voice Gain
            (DataToken)999999.0f, //Voice Range Near
            (DataToken)1000000.0f, //Voice Range Far
            (DataToken)AudioSettingManager.DEFAULT_VOICE_VOLUMETRIC_RADIUS,
            (DataToken)false //Voice Lowpass
        };
        DataList SettingOff = new DataList()
        {
            (DataToken)0.0f, //Voice Gain
            (DataToken)0f, //Voice Range Near
            (DataToken)0f, //Voice Range Far
            (DataToken)AudioSettingManager.DEFAULT_VOICE_VOLUMETRIC_RADIUS,
            (DataToken)false //Voice Lowpass
        };
        public void ResetVariables()
        {
            playerIDs = new int[0];
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
                if (_owner == Networking.LocalPlayer) gmWhisperManager.localGMWhisper = this;
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
            if (_owner == Networking.LocalPlayer) gmWhisperManager.localGMWhisper = this;
            if (!isInitialized)
            {
                SetGMWhisper();
                isInitialized = true;
            }
        }
        public int[] playerIDs
        {
            set
            {
                _playerIDs = value;
                if (!Utilities.IsValid(owner)) return;
                if (owner == Networking.LocalPlayer) return;
                SetGMWhisper();
            }
            get => _playerIDs;
        }
        public void SetGMWhisper()
        {
            int localID = Networking.LocalPlayer.playerId;
            foreach (var id in _playerIDs)
            {
                if (id != localID) continue;
                owner.RemoveAudioSetting(gmWhisperManager.audioSettingManager, SETTING_ID);
                owner.AddAudioSetting(gmWhisperManager.audioSettingManager, SETTING_ID, int.MinValue, SettingBroadcast);
                gmWhisperManager.audioSettingManager.UpdateAudioSettings(owner);

                return;
            }
            owner.RemoveAudioSetting(gmWhisperManager.audioSettingManager, SETTING_ID);
            if (_playerIDs.Length > 0) owner.AddAudioSetting(gmWhisperManager.audioSettingManager, SETTING_ID, int.MinValue, SettingOff);
            gmWhisperManager.audioSettingManager.UpdateAudioSettings(owner);
        }
#endif
    }
}
