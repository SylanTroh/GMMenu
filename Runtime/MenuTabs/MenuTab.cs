
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MenuTab : UdonSharpBehaviour
    {
        [NotNull] private Transform panelParent;
        [NotNull] private PlayerPermissions Permissions;
        [NotNull] private GMMenuToggle MenuToggle;
        [NotNull] private ToggleByPermission toggleByPermission;

        bool isInitialized = false;
        bool isFirstActive;
        [Header("------Don't Touch------")]
        [NotNull, SerializeField] private GameObject panel;

        void Start()
        {
            //Modules
            Permissions = Utils.Modules.PlayerPermissions(transform);
            MenuToggle = Utils.Modules.GMMenuToggle(transform);
            toggleByPermission = transform.GetComponent<ToggleByPermission>();

            //Delay Module Setup to avoid race conditions
            SendCustomEventDelayedSeconds("EnablePermissionListener", 0.0f);
            SendCustomEventDelayedSeconds("EnableMenuToggleListener", 0.0f);

            panelParent = panel.transform.parent;
            //Only have first panel enabled on start
            ResetPanels();
        }
        //Set DefaultPanel
        public bool IsActive()
        {
            return toggleByPermission.IsActive();
        }
        private bool IsFirstActive()
        {
            foreach (Transform t in transform.parent)
            {
                var menuTab = t.GetComponent<MenuTab>();
                if (!Utilities.IsValid(menuTab)) continue;
                if (menuTab.IsActive())
                {
                    return transform == t;
                }
            }
            return false;
        }
        //Reset to Default Panel
        private void ResetPanels()
        {
            if (isFirstActive) panel.SetActive(true);
            else panel.SetActive(false);
        }
        //Enable Event Listeners
        public void EnablePermissionListener()
        {
            Permissions.AddListener(this);
        }
        public void EnableMenuToggleListener()
        {
            MenuToggle.AddListener(this);
        }
        //Event Listeners
        public void OnPermissionUpdate()
        {
            isFirstActive = IsFirstActive();
        }
        public void OnMenuToggleOn()
        {
            if (!isInitialized)
            {
                isFirstActive = IsFirstActive();
                isInitialized = true;
            }
            ResetPanels();
        }
        //Button Click
        public void OnClick()
        {
            foreach(Transform g in panelParent)
            {
                g.gameObject.SetActive(false);
            }
            panel.SetActive(true);
        }
    }
}
