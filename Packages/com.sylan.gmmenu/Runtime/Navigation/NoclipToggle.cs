
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace Sylan.GMMenu
{
    public class NoclipToggle : GMMenuPart
    {
        [SerializeField] PlayerMover playerMover;
        [SerializeField] Text text;
        [SerializeField] Image image;
        Color activeColor = new Color(207f / 255f, 44f / 255f, 179f / 255f, 191f / 255f);
        Color inactiveColor = new Color(153f / 255f, 76f / 255f, 140f / 255f, 191f / 255f);

        public void ToggleNoclipOnDoubleJump()
        {
            playerMover.noclipOnDoubleJump = !playerMover.noclipOnDoubleJump;
            if (playerMover.noclipOnDoubleJump)
            {
                text.text = "Noclip On Double Jump";
                image.color = activeColor;
            }
            else
            {
                text.text = "Noclip Off";
                image.color = inactiveColor;
                playerMover.noclip = false;
            }
        }
    }
}
