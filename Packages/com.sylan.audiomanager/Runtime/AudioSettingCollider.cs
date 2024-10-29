using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Sylan.AudioManager
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AudioSettingCollider : UdonSharpBehaviour
    {
        [Header("AudioSetting ID. Used for debugging. Needs to be non-empty and unique.")]
        public string settingID = string.Empty;

        [Header("Lower number means higher priority", order = 0)]
        [Space(-10, order = 1)]
        [Header("Audiozones have priority 1000", order = 2)]
        public int priority = AudioSettingManager.DEFAULT_PRIORITY;

        public float voiceGain = AudioSettingManager.DEFAULT_VOICE_GAIN;
        public float voiceNear = AudioSettingManager.DEFAULT_VOICE_RANGE_NEAR;
        public float voiceFar = AudioSettingManager.DEFAULT_VOICE_RANGE_FAR;
        public float volumetricRadius = AudioSettingManager.DEFAULT_VOICE_VOLUMETRIC_RADIUS;
        public bool lowpassFilter = AudioSettingManager.DEFAULT_VOICE_LOWPASS;

        [HideInInspector, SerializeField] private AudioSettingManager _AudioSettingManager;
        public const string AudioSettingManagerPropertyName = nameof(_AudioSettingManager);

        DataList audioSetting;

        private void Start()
        {
            if (settingID == string.Empty) Destroy(this);
            DataToken[] tokens = {voiceGain, voiceNear, voiceFar, volumetricRadius, lowpassFilter };
            audioSetting = new DataList(tokens);
        }
        public override void OnPlayerTriggerEnter(VRCPlayerApi triggeringPlayer)
        {
            if (triggeringPlayer == Networking.LocalPlayer) return;
            Debug.Log("[AudioManager] Apply Audio Setting " + settingID + " to " + triggeringPlayer.displayName + "-" + triggeringPlayer.playerId.ToString());

            triggeringPlayer.AddAudioSetting(_AudioSettingManager, settingID, priority, audioSetting);

            _AudioSettingManager.UpdateAudioSettings(triggeringPlayer);
        }
        public override void OnPlayerTriggerExit(VRCPlayerApi triggeringPlayer)
        {
            if (triggeringPlayer == Networking.LocalPlayer) return;
            Debug.Log("[AudioManager] Apply Audio Setting " + settingID + " to " + triggeringPlayer.displayName + "-" + triggeringPlayer.playerId.ToString());

            triggeringPlayer.RemoveAudioSetting(_AudioSettingManager, settingID);

            _AudioSettingManager.UpdateAudioSettings(triggeringPlayer);
        }
    }
}
