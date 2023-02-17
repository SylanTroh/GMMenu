
using UdonSharp;
using UnityEngine;

namespace Sylan.GMMenu.Utils
{
    public class Modules : UdonSharpBehaviour
    {
        public static PlayerPermissions PlayerPermissions(Transform self)
        {
            return self.root.Find("PlayerPermissions").GetComponent<PlayerPermissions>();
        }
        public static GMMenuToggle GMMenuToggle(Transform self)
        {
            return self.root.Find("GMMenuToggle").GetComponent<GMMenuToggle>();
        }
        public static MessageSyncManager MessageSyncManager(Transform self)
        {
            return self.root.Find("MessageSyncManager").GetComponent<MessageSyncManager>();
        }
        public static MessageData[] MessageData(Transform self)
        {
            return self.root.Find("MessageSyncManager").GetComponentsInChildren<MessageData>();
        }
        public static Teleporter Teleporter(Transform self)
        {
            return self.root.Find("Teleporter").GetComponent<Teleporter>();
        }
        public static WatchCamera WatchCamera(Transform self)
        {
            return self.root.Find("WatchCamera").GetComponent<WatchCamera>();
        }
        public static PlayerMover PlayerMover(Transform self)
        {
            return self.root.Find("PlayerMover").GetComponent<PlayerMover>();
        }
    }
}
