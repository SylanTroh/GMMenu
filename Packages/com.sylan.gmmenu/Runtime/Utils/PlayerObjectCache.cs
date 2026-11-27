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
            if (!Utilities.IsValid(player)) return false;
            return cache.Remove(player.playerId);
        }

        public static bool GetPlayerObject<T>(DataDictionary cache, VRCPlayerApi player, out T playerObject)
        {
            playerObject = default(T);
            if (!Utilities.IsValid(player)) return false;
            if (!cache.TryGetValue(player.playerId, TokenType.Reference, out DataToken value)) return false;
            playerObject = (T)value.Reference;
            return true;
        }
    }
}
