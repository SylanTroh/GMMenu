
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SummonButton : GMMenuPart
    {
        [SerializeField] private GameObject summonButton;
        [SerializeField] private Text summonText;
        private bool confirmSummon = false;

        void OnEnable()
        {
            summonText.text = "Summon";
            confirmSummon = false;
        }
        public void SummonPlayer()
        {
            var players = gmMenu.PlayerSelector.GetSelectedPlayers();
            foreach (VRCPlayerApi p in players)
            {
                if (!Utilities.IsValid(p)) continue;
                gmMenu.Teleporter.SummonPlayer(p);
            }
            UnConfirmSummon();
        }
        public void ConfirmSummon()
        {
            if (!confirmSummon)
            {
                summonText.text = "Confirm";
                confirmSummon = true;
                SendCustomEventDelayedSeconds(nameof(UnConfirmSummon), 5.0f);
            }
            else
            {
                summonText.text = "Summon";
                SummonPlayer();
            }
        }
        public void UnConfirmSummon()
        {
            confirmSummon = false;
            summonText.text = "Summon";
        }
    }
}
