using UdonSharp;
using UnityEngine;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GMMenuPart : UdonSharpBehaviour
    {
        [HideInInspector, SerializeField] public GMMenu gmMenu;
        public const string GMMenuPropertyName = nameof(gmMenu);
    }
}
