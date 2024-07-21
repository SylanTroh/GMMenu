#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;

namespace GMMenu.Editor
{
    [InitializeOnLoad]
    public class GMMenuOptionalDefineEditor
    {
        static GMMenuOptionalDefineEditor()
        {
            PackageManager.packageRemoved += OnPackageRemoved;
        }

        private static void OnPackageRemoved(PackageRemovedEventArgs args)
        {
            // Check if the removed package corresponds to the one you're interested in
            if (args.packageId == "com.sylan.audiomanager")
            {
                // Remove the SYLAN_AUDIOMANAGER scripting define symbol
                RemoveScriptingDefineSymbol("SYLAN_AUDIOMANAGER");
            }
        }

        private static void RemoveScriptingDefineSymbol(string symbol)
        {
            BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            string[] symbols = defines.Split(';');
            string newDefines = "";
            foreach (string s in symbols)
            {
                if (s != symbol)
                {
                    newDefines += s + ";";
                }
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newDefines);
            Debug.LogFormat("Removed Scripting Define Symbol for selected build target ({0}): {1}", targetGroup.ToString(), symbol);
        }
    }
}

}
#endif