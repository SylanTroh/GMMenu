using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using UnityEngine.Serialization;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerPermissions : GMMenuPart
    {
        [Header("Elevate Default Permission for Everyone")]
        [SerializeField, FormerlySerializedAs("everyoneIsGM")] private bool playersAreGM;
        [SerializeField, FormerlySerializedAs("everyoneIsFacilitator")] private bool playersAreFacilitator;
        [SerializeField, FormerlySerializedAs("everyoneIsDisabled")] private bool playersAreDeactivated;

        //Variables to store the list of VRChat usernames
        [Header("Optional List of Names")]
        [SerializeField] private string[] GMList;
        [SerializeField] private string[] FacilitatorList;
        [SerializeField, FormerlySerializedAs("DisabledList")] private string[] DeactivatedList;

        //URL for String Loading
        [Header("Optional URL with List of Names" +
            "\n" +
            "One Name Per Line (Case Sensitive)")]
        [SerializeField] public VRCUrl GMListURL;
        [SerializeField] public VRCUrl FacilitatorListURL;
        [SerializeField, FormerlySerializedAs("DisabledListURL")] public VRCUrl DeactivatedListURL;


        [Header("------Don't Touch------")]
        private int _permission = 0;
        private int _tempPermission = 0;
        private UdonSharpBehaviour[] PermissionEventListeners = new UdonSharpBehaviour[0];

        public const int PERMISSION_GM = 2;
        public const int PERMISSION_FACILITATOR = 1;
        public const int PERMISSION_PLAYER = 0;
        public const int PERMISSION_DEACTIVATED = -1;

        void Start()
        {
            if (playersAreDeactivated) SetBasePermission(PERMISSION_DEACTIVATED);
            if (playersAreFacilitator) SetBasePermission(PERMISSION_FACILITATOR);
            if (playersAreGM) SetBasePermission(PERMISSION_GM);

            var listPermission = GetPermissionFromLists();
            _permission = listPermission == 0 ? _permission : listPermission;

            SetTempPermission(_permission);
            SendPermissionUpdateEvent();

            LoadPermissionLists();
        }
        private int GetPermissionFromLists()
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
            foreach (string name in DeactivatedList)
            {
                if (localName == name) return PERMISSION_DEACTIVATED;
            }
            return 0;
        }
        public void LoadPermissionLists()
        {
            if (GMListURL != VRCUrl.Empty)
            {
                VRCStringDownloader.LoadUrl(GMListURL, (IUdonEventReceiver)this);
            }
            if (FacilitatorListURL != VRCUrl.Empty)
            {
                VRCStringDownloader.LoadUrl(FacilitatorListURL, (IUdonEventReceiver)this);
            }
            if (DeactivatedListURL != VRCUrl.Empty)
            {
                VRCStringDownloader.LoadUrl(DeactivatedListURL, (IUdonEventReceiver)this);
            }
        }
        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            var s = result.Result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (result.Url == GMListURL)
            {
                GMList = s;
            }
            else if (result.Url == FacilitatorListURL)
            {
                FacilitatorList = s;
            }
            else if (result.Url == DeactivatedListURL)
            {
                DeactivatedList = s;
            }

            _permission = GetPermissionFromLists();
            _tempPermission = Mathf.Max(_permission, _tempPermission);
            SendPermissionUpdateEvent();
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
        }
        public int getPermissionLevel()
        {
            return _tempPermission;
        }
        public int getBasePermissionLevel()
        {
            return _permission;
        }
        public void SetBasePermission(int p)
        {
            if (PERMISSION_DEACTIVATED <= p && p <= PERMISSION_GM)
            {
                _permission = p;
                SendPermissionUpdateEvent();
                Debug.Log("[GMMenuPermissions]: Base Permission Level:" + getPermissionLevel().ToString());
            }
            else Debug.LogError("[GMMenuPermissions]: Permission Level Out of Bounds");
        }
        public void SetTempPermission(int p)
        {
            if (PERMISSION_DEACTIVATED <= p && p <= PERMISSION_GM)
            {
                _tempPermission = p;
                SendPermissionUpdateEvent();
                Debug.Log("[GMMenuPermissions]: Permission Level:" + getPermissionLevel().ToString());
            }
            else Debug.LogError("[GMMenuPermissions]: Permission Level Out of Bounds");
        }
        public void SetTempPermission(string p)
        {
            if (p == "GM") _tempPermission = PERMISSION_GM;
            else if (p == "Facilitator") _tempPermission = PERMISSION_FACILITATOR;
            else if (p == "Player" || p == "Default") _tempPermission = PERMISSION_PLAYER;
            else if (p == "Disabled" || p == "Deactivated") _tempPermission = PERMISSION_DEACTIVATED;
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
        public int GetPlayerBasePermLevel(string PlayerName)
        {
            foreach (string player in GMList)
            {
                if (PlayerName == player)
                {
                    return 2;
                }
            }
            foreach (string player in FacilitatorList)
            {
                if (PlayerName == player)
                {
                    return 1;
                }
            }
            foreach (string player in DeactivatedList)
            {
                if (PlayerName == player)
                {
                    return -1;
                }
            }
            return 0;
        }
    }
}
