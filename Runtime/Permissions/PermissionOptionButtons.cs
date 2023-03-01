
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PermissionOptionButtons : UdonSharpBehaviour
    {
        [NotNull] private PlayerPermissions Permissions;
        [NotNull,SerializeField] Image gmButton, facilitatorButton, playerButton;
        Color activeColor = new Color(207f / 255f, 44f / 255f, 179f / 255f, 191f / 255f);
        Color inactiveColor = new Color(153f / 255f, 76f / 255f, 140f / 255f, 191f / 255f);

        void Start()
        {
            Permissions = Utils.Modules.PlayerPermissions(transform);

            SendCustomEventDelayedSeconds(nameof(EnablePermissionListener), 0.0f);
            SendCustomEventDelayedSeconds(nameof(OnPermissionUpdate), 0.0f);
        }
        public void EnablePermissionListener()
        {
            Permissions.AddListener(this);
        }
        public void SetPermissionGM()
        {
            Permissions.SetTempPermission(PlayerPermissions.PERMISSION_GM);
        }
        public void SetPermissionFacilitator()
        {
            Permissions.SetTempPermission(PlayerPermissions.PERMISSION_FACILITATOR);
        }
        public void SetPermissionPlayer()
        {
            Permissions.SetTempPermission(PlayerPermissions.PERMISSION_PLAYER);
        }
        public void OnPermissionUpdate()
        {
            var permissionLevel = Permissions.getPermissionLevel();
            gmButton.color = inactiveColor;
            facilitatorButton.color = inactiveColor;
            playerButton.color = inactiveColor;
            switch (permissionLevel)
            {
                case PlayerPermissions.PERMISSION_GM:
                    gmButton.color = activeColor;
                    break;
                case PlayerPermissions.PERMISSION_FACILITATOR:
                    facilitatorButton.color = activeColor;
                    break;
                case PlayerPermissions.PERMISSION_PLAYER:
                    playerButton.color = activeColor;
                    break;
            }
        }
    }
}
