#if UNITY_IOS
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Nefta.Editor
{
    public class NeftaPostProcessBuild
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
            var swiftRelativePath = "Classes/SwiftSupport.swift";
            var swiftPath = Path.Combine(path, swiftRelativePath);
            if (!File.Exists(swiftPath))
            {
                using var writer = File.CreateText(swiftPath);
                writer.WriteLine("import Foundation\n");
                writer.Close();
            }

            var projectPath = PBXProject.GetPBXProjectPath(path);
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
            project.AddBuildProperty(project.GetUnityMainTargetGuid(), "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES",
                "YES");

            project.WriteToFile(projectPath);
            
            var plistPath = Path.Combine(path, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            plist.root.values.TryGetValue("SKAdNetworkItems", out var skAdNetworkItems);
            var existingSkAdNetworkIds = new HashSet<string>();

            if (skAdNetworkItems != null && skAdNetworkItems.GetType() == typeof(PlistElementArray))
            {
                var plistElementDictionaries = skAdNetworkItems.AsArray().values
                    .Where(plistElement => plistElement.GetType() == typeof(PlistElementDict));
                foreach (var plistElement in plistElementDictionaries)
                {
                    PlistElement existingId;
                    plistElement.AsDict().values.TryGetValue("SKAdNetworkIdentifier", out existingId);
                    if (existingId == null || existingId.GetType() != typeof(PlistElementString)
                                           || string.IsNullOrEmpty(existingId.AsString())) continue;

                    existingSkAdNetworkIds.Add(existingId.AsString());
                }
            }
            else
            {
                skAdNetworkItems = plist.root.CreateArray("SKAdNetworkItems");
            }

            const string neftaSkAdNetworkId = "2lj985962l.adattributionkit";
            if (!existingSkAdNetworkIds.Contains(neftaSkAdNetworkId))
            {
                var skAdNetworkItemDict = skAdNetworkItems.AsArray().AddDict();
                skAdNetworkItemDict.SetString("SKAdNetworkIdentifier", neftaSkAdNetworkId);
            }

            plist.WriteToFile(plistPath);
        }
    }
}
#endif