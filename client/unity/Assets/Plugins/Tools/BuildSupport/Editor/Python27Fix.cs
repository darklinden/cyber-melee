#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Wtf
{
    public class Python27Fix : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;
        public void OnPreprocessBuild(BuildReport report)
        {
            System.Environment.SetEnvironmentVariable("EMSDK_PYTHON", "/usr/local/bin/python");
            Debug.Log("Python27Fix: OnPreprocessBuild");
        }
    }
}
#endif