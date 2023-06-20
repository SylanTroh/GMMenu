
using Sylan.GMMenu;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class GMMenuPart : UdonSharpBehaviour
{
    [HideInInspector,SerializeField] public GMMenu gmMenu;
}
