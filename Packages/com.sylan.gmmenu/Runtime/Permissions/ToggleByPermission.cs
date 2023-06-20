
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.Diagnostics;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ToggleByPermission : GMMenuPart
    {
        [NotNull] private PlayerPermissions Permissions;
        [Header("Set who has permissions to view this Gameobject")]
        [NotNull, SerializeField] private bool permissionGM;
        [NotNull, SerializeField] private bool permissionFacilitator;
        [NotNull, SerializeField] private bool permissionPlayer;
        [NotNull] private int permissionMask = 0;
        [NotNull,SerializeField] bool useBasePermissionLevel;

        [NotNull, SerializeField] private GameObject[] gameObjects = new GameObject[0];
        bool isActive = false;

        void Start()
        {
            //This is a bitmask to represent who has permissions to view the object.
            if (permissionGM) permissionMask += 4;
            if (permissionFacilitator) permissionMask += 2;
            if (permissionPlayer) permissionMask += 1;

            Permissions = gmMenu.PlayerPermissions;

            Permissions.AddListener(this);
            OnPermissionUpdate();
        }
        public void OnPermissionUpdate()
        {
            var permissionLevel = (useBasePermissionLevel) ? Permissions.getBasePermissionLevel() : Permissions.getPermissionLevel();
            isActive = ((1 << permissionLevel) & permissionMask) > 0;
            foreach (GameObject obj in gameObjects)
            {
                obj.SetActive(isActive);
            }
        }
        public bool IsActive()
        {
            return isActive;
        }
    }
}
