
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.AudioManager
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("Scrips/Audio Zone Collider")]
    public class AudioZoneCollider : UdonSharpBehaviour
    {
        [Header("Primary AudioZone ID")]
        public string zoneID = string.Empty;

        [Header("Additional AudioZones. Useful for transitions.", order = 0)]
        [Space(-10, order = 1)]
        [Header("To match players who are not in a zone, set an empty string.", order = 2)]
        public string[] transitionZoneIDs;

        [HideInInspector, SerializeField] private AudioZoneManager _AudioZoneManager;
        public const string AudioZoneManagerPropertyName = nameof(_AudioZoneManager);

        private bool hasAudioSettingComponent = false;

        public bool isNegativeZone = false;

        private void Start()
        {
            hasAudioSettingComponent = (GetComponent<AudioSettingCollider>() != null);
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi triggeringPlayer)
        {
            Debug.Log("[AudioManager] " + triggeringPlayer.displayName + "-" + triggeringPlayer.playerId.ToString() + " Entering Zone " + zoneID + "-" + gameObject.GetInstanceID());

            triggeringPlayer.EnterAudioZone(_AudioZoneManager, zoneID, isNegativeZone);
            foreach (string id in transitionZoneIDs)
            {
                triggeringPlayer.EnterAudioZone(_AudioZoneManager, id, isNegativeZone);
            }

            _AudioZoneManager.UpdateAudioZoneSetting(triggeringPlayer, hasAudioSettingComponent);
        }
        public override void OnPlayerTriggerExit(VRCPlayerApi triggeringPlayer)
        {
            Debug.Log("[AudioManager] " + triggeringPlayer.displayName + "-" + triggeringPlayer.playerId.ToString() + " Exiting Zone " + zoneID + "-" + gameObject.GetInstanceID());

            triggeringPlayer.ExitAudioZone(_AudioZoneManager, zoneID, isNegativeZone);
            foreach (string id in transitionZoneIDs)
            {
                triggeringPlayer.ExitAudioZone(_AudioZoneManager, id, isNegativeZone);
            }

            _AudioZoneManager.UpdateAudioZoneSetting(triggeringPlayer, hasAudioSettingComponent);
        }
    }
}
