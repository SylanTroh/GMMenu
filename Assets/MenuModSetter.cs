
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MenuModSetter : UdonSharpBehaviour
{
    private void Start()
    {
        MovementSpeedMedium();
        JumpHeightMedium();
    }
    public void MovementSpeedSlow()
    {
        Networking.LocalPlayer.SetRunSpeed(4f);
        Networking.LocalPlayer.SetStrafeSpeed(2f);
        Networking.LocalPlayer.SetWalkSpeed(2f);
    }
    public void MovementSpeedMedium()
    {
        Networking.LocalPlayer.SetRunSpeed(6f);
        Networking.LocalPlayer.SetStrafeSpeed(2.5f);
        Networking.LocalPlayer.SetWalkSpeed(2.5f);
    }
    public void MovementSpeedFast()
    {
        Networking.LocalPlayer.SetRunSpeed(8f);
        Networking.LocalPlayer.SetStrafeSpeed(3f);
        Networking.LocalPlayer.SetWalkSpeed(3f);
    }
    public void JumpHeightLow()
    {
        Networking.LocalPlayer.SetJumpImpulse(3f);
    }
    public void JumpHeightMedium()
    {
        Networking.LocalPlayer.SetJumpImpulse(4.5f);
    }
    public void JumpHeightHigh()
    {
        Networking.LocalPlayer.SetJumpImpulse(6f);
    }
}
