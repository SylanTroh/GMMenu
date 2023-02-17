
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu.Utils
{
    public class AvatarUtils : UdonSharpBehaviour
    {
        public static float AvatarHeight(VRCPlayerApi player)
        {
            var avatarHeight = 0.0f;
            var head = player.GetBonePosition(HumanBodyBones.Head);
            var neck = player.GetBonePosition(HumanBodyBones.Neck);
            var hips = player.GetBonePosition(HumanBodyBones.Hips);
            var upperleg = player.GetBonePosition(HumanBodyBones.RightUpperLeg);
            var lowerleg = player.GetBonePosition(HumanBodyBones.RightLowerLeg);
            var foot = player.GetBonePosition(HumanBodyBones.RightFoot);
            avatarHeight += BoneDistance(head, neck);
            avatarHeight += BoneDistance(neck, hips);
            avatarHeight += BoneDistance(hips, upperleg);
            avatarHeight += BoneDistance(upperleg, lowerleg);
            avatarHeight += BoneDistance(lowerleg, foot);
            return avatarHeight;
        }
        public static float BoneDistance(Vector3 bone1, Vector3 bone2)
        {
            return (bone1 - bone2).magnitude;
        }
    }
}