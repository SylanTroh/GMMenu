/*
Teleports the player using GetPosition and GetRotation as the basis for
the interface, but does all the math to teleport by TrackingData origin
under the hood correctly. This gives it a solid, reliable teleport suitable
for seamless teleportation use cases, while giving you an interface that
still has an easy to use pivot point and forward axis.

Copyright (c) 2023 @Phasedragon on GitHub
Additional help by @Nestorboy

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu.Utils
{
    public static class TeleportUtils
    {
        /// <summary>
        /// <para>See: https://gist.github.com/Phasedragon/5b76edfb8723b6bc4a49cd43adde5d3d</para>
        /// </summary>
        /// <param name="teleportRot">Gets projected onto the Y plane.</param>
        public static void RoomAlignedTeleport(VRCPlayerApi player, Vector3 teleportPos, Quaternion teleportRot, bool lerpOnRemote)
        {
#if UNITY_EDITOR
            // Skip process and Exit early for ClientSim since there is no play space to orient.
            player.TeleportTo(teleportPos, teleportRot);
#else
            // This is absolutely not how you are supposed to use euler angles. Converting a quaternion to
            // euler angles, taking some component of that and then converting that back to a quaternion is
            // asking for trouble, and that is exactly what is happening here. However through some miracle
            // this case actually behaves correctly, and I (JanSharp) believe that it's related to the order
            // that the euler axis get processed by Unity. Supposedly it is YXZ around local axis and ZXY
            // around world axis. So maybe these functions here use YXZ and that's why it works.
            teleportRot = Quaternion.Euler(0, teleportRot.eulerAngles.y, 0);

            // Get player pos/rot
            Vector3 playerPos = player.GetPosition();
            Quaternion invPlayerRot = Quaternion.Inverse(player.GetRotation());

            // Get origin pos/rot
            VRCPlayerApi.TrackingData origin = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);

            // Subtract player from origin in order to get the offset from the player to the origin
            // offset = origin - player
            Vector3 offsetPos = origin.position - playerPos;
            Quaternion offsetRot = invPlayerRot * origin.rotation;

            // Add the offset onto the destination in order to construct a pos/rot of where your origin would be in order to put the player at the destination
            // target = destination + offset
            player.TeleportTo(
                teleportPos + teleportRot * invPlayerRot * offsetPos,
                teleportRot * offsetRot,
                VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint,
                lerpOnRemote);
#endif
        }
    }
}
