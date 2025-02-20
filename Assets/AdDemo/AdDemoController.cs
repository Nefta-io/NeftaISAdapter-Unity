using System.Collections.Generic;
using Nefta;
using Nefta.Events;
using UnityEngine;

namespace AdDemo
{
    public class AdDemoController : MonoBehaviour
    {
#if UNITY_IOS
        private const string _appKey = "1c0431145";
        private const string _neftaAppId = "5661184053215232";
#else // UNITY_ANDROID
        private const string _appKey = "1bb635bc5";
        private const string _neftaAppId = "5643649824063488";
#endif
        private bool _isBannerShown;

        [SerializeField] private BannerController _banner;
        [SerializeField] private InterstitialController _interstitial;
        [SerializeField] private RewardedController _rewarded;
        
        private void Awake()
        {
            Adapter.EnableLogging(true);
            Adapter.Init(_neftaAppId);
                
            new ProgressionEvent(Type.Task, Status.Start) { _name = "tutorial", _value = 1}.Record();
            
            Adapter.BehaviourInsightCallback = OnBehaviourInsight;
            Adapter.GetBehaviourInsight(new string[] { "p_churn_14d"});
            
            Debug.Log("unity-script: IronSource.Agent.validateIntegration");
            IronSource.Agent.validateIntegration();

            Debug.Log("unity-script: unity version" + IronSource.unityVersion());

            // SDK init
            Debug.Log("unity-script: IronSource.Agent.init");
            IronSource.Agent.init(_appKey);

            IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;
            IronSourceEvents.onImpressionDataReadyEvent += ImpressionDataReadyEvent;
            
            _banner.Init();
            _interstitial.Init();
            _rewarded.Init();
        }
        
        void OnApplicationPause(bool isPaused)
        {
            Debug.Log("unity-script: OnApplicationPause = " + isPaused);
            IronSource.Agent.onApplicationPause(isPaused);
        }
        
        void SdkInitializationCompletedEvent()
        {
            Debug.Log("unity-script: I got SdkInitializationCompletedEvent");
        }
        
        void ImpressionDataReadyEvent(IronSourceImpressionData impressionData)
        {
            Debug.Log("unity - script: I got ImpressionDataReadyEvent ToString(): " + impressionData.ToString());
            Debug.Log("unity - script: I got ImpressionDataReadyEvent allData: " + impressionData.allData);
        }
        
        private void OnBehaviourInsight(Dictionary<string, Insight> behaviourInsight)
        {
            foreach (var insight in behaviourInsight)
            {
                var insightValue = insight.Value;
                Debug.Log($"BehaviourInsight {insight.Key} status:{insightValue._status} i:{insightValue._int} f:{insightValue._float} s:{insightValue._string}");
            }
        }
    }
}
