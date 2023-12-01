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
                var path = buildPath + "/Podfile";
                var text = File.ReadAllText(path);
                var podIndex = text.IndexOf("pod 'IronSourceSDK'", StringComparison.InvariantCulture);
                var index = text.IndexOf('\n', podIndex); 
                text = text.Insert(index + 1, "  pod 'NeftaISAdapter', :git => 'https://github.com/Nefta-io/NeftaISAdapter.git', :tag => '1.1.4'\n");

                // if you're are in framework mode (use_frameworks!) and link them statically (:linkage => :static)
                // var iphoneTargetIndex = text.IndexOf("target 'Unity-iPhone' do", StringComparison.InvariantCulture);
                // index = text.IndexOf('\n', iphoneTargetIndex);
                // text = text.Insert(index + 1, "  pod 'NeftaSDK'\n");
                
                text += PostInstallIronSourceLinkage;
                File.WriteAllText(path, text);
            }
        }
        
        [MenuItem("Ads Mediation/Export Nefta Custom Adapter SDK", false, int.MaxValue)]
        private static void ExportAdSdkPackage()
        {
          var packageName = "NeftaIS_SDK_v1.1.4.unitypackage";
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