
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
            float avatarHeightFromBones = AvatarHeightFromBones(player);
            float avatarEyeHeightAsMeters = player.GetAvatarEyeHeightAsMeters();

            if (avatarHeightFromBones > 0.1f)
            {
                //If the avatar is humanoid, eye height is correct
                return avatarEyeHeightAsMeters;
            }
            
            //If the avatar is non-humanoid, return a constant
            return 1.75f;
        }
        public static float AvatarHeightFromBones(VRCPlayerApi player)
        {
            //Old Method
            //Doesn't work for models without bones
            //Should perhaps add a better function to check for bones
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
        
        public static float PlayerHeightFromTracking()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            Vector3 headPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            Vector3 playerPosition = localPlayer.GetPosition();
            float height = headPosition.y - playerPosition.y;
            return height;
        }
    }
}