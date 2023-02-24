
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using static VRC.SDKBase.VRC_SceneDescriptor;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Teleporter : UdonSharpBehaviour
    {
        private Vector3 prevPos;
        private Quaternion prevRot;
        bool teleportFlag;

        PlayerMover playerMover;
        PlayerSummoner[] playerSummoner;

        private UdonSharpBehaviour[] TeleportEventListeners = new UdonSharpBehaviour[0];
        void Start()
        {
            playerMover = Utils.Modules.PlayerMover(transform);
            playerSummoner = Utils.Modules.PlayerSummoner(transform);
        }
        public void TeleportTo(Vector3 teleportPos, Quaternion teleportRot, SpawnOrientation teleportOrientation = SpawnOrientation.AlignPlayerWithSpawnPoint)
        {
            teleportFlag = true;
            prevPos = Networking.LocalPlayer.GetPosition();
            prevRot = Networking.LocalPlayer.GetRotation();
            if (playerMover.noclip) playerMover.station.position = teleportPos;
            Networking.LocalPlayer.TeleportTo(teleportPos, teleportRot, teleportOrientation);
            GMMenuToggle.UpdateRotationPC(transform);
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
            GMMenuToggle.UpdateRotationPC(transform);
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
            if (!Networking.IsOwner(gameObject)) return;
            SetSummonerOwnership(player);
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
    }
}
