
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerListViewport : GMMenuPart
    {
        public GameObject PlayerPanelTemplate;
        public Transform PlayerListContent;
        [NotNull] GMMenuToggle menuToggle;
        [NotNull] Teleporter teleporter;
        [NotNull] WatchCamera watchCamera;
        [NotNull] PlayerPermissions playerPermissions;

        private DataList playerList = new DataList() { };
        private DataList playerNameList = new DataList() { };
        VRCPlayerApi[] players = new VRCPlayerApi[0];
        PlayerPanel[] panels = new PlayerPanel[0];

        private void Start()
        {
            menuToggle = gmMenu.GMMenuToggle;
            playerPermissions = gmMenu.PlayerPermissions;
            teleporter = gmMenu.Teleporter;
            watchCamera = gmMenu.WatchCamera;

            menuToggle.AddListener(this);
            //InitializePlayerList(); VRChat fires OnPlayerJoined Events on world join so this is not needed
        }
        //private void InitializePlayerList()
        //{
        //    VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        //    VRCPlayerApi.GetPlayers(players);
        //    foreach (VRCPlayerApi player in players)
        //    {
        //        InsertPlayer(player);
        //    }
        //    CopyPlayersToArray();
        //}
        private void InsertPlayer(VRCPlayerApi player)
        {
            int insertIndex = playerNameList.BinarySearch(player.displayName);
            if (insertIndex < 0) insertIndex = ~insertIndex;
            if (insertIndex >= playerList.Count)
            {
                playerList.Add(new DataToken(player));
                playerNameList.Add(player.displayName);
            }
            else
            {
                playerList.Insert(insertIndex,new DataToken(player));
                playerNameList.Insert(insertIndex, player.displayName);
            }

        }
        private void RemovePlayer(VRCPlayerApi player)
        {
            int removeIndex = playerNameList.BinarySearch(player.displayName);
            if (removeIndex < 0)
            {
                Debug.LogError("[PlayerListViewport] Tried to remove player not in PlayerList");
                return;
            }
            playerList.RemoveAt(removeIndex);
            playerNameList.RemoveAt(removeIndex);
        }
        private void CopyPlayersToArray()
        {
            players = new VRCPlayerApi[playerList.Count];
            for(int i = 0; i < players.Length; i++)
            {
                players[i] = (VRCPlayerApi)playerList[i].Reference;
            }
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            InsertPlayer(player);
            CopyPlayersToArray();
            SendCustomEventDelayedFrames(nameof(UpdateViewport), 1);
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            RemovePlayer(player);
            CopyPlayersToArray();
            SendCustomEventDelayedFrames(nameof(UpdateViewport), 1);
        }
        // Events
        public void OnMenuToggleOn()
        {
            UpdateViewportContinuous();
        }
        public void OnNewMessage()
        {
            UpdateViewport();
        }
        public void OnMessageUpdate()
        {
            UpdateViewport();
        }
        //UI Functions
        public void UpdateViewport()
        {
            ShowPlayers(players);
        }
        public void UpdateViewportContinuous()
        {
            UpdateViewport();
            if (menuToggle.MenuState()) SendCustomEventDelayedSeconds(nameof(UpdateViewportContinuous), 5.0f);
        }
        private void ShowPlayers(VRCPlayerApi[] players)
        {
            if (!Utilities.IsValid(players)) return;
            int playerNum = 0;
            foreach (VRCPlayerApi player in players)
            {
                if (!Utilities.IsValid(player)) continue;
                if (player == Networking.LocalPlayer) continue;
                if (playerNum >= panels.Length) InstansiatePlayerPanel();
                panels[playerNum].DrawPanel(player);
                playerNum++;
            }
            for (int i = playerNum; i < panels.Length; i++)
            {
                panels[i].gameObject.SetActive(false);
            }
        }
        void InstansiatePlayerPanel()
        {
            var panel = Instantiate(PlayerPanelTemplate, PlayerListContent).transform;
            PlayerPanel playerPanel = panel.GetComponent<PlayerPanel>();
            playerPanel.teleporter = teleporter;
            playerPanel.watchCamera = watchCamera;
            Utils.ArrayUtils.Append(ref panels, playerPanel);
        }
        public void ClearSelectedPlayers()
        {
            foreach (var panel in panels)
            {
                panel.ClearSelectedPlayer();
            }
        }
    }
}
