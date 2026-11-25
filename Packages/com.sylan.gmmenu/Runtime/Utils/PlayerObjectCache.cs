using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Sylan.GMMenu.Utils
{
    public static class PlayerObjectCache
    {
        public static void RegisterPlayerObject<T>(DataDictionary cache, VRCPlayerApi player, T playerObject)
        {
            if(!Utilities.IsValid(player)) return;
            cache.SetValue(player.playerId, new DataToken(playerObject));
        }

        public static bool DeregisterPlayerObject(DataDictionary cache, VRCPlayerApi player)
        {
            var success = cache.Remove(player.playerId);
            return success;
        }

        public static bool GetPlayerObject<T>(DataDictionary cache, VRCPlayerApi player, out T playerObject)
        {
            if (cache.TryGetValue(player.playerId, TokenType.Reference, out DataToken value))
            {
                playerObject = (T)value.Reference;
                return true;
            }
            playerObject = default(T);
            return false;
        }
    }
}
