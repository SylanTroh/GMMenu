
using Sylan.AudioManager;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GMWhisperManager : GMMenuPart
    {
        [HideInInspector] public GMWhisper localGMWhisper;
        private GMWhisper[] gmWhispers;
        [SerializeField] public AudioSettingManager audioSettingManager;

        [SerializeField] GameObject GMWhisperButton;
        [SerializeField] Text GMWhisperButtonText;
        bool gmWhisperEnabled = false;

        void Start()
        {
            if (audioSettingManager == null)
            {
                Destroy(GMWhisperButton.gameObject);
                Destroy(gameObject);
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
            for(int i = 0; i < players.Length; i++)
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
    }
}
