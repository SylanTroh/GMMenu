using UdonSharp;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerSummoner : GMMenuPart
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
            teleporter = gmMenu.Teleporter;
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
                if (_owner == Networking.LocalPlayer) teleporter.localPlayerSummoner = this;
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
            teleporter.localPlayerSummoner = this;

            var teleportTarget = VRCPlayerApi.GetPlayerById(requester);
            if (!Utilities.IsValid(teleportTarget)) return;
            teleporter.SummonWarning(true);
            SendCustomEventDelayedSeconds(nameof(TeleportEvent), 3);
        }
        public void TeleportEvent()
        {
            var teleportTarget = VRCPlayerApi.GetPlayerById(requester);
            if (!Utilities.IsValid(teleportTarget)) return;
            teleporter.TeleportToPlayer(teleportTarget);
            teleporter.SummonWarning(false);
        }
    }
}
