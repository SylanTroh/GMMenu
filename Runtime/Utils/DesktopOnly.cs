using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu.Utils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DesktopOnly : UdonSharpBehaviour
    {
        private void Start()
        {
            if (Networking.LocalPlayer.IsUserInVR()) gameObject.SetActive(false);
        }

        public static void Toggle(GameObject gameObject)
        {
            if (Networking.LocalPlayer.IsUserInVR()) gameObject.SetActive(false);
        }
    }
}