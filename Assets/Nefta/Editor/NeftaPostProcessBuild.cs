#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Nefta.Editor
{
    public class NeftaPostProcessBuild
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            var swiftRelativePath = "Classes/SwiftSupport.swift";
            var swiftPath = Path.Combine(buildPath, swiftRelativePath);
            if (!File.Exists(swiftPath))
            {
                using var writer = File.CreateText(swiftPath);
                writer.WriteLine("import Foundation\n");
                writer.Close();
            }

            var projectPath = PBXProject.GetPBXProjectPath(buildPath);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);

            var unityFrameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
            var swiftFileGuid = project.AddFile(swiftRelativePath, swiftRelativePath);
            project.AddFileToBuild(unityFrameworkTargetGuid, swiftFileGuid);
            
            var swiftVersion = project.GetBuildPropertyForAnyConfig(unityFrameworkTargetGuid, "SWIFT_VERSION");
            if (string.IsNullOrEmpty(swiftVersion))
            {
                project.SetBuildProperty(unityFrameworkTargetGuid, "SWIFT_VERSION", "5.0");
            }
            
            project.AddBuildProperty(unityFrameworkTargetGuid, "CLANG_ENABLE_MODULES", "YES");
            project.AddBuildProperty(project.GetUnityMainTargetGuid(), "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            
            project.WriteToFile(projectPath);
        }
    }
}
#endif