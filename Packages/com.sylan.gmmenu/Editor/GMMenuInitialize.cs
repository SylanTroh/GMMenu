using Sylan.GMMenu.EditorUtilities;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Sylan.GMMenu
{
    [InitializeOnLoad]
    public class GMMenuInitialize : IVRCSDKBuildRequestedCallback
    {
        private static bool SetSerializedProperties()
        {
            //Object with Serialized Property(s)
            if (!SerializedPropertyUtils.GetSerializedObjects<GMMenuPart>(out SerializedObject[] serializedObjects)) return false;

            foreach (SerializedObject serializedObject in serializedObjects)
            {
                //Set Serialized Property
                SerializedPropertyUtils.PopulateSerializedProperty<GMMenu>(serializedObject, GMMenuPart.GMMenuPropertyName);
            }
            return true;
        }
        //
        //Run On Play
        //
        static GMMenuInitialize()
        //Rename Static Constructor to match Class name
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;
            SetSerializedProperties();
        }
        //
        // Run On Build
        //
        public int callbackOrder => 0;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType != VRCSDKRequestedBuildType.Scene) return false;
            return SetSerializedProperties();
        }
    }
}
