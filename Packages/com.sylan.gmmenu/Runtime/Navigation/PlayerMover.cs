
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerMover : GMMenuPart
    {
        VRCPlayerApi localPlayer;
        [FieldChangeCallback(nameof(noclip))]
        bool _noclip = false;

        [NonSerialized] public bool noclipOnDoubleJump = false;
        bool jumpPressed = false;

        public float speedMagnitude = 8.0f;
        float speedLongitudinal= 0.0f;
        float speedHorizontal = 0.0f;
        float speedVertical = 0.0f;


        Quaternion headVector;
        Vector3 offset;

        public Transform station;
        BoxCollider boxCollider;

        GMMenuToggle menuToggle;
        void Start()
        {
            menuToggle = gmMenu.GMMenuToggle;
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
                SendCustomEventDelayedSeconds(nameof(ResetJumpPressed), 0.5f);
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
        void Update()
        {
            UpdateStationPosition();
        }
        public void UpdateStationPosition()
        {
            if (!noclip) return;
            //Don't teleport while staying still. 
            bool isStill = (speedHorizontal == 0 && speedVertical == 0 && speedLongitudinal == 0);
            if (!isStill)
            {
                headVector = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                offset = speedLongitudinal * (headVector * Vector3.forward);
                offset += speedHorizontal * (headVector * Vector3.right);
                offset += speedVertical * (headVector * Vector3.up);
                offset *= speedMagnitude * Time.deltaTime;
                station.position += offset;
                //localPlayer.TeleportTo(
                TeleportPlayer(
                    localPlayer,
                    station.position, 
                    localPlayer.GetRotation(),
                    true
                    );
                return;
            }
            if (!menuToggle.MenuState())
            {
                //localPlayer.TeleportTo(
                TeleportPlayer(
                    localPlayer,
                    station.position,
                    localPlayer.GetRotation(),
                    true
                    );
                return;
            }
            localPlayer.SetVelocity(Vector3.zero);
            return;

        }
        public void TeleportPlayer(VRCPlayerApi player, Vector3 teleportPos, Quaternion teleportRotation, bool lerpOnRemote)
        {
            //This function teleports the player,
            //calculates the difference between the expected rotation and the actual rotation resulting from the teleport.
            //then teleports the player again to compensate for the error.
            //This is temporary patch for a problem where noclip causes one to spin rapidly, until I can figure out what is causing this to happen
            player.TeleportTo(
                teleportPos,
                teleportRotation,
                VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint,
                lerpOnRemote
                );
            player.TeleportTo(teleportPos,
                Quaternion.Inverse(localPlayer.GetRotation()) * teleportRotation * teleportRotation, 
                VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint,
                lerpOnRemote);
        }
        public void ToggleNoclip()
        {
            noclip = !noclip;
        }
        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            station.position = player.GetPosition();
        }
    }
}
