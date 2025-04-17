using System.Collections.Generic;
using com.unity3d.mediation;
using Nefta;
using Nefta.Events;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField] private Toggle _networkToggle;
        [SerializeField] private Button _testSuiteButton;
        
        [SerializeField] private BannerController _banner;
        [SerializeField] private InterstitialController _interstitial;
        [SerializeField] private RewardedController _rewarded;
        
        private void Awake()
        {
            Adapter.EnableLogging(true);
            Adapter.Init(_neftaAppId);

            Adapter.BehaviourInsightCallback = OnBehaviourInsight;
            Adapter.GetBehaviourInsight(new string[] { BannerController.InsightName });
            Adapter.SetContentRating(Adapter.ContentRating.ParentalGuidance);
            
            new ProgressionEvent(Type.Task, Status.Start) { _name = "tutorial", _value = 1}.Record();
            
            Debug.Log("unity-script: IronSource.Agent.init");
            IronSource.Agent.setMetaData("is_test_suite", "enable");
            IronSourceEvents.onSegmentReceivedEvent += SegmentReceivedEvent;
            LevelPlay.OnInitSuccess += OnInitSuccess;
            LevelPlay.OnInitFailed += OnInitFailed;
            LevelPlay.Init(_appKey);
            
            Debug.Log("unity-script: IronSource.Agent.validateIntegration");
            IronSource.Agent.validateIntegration();
            Debug.Log("unity-script: unity version" + IronSource.unityVersion());
            
            _banner.Init();
            _interstitial.Init();
            _rewarded.Init();
            
            _networkToggle.onValueChanged.AddListener(OnNetworkToggled);
            _testSuiteButton.onClick.AddListener(OnTestSuiteClick);
            SetSegment(false);
        }
        
        private void OnApplicationPause(bool isPaused)
        {
            Debug.Log("unity-script: OnApplicationPause = " + isPaused);
            LevelPlay.SetPauseGame(isPaused);
        }
        
        private void OnInitSuccess(LevelPlayConfiguration configuration)
        {
            Debug.Log("unity-script: I got SdkInitializationCompletedEvent: "+ configuration);
            
            _banner.OnReady();
            _interstitial.OnReady();
            _rewarded.OnReady();
        }

        private void OnInitFailed(LevelPlayInitError error)
        {
            Debug.Log("unity-script: I got SdkInitializationCompletedEvent: "+ error);
        }
        
        private void OnBehaviourInsight(Dictionary<string, Insight> behaviourInsight)
        {
            _banner.SetInsights(behaviourInsight);
            
            foreach (var insight in behaviourInsight)
            {
                var insightValue = insight.Value;
                Debug.Log($"BehaviourInsight {insight.Key} status:{insightValue._status} i:{insightValue._int} f:{insightValue._float} s:{insightValue._string}");
            }
        }

        private void OnNetworkToggled(bool isOn)
        {
            SetSegment(isOn);
        }
        
        private void OnTestSuiteClick()
        {
            IronSource.Agent.launchTestSuite();
        }

        private void SetSegment(bool isIs)
        {
            IronSourceSegment networkSegment = new IronSourceSegment();
            if (isIs)
            {
                networkSegment.segmentName = "is";
            }
            else
            {
                networkSegment.segmentName = "nefta";
            }
            IronSource.Agent.setSegment(networkSegment);

            Debug.Log("Selected segment: "+ networkSegment.segmentName);
        }

        private void SegmentReceivedEvent(string segment)
        {
            Debug.Log("SegmentReceivedEvent: "+ segment);
        }
    }
}
