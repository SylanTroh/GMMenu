#if SYLAN_AUDIOMANAGER_VERSION || (COMPILER_UDONSHARP && SYLAN_AUDIOMANAGER)
// The version define SYLAN_AUDIOMANAGER_VERSION defined on the assembly definition is sufficient for
// regular compilation, it appears to be how optional compilation should be done for unity packages,
// including when having optional package dependencies.
//
// The define SYLAN_AUDIOMANAGER is set in the project settings through editor scripting by the audio manager
// package. That is specifically required for UdonSharp because that does not have support for version defines.
// However since SYLAN_AUDIOMANAGER is in the global list of defines, it is going to remain there when the
// audio manager gets removed, therefore we cannot just use SYLAN_AUDIOMANAGER to conditionally compile code
// that uses the audio manager as that would fail compilation when the package gets removed.
//
// There is editor scripting in the editor assembly in the GMMenu which removes the SYLAN_AUDIOMANAGER define
// to make sure the UdonSharp compilation result matches the compiled UdonSharpBehaviour.
//
// Since the editor assembly is separate from the runtime assembly we could just not care about these compile
// errors, so long as just the editor assembly compiles successfully so it can unset the define. However if
// the audio manager package was removed while unity was closed, opening unity would prompt that there are
// compile errors and if somebody choses to enter safe mode, the editor script would not run and the compile
// errors would not be resolved automatically.
//
// All of that is the reason why SYLAN_AUDIOMANAGER is only used if COMPILER_UDONSHARP is also set.
#define AUDIOMANAGER
#endif

#if AUDIOMANAGER
using Sylan.AudioManager;
#endif
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VoiceModeManager : GMMenuPart
    {
        [SerializeField] GameObject VoiceModeButtons;

#if AUDIOMANAGER
        [HideInInspector,SerializeField] public AudioSettingManager audioSettingManager;
        public const string AudioSettingManagerPropertyName = nameof(audioSettingManager);
#endif

        private UdonSharpBehaviour[] VoiceModeChangedEventListeners = new UdonSharpBehaviour[0];

        [HideInInspector] public VoiceMode localVoiceMode;
        private VoiceMode[] audioSettings;
        private DataDictionary audioSettingsIndex = new DataDictionary();

        [Header("Lower number means higher priority", order = 0)]
        [Space(-10, order = 1)]
        [Header("Audiozones have priority 1000", order = 2)]
        public int priority = 2000;
        public float whisperGain = 15f;
        public float whisperFarRange = 5f;
        public float whisperNearRange = 0.5f;
        public float yellGain = 15f;
        public float yellFarRange = 40f;

        private void DestroySelf()
        {
            Destroy(VoiceModeButtons);
            Destroy(gameObject);
        }

#if !AUDIOMANAGER
        void Start()
        {
            DestroySelf();
        }
#else
        void Start()
        {
            if (audioSettingManager == null)
            {
                DestroySelf();
                audioSettings = new VoiceMode[0]; // Prevent potential errors since Destroy is not instant.
                return;
            }
            audioSettings = GetComponentsInChildren<VoiceMode>();
            foreach (VoiceMode voiceMode in audioSettings)
            {
                voiceMode.priority = priority;
            }
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            SetAudioSettingOwnership(player);
        }
        private void SetAudioSettingOwnership(VRCPlayerApi player)
        {
            for (int i = 0; i < audioSettings.Length; i++)
            {
                VoiceMode voiceMode = audioSettings[i];
                if (voiceMode.owner != null) continue;

                voiceMode.owner = player;
                voiceMode.RequestSerialization();
                audioSettingsIndex.SetValue(player.playerId, i);
                return;
            }
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            RevokeAudioSettingOwnership(player);
        }
        private void RevokeAudioSettingOwnership(VRCPlayerApi player)
        {
            for (int i = 0; i < audioSettings.Length; i++)
            {
                VoiceMode voiceMode = audioSettings[i];
                if (voiceMode.owner != player) continue;

                voiceMode.owner = null;
                voiceMode.RequestSerialization();
                audioSettingsIndex.Remove(player.playerId);
            }
        }
        public VoiceMode GetLocalSetting()
        {
            return localVoiceMode;
        }
        public int GetLocalSettingValue()
        {
            if (!Utilities.IsValid(localVoiceMode)) return VoiceMode.SETTING_NULL;
            return localVoiceMode.setting;
        }
        public int GetPlayerSettingValue(VRCPlayerApi player)
        {
            if (audioSettingsIndex.TryGetValue(player.playerId, TokenType.Int, out DataToken value))
            {
                int i = value.Int;
                return audioSettings[i].setting;
            }
            return VoiceMode.SETTING_NULL;
        }
        public void SetTalk()
        {
            if (!Utilities.IsValid(localVoiceMode)) return;
            if (!Networking.IsOwner(localVoiceMode.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, localVoiceMode.gameObject);
            localVoiceMode.setting = VoiceMode.SETTING_TALK;
            localVoiceMode.RequestSerialization();
            SendVoiceModeChangedEvent();
        }
        public void SetWhisper()
        {
            if (!Utilities.IsValid(localVoiceMode)) return;
            if (!Networking.IsOwner(localVoiceMode.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, localVoiceMode.gameObject);
            localVoiceMode.setting = VoiceMode.SETTING_WHISPER;
            localVoiceMode.RequestSerialization();
            SendVoiceModeChangedEvent();
        }
        public void SetYell()
        {
            if (!Utilities.IsValid(localVoiceMode)) return;
            if (!Networking.IsOwner(localVoiceMode.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, localVoiceMode.gameObject);
            localVoiceMode.setting = VoiceMode.SETTING_YELL;
            localVoiceMode.RequestSerialization();
            SendVoiceModeChangedEvent();
        }
        public void SetBroadcast()
        {
            if (!Utilities.IsValid(localVoiceMode)) return;
            if (!Networking.IsOwner(localVoiceMode.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, localVoiceMode.gameObject);
            localVoiceMode.setting = VoiceMode.SETTING_BROADCAST;
            localVoiceMode.RequestSerialization();
            SendVoiceModeChangedEvent();
        }
        //Events
        public void SendVoiceModeChangedEvent()
        {
            Utils.Events.SendEvent("OnVoiceModeChanged", VoiceModeChangedEventListeners);
        }
        public void AddListener(UdonSharpBehaviour b)
        {
            Utils.ArrayUtils.Append(ref VoiceModeChangedEventListeners, b);
        }
#endif
    }
}
