
using System.Threading;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerSummoner : UdonSharpBehaviour
    {
        Teleporter teleporter;

        const int OWNER_NULL = -1;
        const int REQUESTER_NULL = -1;

        [FieldChangeCallback(nameof(owner))]
        VRCPlayerApi _owner;
        [UdonSynced]
        int _ownerID = OWNER_NULL;

        [UdonSynced, FieldChangeCallback(nameof(requester))]
        int _requester = OWNER_NULL;
        void Start()
        {
            teleporter = Utils.Modules.Teleporter(transform);
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
                    _requester = REQUESTER_NULL;

                    RequestSerialization();
                    return;
                }
                var id = VRCPlayerApi.GetPlayerId(value);
                if (id != _ownerID) requester = REQUESTER_NULL;
                _ownerID = id;
                RequestSerialization();
            }
            get => _owner;
        }
        public int requester
        {
            set
            {
                _requester = value;
                RequestSerialization();
            }
            get => _requester;
        }

        public override void OnDeserialization()
        {
            if (_ownerID == OWNER_NULL)
            {
                _owner = null;
                return;
            }
            _owner = VRCPlayerApi.GetPlayerById(_ownerID);

            if (_owner != Networking.LocalPlayer) return;

            var teleportTarget = VRCPlayerApi.GetPlayerById(requester);
            if (!Utilities.IsValid(teleportTarget)) return;
            teleporter.TeleportToPlayer(teleportTarget);
        }
    }
}
