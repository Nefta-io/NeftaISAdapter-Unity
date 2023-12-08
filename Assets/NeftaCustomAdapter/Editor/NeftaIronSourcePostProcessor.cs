using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace NeftaCustomAdapter.Editor
{
    public class NeftaIronSourcePostProcessor : MonoBehaviour
    {
        private const string PostInstallIronSourceLinkage = @"
post_install do |installer|
  installer.pods_project.targets.each do |target|
    if target.name == 'NeftaISAdapter'
      framework_ref = installer.pods_project.reference_for_path(File.dirname(__FILE__) + '/Pods/IronSourceSDK/IronSource/IronSource.xcframework')
      target.frameworks_build_phase.add_file_reference(framework_ref, true)
    end
  end
end";
        
        [PostProcessBuild(45)]
        private static void PostProcessBuild(BuildTarget target, string buildPath)
        {
            if (target == BuildTarget.iOS)
            {
                const string dependency = "pod 'NeftaISAdapter', :git => 'https://github.com/Nefta-io/NeftaISAdapter.git', :tag => '1.1.8'";
                
                var path = buildPath + "/Podfile";
                var text = File.ReadAllText(path);
                var podIndex = text.IndexOf("pod 'NeftaISAdapter'", StringComparison.InvariantCulture);
                if (podIndex >= 0)
                {
                    var dependencyEnd = text.IndexOf('\n', podIndex);
                    text = text.Substring(0, podIndex) + dependency + text.Substring(dependencyEnd);
                }
                else
                {
                    podIndex = text.IndexOf("pod 'IronSourceSDK'", StringComparison.InvariantCulture);
                    var index = text.IndexOf('\n', podIndex);
                    text = text.Insert(index + 1, $"  {dependency}\n");
                }

                var configuration = GetNeftaConfiguration();
                if (configuration != null && configuration._forceIncludeNeftaSDK)
                {
                    var iphoneTargetIndex = text.IndexOf("target 'Unity-iPhone' do", StringComparison.InvariantCulture);
                    var index = text.IndexOf('\n', iphoneTargetIndex);
                    text = text.Insert(index + 1, "  pod 'NeftaSDK'\n");
                }

                text += PostInstallIronSourceLinkage;
                File.WriteAllText(path, text);
            }
        }

        private static NeftaConfiguration GetNeftaConfiguration()
        {
            NeftaConfiguration configuration = null;
            
            string[] guids = AssetDatabase.FindAssets("t:NeftaConfiguration");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                configuration = AssetDatabase.LoadAssetAtPath<NeftaConfiguration>(path);
            }

            return configuration;
        }
        
        [MenuItem("Ads Mediation/Select Nefta Configuration", false, int.MaxValue)]
        private static void SelectNeftaConfiguration()
        {
            var configuration = GetNeftaConfiguration();
            if (configuration == null)
            {
                const string scriptName = "NeftaIronSourcePostProcessor";
                string[] scriptGuid = AssetDatabase.FindAssets(scriptName);
                var scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid[0]);
                
                configuration = ScriptableObject.CreateInstance<NeftaConfiguration>();
                AssetDatabase.CreateAsset(configuration, scriptPath.Replace(scriptName + ".cs", "NeftaConfiguration.asset"));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            Selection.objects = new UnityEngine.Object[] { configuration };
        }
        
        [MenuItem("Ads Mediation/Export Nefta Custom Adapter SDK", false, int.MaxValue)]
        private static void ExportAdSdkPackage()
        {
            var packageName = "NeftaIS_SDK_v1.1.8.unitypackage";
            var assetPaths = new string[] { "Assets/NeftaCustomAdapter" };

            try
            {
                AssetDatabase.ExportPackage(assetPaths, packageName, ExportPackageOptions.Recurse);
                Debug.Log($"Finished exporting {packageName}");   
            }
            catch (Exception e)
            {
                Debug.LogError($"Error exporting {packageName}: {e.Message}");   
            }
        }
    }
}