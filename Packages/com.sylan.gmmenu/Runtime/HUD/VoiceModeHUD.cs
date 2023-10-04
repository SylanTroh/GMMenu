using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VoiceModeHUD : GMMenuPart
    {
        private VoiceModeManager voiceModeManager;

        public GameObject hudWhisper;
        public GameObject hudYell;
        public GameObject hudBroadcast;

        public GameObject hud;
        public Transform hudIcons;

        void Start()
        {
            voiceModeManager = gmMenu.VoiceModeManager;

            voiceModeManager.AddListener(this);
        }
        void UpdateHUD()
        {
            DisableHUD();
            var voiceMode = voiceModeManager.GetLocalSettingValue();
            switch (voiceMode)
            {
                case VoiceMode.SETTING_BROADCAST:
                    hudBroadcast.SetActive(true);
                    hud.SetActive(true);
                    return;
                case VoiceMode.SETTING_WHISPER:
                    hudWhisper.SetActive(true);
                    hud.SetActive(true);
                    return;
                case VoiceMode.SETTING_YELL:
                    hudYell.SetActive(true);
                    hud.SetActive(true);
                    return;
                default:
                    return;
            }
        }
        void DisableHUD()
        {
            hud.SetActive(false);
            foreach (Transform t in hudIcons)
            {
                t.gameObject.SetActive(false);
            }
        }
        public void OnVoiceModeChanged()
        {
            UpdateHUD();
        }
    }
}
