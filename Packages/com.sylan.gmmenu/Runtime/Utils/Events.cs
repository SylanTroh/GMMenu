using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu.Utils
{
    public static class Events
    {
        public static void SendEvent(string eventName, UdonSharpBehaviour[] Listeners)
        {
            Debug.Log("[GMMenu]: "+eventName+"Event");
            foreach (UdonSharpBehaviour b in Listeners)
            {
                if(Utilities.IsValid(b)) b.SendCustomEvent(eventName);
            }
        }
    }
}
