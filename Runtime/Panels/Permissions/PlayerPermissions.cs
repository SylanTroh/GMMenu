
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerPermissions : UdonSharpBehaviour
    {
        [SerializeField] private bool everyoneIsGM;
        [SerializeField] private string[] GMList;
        [SerializeField] private bool everyoneIsFacilitator;
        [SerializeField] private string[] FacilitatorList;
        [Header("------Don't Touch------")]
        private int _permission = 0;
        private int _tempPermission = 0;
        private UdonSharpBehaviour[] PermissionEventListeners = new UdonSharpBehaviour[0];

        public const int PERMISSION_GM = 2;
        public const int PERMISSION_FACILITATOR = 1;
        public const int PERMISSION_PLAYER = 0;

        void Start()
        {
            if (everyoneIsGM) _permission = PERMISSION_GM;
            else if(everyoneIsFacilitator) _permission = PERMISSION_FACILITATOR;
            else _permission = ResetPermission(GMList, FacilitatorList);
            _tempPermission = _permission;
            Debug.Log("Permission Level:" + getPermissionLevel().ToString());
            SendPermissionUpdateEvent();
        }
        private static int ResetPermission(string[] GMList, string[] FacilitatorList)
        {
            var localName = Networking.LocalPlayer.displayName;
            foreach (string name in GMList)
            {
                if (localName == name) return PERMISSION_GM;
            }
            foreach (string name in FacilitatorList)
            {
                if (localName == name) return PERMISSION_FACILITATOR;
            }
            return 0;
        }
        public int getPermissionLevel()
        {
            return _tempPermission;
        }
        public int getBasePermissionLevel()
        {
            return _permission;
        }
        public void SetTempPermission(int p)
        {
            if (PERMISSION_PLAYER <= p && p <= PERMISSION_GM)
            {
                _tempPermission = p;
                SendPermissionUpdateEvent();
            }
            else Debug.LogError("[GMMenuPermissions]: Permission Level Out of Bounds");
        }
        public void SetTempPermission(string p)
        {
            if (p == "GM") _tempPermission = PERMISSION_GM;
            else if (p == "Facilitator") _tempPermission = PERMISSION_FACILITATOR;
            else if (p == "Player" || p == "Default") _tempPermission = PERMISSION_PLAYER;
            else Debug.LogError("[GMMenuPermissions]: Permission Level Out of Bounds");
            SendPermissionUpdateEvent();
        }
        public void SendPermissionUpdateEvent()
        {
            Debug.Log("[GMMenu]: PermissionUpdateEvent");
            Utils.Events.SendEvent("OnPermissionUpdate", PermissionEventListeners);

        }
        public void AddListener(UdonSharpBehaviour b)
        {
            Utils.ArrayUtils.Append(ref PermissionEventListeners, b);
        }
    }
}