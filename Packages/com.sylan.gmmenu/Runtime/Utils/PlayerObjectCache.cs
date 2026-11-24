using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Sylan.GMMenu.Utils
{
    public static class PlayerObjectCache
    {
        public static void RegisterPlayerObject<T>(DataDictionary cache, VRCPlayerApi player, T playerObject)
        {
            cache.SetValue(player.playerId, new DataToken(playerObject));
        }

        public static bool DeregisterPlayerObject<T>(DataDictionary cache, VRCPlayerApi player)
        {
            var success = cache.Remove(player.playerId);
            return success;
        }

        public static T GetPlayerObject<T>(DataDictionary cache, VRCPlayerApi player)
        {
            if (cache.TryGetValue(player.playerId, TokenType.Reference, out DataToken value))
            {
                var playerObject = (T)value.Reference;
                return playerObject;
            }
            return default(T);
        }
    }
}
