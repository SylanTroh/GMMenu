using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class GMMenuPart : UdonSharpBehaviour
{
    // Using 'using Sylan.GMMenu;' made UdonSharp fail to compile, claiming that GMMenu is a namespace that
    // is being used as a type, but only if it was compiling without the audio manager package present.
    // That makes just about zero sense but it is what it is.
    [HideInInspector, SerializeField] public Sylan.GMMenu.GMMenu gmMenu;
    public const string GMMenuPropertyName = nameof(gmMenu);
}
