
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using UnityEngine.UI;
using System;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerPanel : PanelTemplate
    {    
        //Gets initialized by PlayerListViewport

        public Text playerName;

        public void DrawPanel(VRCPlayerApi p)
        {
            player = p;
            name = player.playerId.ToString();
            transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            playerName.text = player.displayName;

            WatchCamera.SetThumbnailUV(image, thumbnailID);

            SetBorderColor();

            if (!gameObject.activeSelf) gameObject.SetActive(true);
        }
        public void ToggleSelectPlayer()
        {
            selected = gmMenu.PlayerSelector.ToggleSelection(player);
            if (selected)
            {
                backgroundImage.color = selectedColor;
                return;
            }
            SetBorderColor();
        }
        public void ClearSelectedPlayer()
        {
            selected = false;
            SetBorderColor();
        }
    }
}
