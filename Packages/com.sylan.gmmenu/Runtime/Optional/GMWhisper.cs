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
        private const string SETTING_ID = "GMWhisper";

#if AUDIOMANAGER

        [UdonSynced]
        int[] _playerIDs = new int[0];

        public int[] GetPlayerIDs()
        {
            return _playerIDs;
        }

        public void SetPlayerIDs(int[] playerIDs)
        {
            _playerIDs = playerIDs;
        }

        public void ClearPlayerIDs()
        {
            _playerIDs = new int[0];
        }

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

        private void Start()
        {
            if (gmWhisperManager.audioSettingManager == null) return;
            gmWhisperManager.RegisterGMWhisper(Networking.GetOwner(gameObject),this);
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (player != Networking.GetOwner(gameObject)) return;
            SetGMWhisper();
        }
        public override void OnDeserialization()
        {
            SetGMWhisper();
        }

        public void SetGMWhisper()
        {
            if (gmWhisperManager.audioSettingManager == null) return;
            if (Networking.IsOwner(gameObject)) return;

            var owner = Networking.GetOwner(gameObject);
            int localID = Networking.LocalPlayer.playerId;
            bool selectedFlag = false;
            foreach (var id in _playerIDs)
            {
                if (id != localID)  continue;
                selectedFlag = true;
                break;
            }
            owner.RemoveAudioSetting(gmWhisperManager.audioSettingManager, SETTING_ID);
            if(selectedFlag) owner.AddAudioSetting(gmWhisperManager.audioSettingManager, SETTING_ID, int.MinValue, SettingBroadcast);
            else if (_playerIDs.Length > 0) owner.AddAudioSetting(gmWhisperManager.audioSettingManager, SETTING_ID, int.MinValue, SettingOff);

            gmWhisperManager.audioSettingManager.UpdateAudioSettings(owner);
        }
#endif
    }
}
