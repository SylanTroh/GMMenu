#if !SYLAN_AUDIOMANAGER_VERSION
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
            // This only runs if SYLAN_AUDIOMANAGER_VERSION is unset, in other words only if the audio manager
            // package is not in the project. Which is perfect, exactly what we want.
            // By doing this in the on assembly load this also ensures that the define gets removed in cases
            // where the SYLAN_AUDIOMANAGER define got into the settings through any unknown means, like a
            // user doing it manually, or both the audio manager and the GM Menu having been in the project
            // before, then been removed and only the GM Menu being added back later. Basically catching edge
            // cases.
            RemoveDefinesIfMissing(EditorUserBuildSettings.selectedBuildTargetGroup, new string[] { "SYLAN_AUDIOMANAGER" });
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
