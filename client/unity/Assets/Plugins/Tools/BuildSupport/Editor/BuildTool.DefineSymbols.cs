using System.Collections.Generic;
using UnityEditor;
using System.Text;

namespace Wtf
{
    public static partial class BuildTool
    {
        public static void SetDefineSymbols(BuildTarget target, List<string> symbols, bool clearBeforeAdd = false)
        {
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            List<string> sdsList;
            if (clearBeforeAdd)
            {
                sdsList = new List<string>();
            }
            else
            {
                var sds = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
                if (string.IsNullOrWhiteSpace(sds))
                    sdsList = new List<string>();
                else
                    sdsList = new List<string>(sds.Split(';'));
            }

            foreach (var symbol in symbols)
            {
                if (!sdsList.Contains(symbol))
                {
                    sdsList.Add(symbol);
                }
            }

            if (sdsList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(sdsList[0]);
                for (int i = 1; i < sdsList.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(sdsList[i]))
                    {
                        sb.Append(';');
                        sb.Append(sdsList[i]);
                    }
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, sb.ToString());
            }
            else
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, "");
            }
        }
    }
}