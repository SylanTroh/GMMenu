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
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VoiceModeManager : GMMenuPart
    {
        [FormerlySerializedAs("VoiceModeButtons")]
        [SerializeField] private GameObject voiceModeButtons;

#if AUDIOMANAGER
        [HideInInspector,SerializeField] public AudioSettingManager audioSettingManager;
        public const string AudioSettingManagerPropertyName = nameof(audioSettingManager);
#endif

        private UdonSharpBehaviour[] _voiceModeChangedEventListeners = new UdonSharpBehaviour[0];

        private VoiceMode _localVoiceMode;
        private readonly DataDictionary _voiceModeCache = new DataDictionary();

        [Header("Lower number means higher priority", order = 0)]
        [Space(-10, order = 1)]
        [Header("Audiozones have priority 1000", order = 2)]
        public int priority = 2000;
        public float whisperGain = 15f;
        public float whisperFarRange = 5f;
        public float whisperNearRange = 0.5f;
        public float yellGain = 15f;
        public float yellFarRange = 40f;

        private void DeactivateSelf()
        {
            Destroy(voiceModeButtons);
            gameObject.SetActive(false);
        }

#if !AUDIOMANAGER
        void Start()
        {
            DeactivateSelf();
        }
#else
        void Start()
        {
            if (audioSettingManager == null)
            {
                DeactivateSelf();
                return;
            }
        }

        public void RegisterVoiceMode(VRCPlayerApi player, VoiceMode voiceMode)
        {
            if (Networking.IsOwner(Networking.LocalPlayer, voiceMode.gameObject)) _localVoiceMode = voiceMode;
            Utils.PlayerObjectCache.RegisterPlayerObject(_voiceModeCache,player,voiceMode);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            Utils.PlayerObjectCache.DeregisterPlayerObject(_voiceModeCache,player);
        }

        public int GetLocalVoiceModeSetting()
        {
            if (!Utilities.IsValid(_localVoiceMode)) return VoiceMode.SETTING_NULL;
            return _localVoiceMode.GetVoiceMode();
        }
        public int GetPlayerVoiceModeSetting(VRCPlayerApi player)
        {
            if (Utils.PlayerObjectCache.GetPlayerObject(_voiceModeCache, player, out VoiceMode v))
            {
                return v.GetVoiceMode();
            }
            return VoiceMode.SETTING_NULL;
        }

        [System.Obsolete("Use GetLocalVoiceModeSetting() instead")]
        public int GetLocalSettingValue() => GetLocalVoiceModeSetting();
        [System.Obsolete("Use GetPlayerVoiceModeSetting() instead")]
        public int GetPlayerSettingValue(VRCPlayerApi player) => GetPlayerVoiceModeSetting(player);

        private void SetVoiceMode(int voiceModeSetting)
        {
            if (!Utilities.IsValid(_localVoiceMode)) return;
            _localVoiceMode.SetVoiceMode(voiceModeSetting);
            _localVoiceMode.RequestSerialization();
            SendVoiceModeChangedEvent();
        }
        public void SetTalk() => SetVoiceMode(VoiceMode.SETTING_TALK);
        public void SetWhisper() => SetVoiceMode(VoiceMode.SETTING_WHISPER);
        public void SetYell() => SetVoiceMode(VoiceMode.SETTING_YELL);
        public void SetBroadcast() => SetVoiceMode(VoiceMode.SETTING_BROADCAST);


        //Events
        public void SendVoiceModeChangedEvent()
        {
            Utils.Events.SendEvent("OnVoiceModeChanged", _voiceModeChangedEventListeners);
        }
        public void AddListener(UdonSharpBehaviour b)
        {
            Utils.ArrayUtils.Append(ref _voiceModeChangedEventListeners, b);
        }
#endif
    }
}
