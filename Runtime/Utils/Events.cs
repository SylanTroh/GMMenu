
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu.Utils
{
    public class Events : UdonSharpBehaviour
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
