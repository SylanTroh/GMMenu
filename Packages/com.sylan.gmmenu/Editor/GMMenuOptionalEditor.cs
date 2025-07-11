#if SYLAN_AUDIOMANAGER_VERSION
// Must only be compiled if the audio manager package is present, therefore using SYLAN_AUDIOMANAGER_VERSION
// and not SYLAN_AUDIOMANAGER. Using the latter would cause compile errors when the audio manager package is
// removed, and then due to those compile errors the editor script that is supposed to unset that define would
// not run since it is part of the same assembly definition.
using Sylan.AudioManager;
using Sylan.GMMenu.EditorUtilities;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Sylan.GMMenu
{
    [InitializeOnLoad]
    public class GMMenuOptionalInitialize : IVRCSDKBuildRequestedCallback
    {
        private static bool SetSerializedProperties()
        {
            bool flag = false;
            //Object with Serialized Property(s)
            if (SerializedPropertyUtils.GetSerializedObject<VoiceModeManager>(out SerializedObject serializedObject))
            {
                //Set Serialized Property
                SerializedPropertyUtils.PopulateSerializedProperty<AudioSettingManager>(serializedObject, VoiceModeManager.AudioSettingManagerPropertyName);
                flag = true;
            }

            if (SerializedPropertyUtils.GetSerializedObject<GMWhisperManager>(out serializedObject))
            {
                //Set Serialized Property
                SerializedPropertyUtils.PopulateSerializedProperty<AudioSettingManager>(serializedObject, GMWhisperManager.AudioSettingManagerPropertyName);
                flag = true;
            }

            if (SerializedPropertyUtils.GetSerializedObject<RadioManager>(out serializedObject))
            {
                //Set Serialized Property
                SerializedPropertyUtils.PopulateSerializedProperty<AudioSettingManager>(serializedObject, RadioManager.AudioSettingManagerPropertyName);
                flag = true;
            }

            return flag;
        }
        //
        //Run On Play
        //
        static GMMenuOptionalInitialize()
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
#endif
