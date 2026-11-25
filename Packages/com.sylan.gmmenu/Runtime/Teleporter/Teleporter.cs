using UdonSharp;
using UnityEngine;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using static VRC.SDKBase.VRC_SceneDescriptor;

namespace Sylan.GMMenu
{
    // Apparently using NoVariableSync does not allow for sending network events,
    // VRChat complains that it cannot send networked events on sync mode none scripts.
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Teleporter : GMMenuPart
    {
        private Vector3 prevPos;
        private Quaternion prevRot;
        bool teleportFlag;

        PlayerMover playerMover;
        [SerializeField] PlayerSummoner[] playerSummoner;

        [SerializeField] public GameObject summonWarning;

        [System.NonSerialized] public PlayerSummoner localPlayerSummoner;
        private const float EnsureHasLocalPlayerSummonerLoopInterval = 5f;

        private UdonSharpBehaviour[] TeleportEventListeners = new UdonSharpBehaviour[0];
        void Start()
        {
            playerMover = gmMenu.PlayerMover;
        }
        public void TeleportTo(Vector3 teleportPos, Quaternion teleportRot, SpawnOrientation teleportOrientation = SpawnOrientation.AlignPlayerWithSpawnPoint)
        {
            teleportFlag = true;
            prevPos = Networking.LocalPlayer.GetPosition();
            prevRot = Networking.LocalPlayer.GetRotation();
            if (playerMover.noclip) playerMover.station.position = teleportPos;
            Networking.LocalPlayer.TeleportTo(teleportPos, teleportRot, teleportOrientation);
            gmMenu.GMMenuToggle.UpdateRotationPC();
            SendTeleportEvent();
        }
        public void TeleportTo(Transform teleportTransform, SpawnOrientation teleportOrientation = SpawnOrientation.AlignPlayerWithSpawnPoint)
        {
            TeleportTo(teleportTransform.position,teleportTransform.rotation, teleportOrientation);
        }
        public void UndoTeleport()
        {
            if (!teleportFlag) return;
            if (!Utilities.IsValid(prevPos) || !Utilities.IsValid(prevRot)) return;
            var playerPos = Networking.LocalPlayer.GetPosition();
            var playerRot = Networking.LocalPlayer.GetRotation();
            if (playerMover.noclip) playerMover.station.position = prevPos;
            Networking.LocalPlayer.TeleportTo(prevPos, prevRot);
            prevPos = playerPos;
            prevRot = playerRot;
            gmMenu.GMMenuToggle.UpdateRotationPC();
            SendTeleportEvent();
        }
        public void TeleportToPlayer(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player))
            {
                Debug.Log("[Teleporter]: Invalid Player");
                return;
            }
            Debug.Log("[Teleporter]: Teleporting to " + player.displayName);
            TeleportTo(player.GetPosition(), player.GetRotation()); ;
        }
        //Events
        private void SendTeleportEvent()
        {
            Utils.Events.SendEvent("OnTeleport", TeleportEventListeners);
        }
        public void AddListener(UdonSharpBehaviour b)
        {
            Utils.ArrayUtils.Append(ref TeleportEventListeners, b);
        }
        public void SummonPlayer(VRCPlayerApi player)
        {
            var summoner = GetSummonerByOwner(player);
            Networking.SetOwner(Networking.LocalPlayer,summoner.gameObject);
            summoner.requester = VRCPlayerApi.GetPlayerId(Networking.LocalPlayer);
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.IsOwner(gameObject))
            {
                SetSummonerOwnership(player);
            }
            else
            {
                SendCustomEventDelayedSeconds(nameof(EnsureLocalPlayerSummonerGotAssignedLoop), EnsureHasLocalPlayerSummonerLoopInterval);
            }
        }
        private void SetSummonerOwnership(VRCPlayerApi player)
        {
            foreach (PlayerSummoner summoner in playerSummoner)
            {
                if (summoner.owner != null) continue;

                summoner.owner = player;
                return;
            }
        }
        public void EnsureLocalPlayerSummonerGotAssignedLoop()
        {
            if (localPlayerSummoner != null) return;
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(RequestReconfirmationOfPlayerSummonerAssignment), Networking.LocalPlayer.playerId);
            SendCustomEventDelayedSeconds(nameof(EnsureLocalPlayerSummonerGotAssignedLoop), EnsureHasLocalPlayerSummonerLoopInterval);
        }
        [NetworkCallable(maxEventsPerSecond: 20)]
        public void RequestReconfirmationOfPlayerSummonerAssignment(int playerId)
        {
            VRCPlayerApi player = VRCPlayerApi.GetPlayerById(playerId);
            if (!Utilities.IsValid(player)) return;
            PlayerSummoner summoner = GetSummonerByOwner(player);
            // VRChat is supposed to guarantee that every player is going to agree what the
            // latest synced state of a script is.
            // Turns out when requesting serialization at exactly around the time when a player joins,
            // the synced data will be sent to all players, except sometimes not to the joining player.
            if (summoner != null)
            {
                // Re-request serialization to work around the issue.
                summoner.owner = player;
            }
            else
            {
                // Oh, they actually have no script assigned to them,
                // might happen when the master left at an inopportune time.
                SetSummonerOwnership(player);
            }
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            RevokeSummonerOwnership(player);
        }
        private void RevokeSummonerOwnership(VRCPlayerApi player)
        {
            foreach (PlayerSummoner summoner in playerSummoner)
            {
                if (summoner.owner != player) continue;

                summoner.owner = null;
            }
        }
        //Get PlayerSummoner that belongs to a specific player, or a list of players
        public PlayerSummoner GetSummonerByOwner(VRCPlayerApi player)
        {
            foreach (PlayerSummoner summoner in playerSummoner)
            {
                if (summoner.owner == player) return summoner;
            }
            return null;
        }
        public void SummonWarning(bool state)
        {
            summonWarning.SetActive(state);
        }
    }
}
