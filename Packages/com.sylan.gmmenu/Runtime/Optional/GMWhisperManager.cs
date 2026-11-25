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
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Sylan.GMMenu
{
    // Apparently using NoVariableSync does not allow for sending network events,
    // VRChat complains that it cannot send networked events on sync mode none scripts.
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GMWhisperManager : GMMenuPart
    {
        [SerializeField] GameObject GMWhisperButton;
        [HideInInspector] public GMWhisper localGMWhisper;
        [SerializeField] Text GMWhisperButtonText;
        private const float EnsureHasLocalGMWhisperLoopInterval = 5f;

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
            if (Networking.IsOwner(gameObject))
            {
                SetOwnership(player);
            }
            else
            {
                SendCustomEventDelayedSeconds(nameof(EnsureLocalGMWhisperGotAssignedLoop), EnsureHasLocalGMWhisperLoopInterval);
            }
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
        public void EnsureLocalGMWhisperGotAssignedLoop()
        {
            if (localGMWhisper != null) return;
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(RequestReconfirmationOfGMWhisperAssignment), Networking.LocalPlayer.playerId);
            SendCustomEventDelayedSeconds(nameof(EnsureLocalGMWhisperGotAssignedLoop), EnsureHasLocalGMWhisperLoopInterval);
        }
        [NetworkCallable(maxEventsPerSecond: 20)]
        public void RequestReconfirmationOfGMWhisperAssignment(int playerId)
        {
            VRCPlayerApi player = VRCPlayerApi.GetPlayerById(playerId);
            if (!Utilities.IsValid(player)) return;
            GMWhisper gmWhisper = GetGMWhisperByOwner(player);
            // VRChat is supposed to guarantee that every player is going to agree what the
            // latest synced state of a script is.
            // Turns out when requesting serialization at exactly around the time when a player joins,
            // the synced data will be sent to all players, except sometimes not to the joining player.
            if (gmWhisper != null)
            {
                // Re-request serialization to work around the issue.
                gmWhisper.owner = player;
            }
            else
            {
                // Oh, they actually have no script assigned to them,
                // might happen when the master left at an inopportune time.
                SetOwnership(player);
            }
        }
        public GMWhisper GetGMWhisperByOwner(VRCPlayerApi player)
        {
            foreach (GMWhisper gmWhisper in gmWhispers)
            {
                if (gmWhisper.owner == player) return gmWhisper;
            }
            return null;
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
