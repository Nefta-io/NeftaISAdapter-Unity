using UnityEngine;

namespace AdDemo
{
    public class DemoConfig : ScriptableObject
    {
        public string _isKeyios;
        public string _isKeyAndroid;
        public bool _isSimulator;

        public string GetApiKey()
        {
#if UNITY_IOS
            return _isKeyios;
#else
            return _isKeyAndroid;
#endif
        }
    }
}