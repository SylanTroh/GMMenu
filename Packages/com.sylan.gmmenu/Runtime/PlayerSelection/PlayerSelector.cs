
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerSelector : GMMenuPart
    {
        DataList selectedPlayers = new DataList();
        public bool ToggleSelection(VRCPlayerApi player)
        {
            if(selectedPlayers.Contains(new DataToken(player)))
            {
                RemovePlayer(player);
                return false;
            }
            AddPlayer(player);
            return true;
        }
        public void AddPlayer(VRCPlayerApi player)
        {
            selectedPlayers.Add(new DataToken(player));
        }
        public void RemovePlayer(VRCPlayerApi player)
        {
            selectedPlayers.RemoveAll(new DataToken(player));
        }
        public void ClearPlayers()
        {
            selectedPlayers.Clear();
            gmMenu.PlayerListViewport.ClearSelectedPlayers();
        }
        public VRCPlayerApi[] GetSelectedPlayers()
        {
            VRCPlayerApi[] playerArray = new VRCPlayerApi[selectedPlayers.Count];
            for(int i = 0; i < selectedPlayers.Count; i++)
            {
                playerArray[i] = (VRCPlayerApi)selectedPlayers[i].Reference;
            }
            return playerArray;
        }
        public VRCPlayerApi[] GetSelectedPlayers(VRCPlayerApi activePlayer)
        {
            if (selectedPlayers.Contains(new DataToken(activePlayer)))
            {
                return GetSelectedPlayers();
            }
            VRCPlayerApi[] playerArray = new VRCPlayerApi[selectedPlayers.Count+1];
            for (int i = 0; i < selectedPlayers.Count; i++)
            {
                playerArray[i] = (VRCPlayerApi)selectedPlayers[i].Reference;
            }
            playerArray[selectedPlayers.Count] = activePlayer;
            return playerArray;
        }
    }
}
