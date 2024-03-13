using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.IO;
using UnityEngine.AddressableAssets;

namespace Wtf
{
    public static partial class BuildTool
    {
        [UnityEditor.MenuItem("Tools/BuildTool/BuildWebGL")]
        public static void BuildWebGL()
        {
            Console.WriteLine("BuildWebGL Start");
            var targetGroup = BuildTargetGroup.WebGL;
            var target = BuildTarget.WebGL;

            // Set Build Target
            if (EditorUserBuildSettings.activeBuildTarget != target)
                EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);

            EditorUserBuildSettings.il2CppCodeGeneration = UnityEditor.Build.Il2CppCodeGeneration.OptimizeSpeed;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.compressFilesInPackage = true;

            // 使用 Gama 色彩空间
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            // 关闭自动 Graphics API
            UnityEditor.PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);
            UnityEditor.PlayerSettings.SetGraphicsAPIs(target,
                new[] {
                    UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2
                });
            // 开启多线程渲染
            UnityEditor.PlayerSettings.graphicsJobs = true;
            UnityEditor.PlayerSettings.statusBarHidden = true;

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetProfileSettings profile = settings.profileSettings;
            string profileID = settings.profileSettings.GetProfileId("Default");
            settings.activeProfileId = profileID;

            // set to load remote config bundle
            SetDefineSymbols(target, new List<string> {
                "ENABLE_LOG",
            }, true);

            AddressableAssetSettings.CleanPlayerContent();
            AddressableAssetSettings.BuildPlayerContent(out var result);
            if (string.IsNullOrEmpty(result.Error))
            {
                var projDir = Path.GetDirectoryName(Application.dataPath);
                // copy link.xml to Assets/
                File.Copy(Path.Combine(Addressables.BuildPath, "AddressablesLink/link.xml"),
                    Path.Combine(projDir, "Assets/AddressableAssetsData/link.xml"), true);
                AssetDatabase.Refresh();

                PlayerSettings.WebGL.threadsSupport = false;
                PlayerSettings.runInBackground = false;

                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
                PlayerSettings.WebGL.dataCaching = false;

#if UNITY_2021_2_OR_NEWER
                PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.External;
#else
                PlayerSettings.WebGL.debugSymbols = true;
#endif

                EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;

                PlayerSettings.allowedAutorotateToPortrait = true;
                PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;
                PlayerSettings.allowedAutorotateToLandscapeLeft = false;
                PlayerSettings.allowedAutorotateToLandscapeRight = false;
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

                // 使用 C# 4.6
                UnityEditor.PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.WebGL, ApiCompatibilityLevel.NET_4_6);
                // 使用 IL2CPP Master 模式
                // https://issuetracker.unity3d.com/issues/il2cpp-android-build-failure-when-building-project-with-toolbuddy-asset-and-il2cpp-scripting-backend-selected
                /** 
                    1. For Unity 2022.1 and earlier versions, change the C++ Compiler Configuration option from a value of "Master" 
                    to a value of "Release". This will avoid link time optimization, and therefore avoid the error.
                    This may change the performance characteristics of the code at run time though.
                    2. Use Unity 2022.2 and later, where Unity uses the r23 NDK and the lld linker. 
                    This bug is not present in that version.
                */
                UnityEditor.PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.WebGL, Il2CppCompilerConfiguration.Release);
                // 开启 Unsafe Code
                UnityEditor.PlayerSettings.allowUnsafeCode = true;
                // 开启 Strip Engine Code
                UnityEditor.PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.Low);
                // 开启 Strip Unused Mesh Components
                UnityEditor.PlayerSettings.stripUnusedMeshComponents = true;
                // 开启 Strip Mip Maps
                UnityEditor.PlayerSettings.mipStripping = true;

                UnityEditor.PlayerSettings.SetIncrementalIl2CppBuild(BuildTargetGroup.WebGL, false);

                PlayerSettings.WebGL.emscriptenArgs = string.Empty;

#if UNITY_2021_2_OR_NEWER
                PlayerSettings.WebGL.emscriptenArgs += " -s EXPORTED_FUNCTIONS=_sbrk,_emscripten_stack_get_base,_emscripten_stack_get_end";
#if UNITY_2021_2_5
                PlayerSettings.WebGL.emscriptenArgs += ",_main";
#endif

#endif
                PlayerSettings.runInBackground = false;
                PlayerSettings.WebGL.emscriptenArgs += $" -s TOTAL_MEMORY={256}MB";

                UnityEngine.Debug.Log("[Builder] Starting to build WebGL project ... ");
                UnityEngine.Debug.Log("PlayerSettings.WebGL.emscriptenArgs : " + PlayerSettings.WebGL.emscriptenArgs);

                // PlayerSettings.WebGL.memorySize = memorySize;
                BuildOptions option = BuildOptions.None;

#if UNITY_2021_2_OR_NEWER
                option |= BuildOptions.CleanBuildCache;
#endif

                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                {
                    UnityEngine.Debug.LogFormat("[Builder] Current target is: {0}, switching to: {1}", EditorUserBuildSettings.activeBuildTarget, BuildTarget.WebGL);
                    if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                    {
                        UnityEngine.Debug.LogFormat("[Builder] Switching to {0}/{1} failed!", BuildTargetGroup.WebGL, BuildTarget.WebGL);
                        Console.WriteLine("BuildSupport.BuildTool.BuildWebGL - Fail");
                        return;
                    }
                }

                var buildDesDir = Path.Combine(Application.dataPath, "../Build/webgl");
                if (Directory.Exists(buildDesDir))
                {
                    Directory.Delete(buildDesDir, true);
                    Directory.CreateDirectory(buildDesDir);
                }
                var report = BuildPipeline.BuildPlayer(new[] { "Assets/Scenes/Start.unity" }, buildDesDir, BuildTarget.WebGL, option);
                if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    UnityEngine.Debug.LogFormat("[Builder] BuildPlayer failed. emscriptenArgs:{0}", PlayerSettings.WebGL.emscriptenArgs);
                    Console.WriteLine("BuildSupport.BuildTool.BuildWebGL - Fail");

                    foreach (var step in report.steps)
                    {
                        Console.WriteLine(step.name);
                        foreach (var message in step.messages)
                        {
                            Console.WriteLine(message.content);
                        }
                    }

                    return;
                }

                Console.WriteLine("BuildSupport.BuildTool.BuildWebGL - Done");
            }
            else
            {
                Console.WriteLine(result.Error);
            }
        }
    }
}