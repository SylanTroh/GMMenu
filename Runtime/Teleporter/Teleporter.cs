
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    public class Teleporter : UdonSharpBehaviour
    {
        private Vector3 prevPos;
        private Quaternion prevRot;
        bool teleportFlag;

        PlayerMover playerMover;

        private UdonSharpBehaviour[] TeleportEventListeners = new UdonSharpBehaviour[0];
        private void Start()
        {
            playerMover = Utils.Modules.PlayerMover(transform);
        }
        public void TeleportTo(Vector3 teleportPos, Quaternion teleportRot)
        {
            teleportFlag = true;
            prevPos = Networking.LocalPlayer.GetPosition();
            prevRot = Networking.LocalPlayer.GetRotation();
            if (playerMover.noclip) playerMover.station.position = teleportPos;
            Networking.LocalPlayer.TeleportTo(teleportPos, teleportRot);
            GMMenuToggle.UpdateRotationPC(transform);
            SendTeleportEvent();
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
    }
}
