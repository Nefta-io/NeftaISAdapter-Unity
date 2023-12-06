using UnityEngine;

namespace NeftaCustomAdapter.Editor
{
    public class NeftaConfiguration : ScriptableObject
    {
        [Tooltip("If you're are in framework mode (use_frameworks!) and link them statically (:linkage => :static)")]
        public bool _forceIncludeNeftaSDK;
    }
}