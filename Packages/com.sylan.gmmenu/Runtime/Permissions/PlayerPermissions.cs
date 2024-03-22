
using System.Security.Policy;
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using System.Text.RegularExpressions;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerPermissions : GMMenuPart
    {
        [Header("Elevate Default Permission for Everyone")]
        [SerializeField] private bool everyoneIsGM;
        [SerializeField] private bool everyoneIsFacilitator;
        [SerializeField] private bool everyoneIsDisabled;

        [Header("Optional List of Names")]
        [SerializeField] private string[] GMList;
        [SerializeField] private string[] FacilitatorList;
        [SerializeField] private string[] DisabledList;

        [Header("Optional URL with List of Names" +
            "\n" +
            "One Name Per Line (Case Sensitive)")]
        [SerializeField] public VRCUrl GMListURL;
        [SerializeField] public VRCUrl FacilitatorListURL;
        [SerializeField] public VRCUrl DisabledListURL;


        [Header("------Don't Touch------")]
        private int _permission = 0;
        private int _tempPermission = 0;
        private UdonSharpBehaviour[] PermissionEventListeners = new UdonSharpBehaviour[0];

        public const int PERMISSION_GM = 2;
        public const int PERMISSION_FACILITATOR = 1;
        public const int PERMISSION_PLAYER = 0;
        public const int PERMISSION_DISABLED = -1;

        void Start()
        {
            if (everyoneIsDisabled) _permission = Mathf.Min(_permission, PERMISSION_DISABLED);

            _permission = GetPermissionFromLists();

            if (everyoneIsGM) _permission = Mathf.Max(_permission, PERMISSION_GM);
            if (everyoneIsFacilitator) _permission = Mathf.Max(_permission, PERMISSION_FACILITATOR);

            _tempPermission = _permission;
            Debug.Log("Permission Level:" + getPermissionLevel().ToString());
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
            foreach (string name in DisabledList)
            {
                if (localName == name) return PERMISSION_DISABLED;
            }
            return 0;
        }
        public void LoadPermissionLists()
        {
            if(GMListURL != VRCUrl.Empty)
            {
                VRCStringDownloader.LoadUrl(GMListURL, (IUdonEventReceiver)this);
            }
            if (FacilitatorListURL != VRCUrl.Empty)
            {
                VRCStringDownloader.LoadUrl(FacilitatorListURL, (IUdonEventReceiver)this);
            }
            if (DisabledListURL != VRCUrl.Empty)
            {
                VRCStringDownloader.LoadUrl(DisabledListURL, (IUdonEventReceiver)this);
            }
        }
        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            var s = result.Result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (result.Url == GMListURL)
            {
                GMList = s;
            }
            else if(result.Url == FacilitatorListURL)
            {
                FacilitatorList = s;
            }
            else if(result.Url == DisabledListURL)
            {
                DisabledList = s;
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
        public void SetTempPermission(int p)
        {
            if (PERMISSION_DISABLED <= p && p <= PERMISSION_GM)
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
            else if (p == "Disabled") _tempPermission = PERMISSION_DISABLED;
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
            foreach(string player in GMList)
            {
                if (PlayerName == player)
                {
                    return 2;
                }
            }
            foreach(string player in FacilitatorList)
            {
                if (PlayerName == player)
                {
                    return 1;
                }
            }
            foreach (string player in DisabledList)
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
