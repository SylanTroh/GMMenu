
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using JetBrains.Annotations;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TeleportButton : UdonSharpBehaviour
    {
        [NotNull,SerializeField] Transform GMMenu;
        [NotNull] Teleporter teleporter;
        public Transform teleportPosition;
        public VRC_SceneDescriptor.SpawnOrientation teleportOrientation;
        private void Start()
        {
            teleporter = Utils.Modules.Teleporter(GMMenu.transform);
        }
        public override void Interact()
        {
            VRCPlayerApi player = Networking.LocalPlayer;
            teleporter.TeleportTo(teleportPosition, teleportOrientation);
        }
    }
}
