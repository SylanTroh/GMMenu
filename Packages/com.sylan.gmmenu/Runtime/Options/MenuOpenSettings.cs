
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MenuOpenSettings : GMMenuPart
    {
        public bool onLeftHand = true;
        public int openMode = 0;
        [Header("Don't Touch")]
        [SerializeField] private GameObject optionsTab;
        [SerializeField] Image leftHandButton, rightHandButton, downUpButton, downDownButton, holdDownButton;
        Color activeColor = new Color(207f / 255f, 44f / 255f, 179f / 255f, 191f / 255f);
        Color inactiveColor = new Color(153f / 255f, 76f / 255f, 140f / 255f, 191f / 255f);

        [SerializeField] HorizontalLayoutGroup bottomBar;
        void Start()
        {
            if (!Networking.LocalPlayer.IsUserInVR())
            {
                Destroy(optionsTab);
                return;
            }
            SetLeftHand();
            InputLookDownUp();
        }

        public void SetLeftHand()
        {
            onLeftHand = true;
            rightHandButton.color = inactiveColor;
            leftHandButton.color = activeColor;
            gmMenu.GMMenuToggle.SetLeftHand();
            bottomBar.childAlignment = TextAnchor.UpperRight;
        }
        public void SetRightHand()
        {
            onLeftHand = false;
            rightHandButton.color = activeColor;
            leftHandButton.color = inactiveColor;
            gmMenu.GMMenuToggle.SetRightHand();
            bottomBar.childAlignment = TextAnchor.UpperLeft;
        }
        public void InputLookDownUp()
        {
            openMode = 0;
            downUpButton.color = activeColor;
            downDownButton.color = inactiveColor;
            holdDownButton.color = inactiveColor;
        }
        public void InputLookDownDown()
        {
            openMode = 1;
            downUpButton.color = inactiveColor;
            downDownButton.color = activeColor;
            holdDownButton.color = inactiveColor;
        }
        public void InputLookHoldDown()
        {
            openMode = 2;
            downUpButton.color = inactiveColor;
            downDownButton.color = inactiveColor;
            holdDownButton.color = activeColor;
        }
    }
}
