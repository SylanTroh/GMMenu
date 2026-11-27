#if SYLAN_AUDIOMANAGER_VERSION || (COMPILER_UDONSHARP && SYLAN_AUDIOMANAGER)
// See VoiceModeManager for an explanation for the condition above.
#define AUDIOMANAGER
#endif

#if AUDIOMANAGER
using Sylan.AudioManager;
using VRC.SDK3.Data;
#endif
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GMWhisperManager : GMMenuPart
    {
        [SerializeField] private GameObject GMWhisperButton;
        [SerializeField] private Text GMWhisperButtonText;

#if AUDIOMANAGER
        [HideInInspector, SerializeField] public AudioSettingManager audioSettingManager;
        public const string AudioSettingManagerPropertyName = nameof(audioSettingManager);

        private readonly DataDictionary _gmWhisperCache = new DataDictionary();
        private GMWhisper _localGMWhisper;

        private bool _gmWhisperEnabled = false;
#endif

        private void DeactivateSelf()
        {
            GMWhisperButton.SetActive(false);
            gameObject.SetActive(false);
        }

#if !AUDIOMANAGER
        void Start()
        {
            DeactivateSelf();
        }
#else
        void Start()
        {
            if (audioSettingManager == null)
            {
                DeactivateSelf();
                return;
            }
        }

        public void RegisterGMWhisper(VRCPlayerApi player, GMWhisper gmWhisper)
        {
            if (Networking.IsOwner(Networking.LocalPlayer, gmWhisper.gameObject)) _localGMWhisper = gmWhisper;
            Utils.PlayerObjectCache.RegisterPlayerObject(_gmWhisperCache,player,gmWhisper);
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            Utils.PlayerObjectCache.DeregisterPlayerObject(_gmWhisperCache,player);
        }
        public GMWhisper GetLocalGMWhisper()
        {
            return _localGMWhisper;
        }

        private int[] GetPlayerWhisperListenerIDs(VRCPlayerApi player)
        {
            if (Utils.PlayerObjectCache.GetPlayerObject(_gmWhisperCache, player, out GMWhisper gmWhisper))
            {
                return gmWhisper.GetPlayerIDs();
            }
            return new int[0];
        }
        public bool IsWhisperingTo(VRCPlayerApi whisperer, VRCPlayerApi listener)
        {
            int[] ids = GetPlayerWhisperListenerIDs(whisperer);

            if (ids.Length == 0) return false;

            int listenerID = listener.playerId;
            foreach (var t in ids)
            {
                if (t == listenerID) return true;
            }
            return false;
        }
        public void ToggleGMWhisper()
        {
            if (!_gmWhisperEnabled) EnableGMWhisper();
            else DisableGMWhisper();
        }
        private void EnableGMWhisper()
        {
            if (!Utilities.IsValid(_localGMWhisper)) return;
            VRCPlayerApi[] players = gmMenu.PlayerSelector.GetSelectedPlayers();
            int[] playerIDs = new int[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                playerIDs[i] = players[i].playerId;
            }
            _localGMWhisper.SetPlayerIDs(playerIDs);
            _localGMWhisper.RequestSerialization();
            GMWhisperButtonText.text = "Disable GM Whisper";
            _gmWhisperEnabled = true;
        }
        private void DisableGMWhisper()
        {
            if (!Utilities.IsValid(_localGMWhisper)) return;
            _localGMWhisper.ClearPlayerIDs();
            _localGMWhisper.RequestSerialization();
            GMWhisperButtonText.text = "GM Whisper";
            _gmWhisperEnabled = false;
        }
#endif
    }

#if AUDIOMANAGER
    public static class GMWhisperManagerExtensions
    {
        public static bool IsWhisperingTo(this VRCPlayerApi whisperer, VRCPlayerApi listener,
            GMWhisperManager gmWhisperManager)
        {
            return gmWhisperManager.IsWhisperingTo(whisperer, listener);
        }
    }
#endif
}
