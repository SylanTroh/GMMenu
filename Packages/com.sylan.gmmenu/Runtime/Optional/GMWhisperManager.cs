#if SYLAN_AUDIOMANAGER_VERSION || (COMPILER_UDONSHARP && SYLAN_AUDIOMANAGER)
// See VoiceModeManager for an explanation for the condition above.
#define AUDIOMANAGER
#endif

#if AUDIOMANAGER
using Sylan.AudioManager;
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
        [SerializeField] GameObject GMWhisperButton;
        [HideInInspector] public GMWhisper localGMWhisper;
        [SerializeField] Text GMWhisperButtonText;

#if AUDIOMANAGER
        [HideInInspector, SerializeField] public AudioSettingManager audioSettingManager;
        public const string AudioSettingManagerPropertyName = nameof(audioSettingManager);

        private GMWhisper[] gmWhispers;
        bool gmWhisperEnabled = false;
#endif

        private void DestroySelf()
        {
            // Destroy(GMWhisperButton.gameObject);
            Destroy(gameObject);
        }

#if !AUDIOMANAGER
        void Start()
        {
            DestroySelf();
        }
#else
        void Start()
        {
            if (audioSettingManager == null)
            {
                DestroySelf();
                gmWhispers = new GMWhisper[0]; // Prevent error in SetOwnership, since Destroy is not instant.
                return;
            }
            gmWhispers = GetComponentsInChildren<GMWhisper>();
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            SetOwnership(player);
        }
        private void SetOwnership(VRCPlayerApi player)
        {
            foreach (GMWhisper gmWhisper in gmWhispers)
            {
                if (gmWhisper.owner != null) continue;

                gmWhisper.owner = player;
                gmWhisper.RequestSerialization();
                return;
            }
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            RevokeOwnership(player);
        }
        private void RevokeOwnership(VRCPlayerApi player)
        {
            foreach (GMWhisper gmWhisper in gmWhispers)
            {
                if (gmWhisper.owner != player) continue;

                gmWhisper.owner = null;
                gmWhisper.RequestSerialization();
            }
        }
        public GMWhisper GetLocalGMWhisper()
        {
            return localGMWhisper;
        }
        public void ToggleGMWhisper()
        {
            gmWhisperEnabled = !gmWhisperEnabled;
            if (gmWhisperEnabled) EnableGMWhisper();
            else DisableGMWhisper();
        }
        private void EnableGMWhisper()
        {
            VRCPlayerApi[] players = gmMenu.PlayerSelector.GetSelectedPlayers();
            int[] playerIDs = new int[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                playerIDs[i] = players[i].playerId;
            }
            if (!Networking.IsOwner(localGMWhisper.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, localGMWhisper.gameObject);
            localGMWhisper.playerIDs = playerIDs;
            localGMWhisper.RequestSerialization();
            GMWhisperButtonText.text = "Disable GM Whisper";
        }
        private void DisableGMWhisper()
        {
            if (!Networking.IsOwner(localGMWhisper.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, localGMWhisper.gameObject);
            localGMWhisper.playerIDs = new int[0];
            localGMWhisper.RequestSerialization();
            GMWhisperButtonText.text = "GM Whisper";
        }
#endif
    }
}
