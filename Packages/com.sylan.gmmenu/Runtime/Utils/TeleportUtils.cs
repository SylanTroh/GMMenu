using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu.Utils
{
    public static class TeleportUtils
    {
        /// <summary>Handles quaternions where their forward vector is pointing straight up or down.</summary>
        /// <returns>A quaternion purely rotating around the Y axis. If the given <paramref name="rotation"/>
        /// was upside down, the result does not reflect as such. The "up" of the resulting rotation is always
        /// equal to <see cref="Vector3.up"/>.</returns>
        public static Quaternion ProjectOntoYPlane(Quaternion rotation)
        {
            Vector3 projectedForward = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up);
            return projectedForward == Vector3.zero // Facing straight up or down?
                ? Quaternion.LookRotation(rotation * Vector3.down) // Imagine a head facing staring up. The chin is down.
                : Quaternion.LookRotation(projectedForward.normalized);
        }

        /// <summary>
        /// <para><see cref="VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint"/> is bugged,
        /// see this canny: https://feedback.vrchat.com/udon/p/teleporting-the-player-to-the-same-rotation-they-were-already-sometimes-introduc</para>
        /// <para><see cref="VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint"/> is bugged,
        /// see this canny: https://feedback.vrchat.com/udon/p/teleport-sometimes-gets-stuck-on-geometry</para>
        /// <para>The latter makes it unusable for no-clip. Even doing an "align player" teleport beforehand
        /// to theoretically move the player into the collider, followed by an "align room" teleport does not
        /// work, it has the exact same issue.</para>
        /// <para>Not just no-clip though, this can happen when teleporting through colliders even in "normal"
        /// use cases of the teleport function.</para>
        /// <para>Thus the only option is to use "align player" teleports, and deal with the former mentioned
        /// bug.</para>
        /// <para>Note that when not dealing with the bug, so just using a single normal "align player"
        /// teleport call for no-clip causes players in VR to spin when looking up/down and then looking left
        /// or right.</para>
        /// <para>Using <see cref="VRCPlayerApi.GetRotation"/> before and after a teleport call to then
        /// calculate a rotation difference which gets applied to a consecutive teleport call mostly works
        /// around this issue. Works for desktop, mostly works in half body though it sometimes causes
        /// intentional head movement to get undone, however in full body it causes nearly all horizontal
        /// head rotation to get cancelled out, such that looking left or right while using no-clip keeps the
        /// view point pointed straight as though the head did not move at all.</para>
        /// <para>Doing almost the exact same thing, however instead of using
        /// <see cref="VRCPlayerApi.GetRotation"/>, getting the head tracking data and using that to calculate
        /// a rotation difference that was induced by the teleport mostly solves this full body issue. There
        /// might just be a few jitters left or right that can be noticed when looking up or down while in
        /// full body VR.</para>
        /// <para>Turning the head upside down can cause the player to get turned around 180 degrees due to
        /// the projection of the head rotation onto the Y plane. Does not seem consistent, but luckily hardly
        /// anybody does that.</para>
        /// <para>And lastly another oddity that can happen is when turning the head in a circle rather
        /// quickly, some of that rotation can also get cancelled out, which is presumably jarring. It seems
        /// rare however.</para>
        /// </summary>
        public static void TeleportAndRetainHeadRotation(VRCPlayerApi player, Vector3 teleportPosition, bool lerpOnRemote)
        {
            // Get head rotation => teleport => get head rotation again => calculate offset induced by teleport => corrective teleport.
            Quaternion playerRotation = player.GetRotation();
            Quaternion preHeadRotation = ProjectOntoYPlane(player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation);
            player.TeleportTo(teleportPosition, playerRotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, lerpOnRemote);
            Quaternion postHeadRotation = ProjectOntoYPlane(player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation);
            Quaternion headRotationOffset = Quaternion.Inverse(postHeadRotation) * preHeadRotation;
            player.TeleportTo(teleportPosition, headRotationOffset * playerRotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, lerpOnRemote);
        }
    }
}
