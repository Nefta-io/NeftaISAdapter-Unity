using System;
using System.Collections.Generic;
using Nefta.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor
{
    public class Builder
    {
        [MenuItem("Nefta developer/Export Nefta Custom Adapter SDK", false, int.MaxValue)]
        private static void ExportAdSdkPackage()
        {
            var packageName = $"NeftaIS_SDK_{Application.version}.unitypackage";
            try
            {
                AssetDatabase.ExportPackage("Assets/Nefta", packageName, ExportPackageOptions.Recurse);
                Debug.Log($"Finished exporting {packageName}");   
            }
            catch (Exception e)
            {
                Debug.LogError($"Error exporting {packageName}: {e.Message}");   
            }
        }
        
        [MenuItem("Nefta developer/Open export location")]
        private static void OpenExportLocation()
        {
            EditorUtility.RevealInFinder(Application.dataPath);
        }
        
        private static void Build(BuildTarget target, string outPath)
        {
            var scenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                scenes.Add(scene.path);
            }
            var options = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = outPath,
                target = target,
                options = BuildOptions.StrictMode
            };
            
            NeftaWindow.TryGetPluginImporters();
            NeftaWindow.TogglePlugins(false);
            
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            
            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build successful");
            }
            else if (report.summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed");
            }
        }

        public static void BuildAndroid()
        {
            Build(BuildTarget.Android, "out_Android.apk");
        }
        
        public static void Buildios()
        {
            Build(BuildTarget.iOS, "out_iOS");
        }
    }
}