#if !UNITY_EDITOR && UNITY_IOS
using System;
using System.Runtime.InteropServices;
using AOT;
#endif
using System.Text;
using Nefta.Core.Events;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nefta
{
    public class Adapter
    {
#if UNITY_EDITOR
        private static bool _plugin;
#elif UNITY_IOS
        [DllImport ("__Internal")]
        private static extern void NeftaPlugin_EnableLogging(bool enable);

        [DllImport ("__Internal")]
        private static extern IntPtr NeftaPlugin_Init(string appId);

        [DllImport ("__Internal")]
        private static extern void NeftaPlugin_Record(IntPtr instance, string recordedEvent);

        [DllImport ("__Internal")]
        private static extern string NeftaPlugin_ShowNuid(IntPtr instance);

        private static IntPtr _plugin;
#elif UNITY_ANDROID
        private static AndroidJavaObject _plugin;
#endif
        private static StringBuilder _eventBuilder;
        
        public static void EnableLogging(bool enable)
        {
#if UNITY_EDITOR
#elif UNITY_IOS
            NeftaPlugin_EnableLogging(enable);
#endif
        }
        
        public static void Init()
        {
            _eventBuilder = new StringBuilder(128);
            var configuration = Resources.Load<NeftaConfiguration>(NeftaConfiguration.FileName);
            if (configuration == null)
            {
                Debug.LogError("Missing Nefta Configuration; Does NeftaConfiguration asset (Window > Nefta > Select Nefta Configuration) exists in Resources?");
            }
#if UNITY_EDITOR
            _plugin = true;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChange;
#elif UNITY_IOS
            EnableLogging(configuration._isLoggingEnabled);
            _plugin = NeftaPlugin_Init(configuration._iOSAppId);
#elif UNITY_ANDROID
            AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaClass neftaPluginClass = new AndroidJavaClass("com.nefta.sdk.NeftaPlugin");
            _plugin = neftaPluginClass.CallStatic<AndroidJavaObject>("Init", unityActivity, configuration._androidAppId);

            Application.focusChanged += OnFocusChanged;
#endif
        }
        
#if !UNITY_EDITOR && UNITY_ANDROID
        private static void OnFocusChanged(bool hasFocus)
        {
            if (hasFocus)
            {
                _plugin.Call("OnResume");
            }
            else
            {
                _plugin.Call("OnPause");
            }
        }
#endif

        public static void Record(GameEvent gameEvent)
        {
            var recordedEvent = gameEvent.GetRecordedEvent();
            _eventBuilder.Clear();
            _eventBuilder.Append("{");
            _eventBuilder.Append("\"event_type\":\"");
            _eventBuilder.Append(recordedEvent._type);
            _eventBuilder.Append("\",\"event_category\":\"");
            _eventBuilder.Append(recordedEvent._category);
            _eventBuilder.Append("\",\"value\":");
            _eventBuilder.Append(recordedEvent._value.ToString());
            _eventBuilder.Append(",\"event_sub_category\":\"");
            _eventBuilder.Append(recordedEvent._subCategory);
            if (recordedEvent._itemName != null)
            {
                _eventBuilder.Append("\",\"item_name\":\"");
                _eventBuilder.Append(recordedEvent._itemName);
            }
            if (recordedEvent._customPayload != null)
            {
                _eventBuilder.Append("\",\"custom_publisher_payload\":\"");
                _eventBuilder.Append(recordedEvent._customPayload);
            }
            _eventBuilder.Append("\"}");
            var eventString = _eventBuilder.ToString();
#if UNITY_EDITOR
            Assert.IsTrue(_plugin, "Before recording game event Init should be called");
            Debug.Log($"Recording {eventString}");
#elif UNITY_IOS
            NeftaPlugin_Record(_plugin, eventString);
#elif UNITY_ANDROID
            _plugin.Call("Record", eventString);
#endif
        }
        
        public static string ShowNuid()
        {
            string nuid = null;
#if UNITY_EDITOR
#elif UNITY_IOS
            nuid = NeftaPlugin_ShowNuid(_plugin);
#elif UNITY_ANDROID
            nuid = _plugin.Call<string>("ShowNuid");
#endif
            return nuid;
        }
        
#if UNITY_EDITOR
        private static void OnPlayModeChange(UnityEditor.PlayModeStateChange playMode)
        {
            if (playMode == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                _plugin = false;
            }
        }
#endif
    }
}