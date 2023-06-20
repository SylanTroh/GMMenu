
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MenuTab : GMMenuPart
    {
        [NotNull] private Transform panelParent;
        [NotNull] private PlayerPermissions Permissions;
        [NotNull] private GMMenuToggle MenuToggle;
        private ToggleByPermission toggleByPermission;

        bool isInitialized = false;
        bool isFirstActive;
        [Header("------Don't Touch------")]
        [NotNull, SerializeField] private CanvasGroup panel;

        void Start()
        {
            //Modules
            Permissions = gmMenu.PlayerPermissions;
            MenuToggle = gmMenu.GMMenuToggle;
            toggleByPermission = transform.GetComponent<ToggleByPermission>();

            Permissions.AddListener(this);
            MenuToggle.AddListener(this);

            panelParent = panel.transform.parent;
            //Only have first panel enabled on start
            ResetPanels();
        }
        //Set DefaultPanel
        public bool IsActive()
        {
            if (!Utilities.IsValid(toggleByPermission)) return true;
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
        //Event Listeners
        public void OnPermissionUpdate()
        {
            SendCustomEventDelayedFrames(nameof(SetFirstActive), 1);
        }
        public void SetFirstActive()
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
                g.GetComponent<CanvasGroup>().SetActive(false);
            }
            panel.SetActive(true);
            gmMenu.PlayerSelector.ClearPlayers();
        }
    }
    public static class CanvasGroupExtensions
    {
        public static void SetActive(this CanvasGroup canvasGroup,bool value)
        {
            if (value)
            {
                canvasGroup.alpha = 1;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                return;
            }
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
