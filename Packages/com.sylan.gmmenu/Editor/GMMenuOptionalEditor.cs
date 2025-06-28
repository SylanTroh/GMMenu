#if SYLAN_AUDIOMANAGER && !COMPILER_UDONSHARP && UNITY_EDITOR
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