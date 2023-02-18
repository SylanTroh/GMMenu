
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerMover : UdonSharpBehaviour
    {
        VRCPlayerApi localPlayer;
        [FieldChangeCallback(nameof(noclip))]
        bool _noclip = false;

        [NonSerialized] public bool noclipOnDoubleJump = false;
        bool jumpPressed = false;

        public float speedMagnitude = 8.0f;
        float speedLongitudinal= 0f;
        float speedHorizontal = 0f;
        float speedVertical = 0f;


        Quaternion headVector;
        Vector3 offset;

        public Transform station;
        BoxCollider boxCollider;
        public bool performanceMode = true;
        void Start()
        {
            boxCollider = station.GetComponent<BoxCollider>();
            localPlayer = Networking.LocalPlayer;
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            if (!noclipOnDoubleJump) return;
            if (!value) return;

            if (jumpPressed)
            {
                ToggleNoclip();
            }
            else
            {
                jumpPressed = true;
                boxCollider.enabled = false;
                SendCustomEventDelayedSeconds("ResetJumpPressed", 0.5f);
            }
        }
        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            speedLongitudinal = value;
        }
        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            speedHorizontal = value;
        }
        //public override void InputLookVertical(float value, UdonInputEventArgs args)
        //{
        //    if (localPlayer.IsUserInVR()) speedVertical = value;
        //}
        public void ResetJumpPressed()
        {
            jumpPressed = false;
            boxCollider.enabled = noclip;
        }
        public bool noclip
        {
            set
            {
                _noclip = value;
                if (value)
                {
                    station.SetParent(null,false);
                    station.position = localPlayer.GetPosition();
                    boxCollider.enabled = true;
                    localPlayer.SetGravityStrength(0);
                }
                else
                {
                    boxCollider.enabled = false;
                    localPlayer.SetGravityStrength(1);
                    localPlayer.SetVelocity(Vector3.zero);
                    station.SetParent(transform,false);
                    station.localPosition = Vector3.zero;
                    station.rotation = Quaternion.identity;
                }
            }
            get => _noclip;
        }
        private void Update()
        {
            UpdateStationPosition();
        }
        void UpdateStationPosition()
        {
            if (!noclip) return;
            //Don't teleport while staying still. 
            if (performanceMode && speedHorizontal == 0 && speedVertical == 0 && speedLongitudinal == 0)
            {
                localPlayer.SetVelocity(Vector3.zero);
                return;
            }
            headVector = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            offset = speedLongitudinal * (headVector * Vector3.forward);
            offset += speedHorizontal * (headVector * Vector3.right);
            offset += speedVertical * (headVector * Vector3.up);
            offset *= speedMagnitude*Time.deltaTime;
            station.position += offset;
            localPlayer.TeleportTo(station.position, localPlayer.GetRotation());
        }
        public void ToggleNoclip()
        {
            noclip = !noclip;
        }
    }
}
