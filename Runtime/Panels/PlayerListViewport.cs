
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerListViewport : UdonSharpBehaviour
    {
        public GameObject PlayerPanelTemplate;
        public Transform PlayerListContent;
        [NotNull] GMMenuToggle menuToggle;
        [NotNull] Teleporter teleporter;
        [NotNull] WatchCamera watchCamera;

        VRCPlayerApi[] players = new VRCPlayerApi[0];
        PlayerPanel[] panels = new PlayerPanel[0];

        bool firstUpdate = false;

        private void Start()
        {
            menuToggle = Utils.Modules.GMMenuToggle(transform);
            teleporter = Utils.Modules.Teleporter(transform);
            watchCamera = Utils.Modules.WatchCamera(transform);

            SendCustomEventDelayedSeconds("EnableMenuToggleListener", 0.0f);
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);
            players = SortPlayers();
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);
            players = SortPlayers();
        }
        //Event Listeners
        public void EnableMenuToggleListener()
        {
            menuToggle.AddListener(this);
        }
        // Events
        public void OnMenuToggleOn()
        {
            if (!firstUpdate)
            {
                players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
                VRCPlayerApi.GetPlayers(players);
                players = SortPlayers();
                firstUpdate = true;
            }
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
            if (menuToggle.MenuState()) SendCustomEventDelayedSeconds("UpdateViewportContinuous", 5.0f);
        }

        private void ClearViewport()
        {
            foreach (Transform a in PlayerListContent)
            {
                Destroy(a.gameObject);
            }
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
        int ComparePlayerName(VRCPlayerApi player1, VRCPlayerApi player2)
        {
            return string.Compare(player1.displayName,player2.displayName);
        }
        public VRCPlayerApi[] SortPlayers()
        {
            //Terrible code to sort messages, because UDON doesn't support built in C# stuff
            
            QuickSortPlayers(players, 0, players.Length - 1);
            return players;
        }
        private void QuickSortPlayers(VRCPlayerApi[] arr, int start, int end)
        {
            if (!Utilities.IsValid(arr)) return;

            int i = 0;
            if (start < end)
            {
                i = PartitionPlayers(arr, start, end);

                QuickSortPlayers(arr, start, i - 1);
                QuickSortPlayers(arr, i + 1, end);
            }
        }
        private int PartitionPlayers(VRCPlayerApi[] arr, int start, int end)
        {
            VRCPlayerApi temp;
            VRCPlayerApi p = arr[end];
            int i = start - 1;

            for (int j = start; j <= end - 1; j++)
            {
                if (ComparePlayerName(arr[j], p) == -1)
                {
                    i++;
                    temp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = temp;
                }
            }

            temp = arr[i + 1];
            arr[i + 1] = arr[end];
            arr[end] = temp;
            return i + 1;
        }
    }
}
