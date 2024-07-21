#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GMMenu.Editor
{
    [InitializeOnLoad]
    public class AudioManagerDefineManager
    {
        static AudioManagerDefineManager()
        {
            Debug.Log("Hello!");
#if !SYLAN_AUDIOMANAGER_VERSION
            RemoveDefinesIfMissing(EditorUserBuildSettings.selectedBuildTargetGroup, new string[] { "SYLAN_AUDIOMANAGER" });
#endif
        }

        private static void RemoveDefinesIfMissing(BuildTargetGroup buildTarget, params string[] removeDefines)
        {
            bool definesChanged = false;
            string existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            HashSet<string> defineSet = new HashSet<string>();

            if (existingDefines.Length > 0)
            {
                defineSet = new HashSet<string>(existingDefines.Split(';'));
            }

            foreach (string removeDefine in removeDefines)
            {
                if (defineSet.Remove(removeDefine))
                {
                    definesChanged = true;
                }
            }

            if (definesChanged)
            {
                string finalDefineString = string.Join(";", defineSet.ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, finalDefineString);
                Debug.LogFormat("Set Scripting Define Symbols for selected build target ({0}) to: {1}", buildTarget.ToString(), finalDefineString);
            }
        }
    }

}
#endif
