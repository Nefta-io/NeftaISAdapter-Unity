
using Nefta;
using Nefta.Events;
using Unity.Services.LevelPlay;
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
            
            Adapter.SetContentRating(Adapter.ContentRating.ParentalGuidance);
            
            new ProgressionEvent(Type.Task, Status.Start) { _name = "tutorial", _value = 1}.Record();
            
            IronSource.Agent.setMetaData("is_test_suite", "enable");
            IronSourceEvents.onSegmentReceivedEvent += SegmentReceivedEvent;
            LevelPlay.OnInitSuccess += OnInitSuccess;
            LevelPlay.OnInitFailed += OnInitFailed;
            LevelPlay.Init(_appKey);
            
            IronSource.Agent.validateIntegration();
            
            _banner.Init();
            _interstitial.Init();
            _rewarded.Init();
            
            _networkToggle.onValueChanged.AddListener(OnNetworkToggled);
            _testSuiteButton.onClick.AddListener(OnTestSuiteClick);
            SetSegment(false);
            
#if UNITY_EDITOR
            OnInitSuccess(null);
#endif
        }
        
        private void OnApplicationPause(bool isPaused)
        {
            LevelPlay.SetPauseGame(isPaused);
        }
        
        private void OnInitSuccess(LevelPlayConfiguration configuration)
        {
            _banner.OnReady();
            _interstitial.OnReady();
            _rewarded.OnReady();
        }

        private void OnInitFailed(LevelPlayInitError error)
        {
            Debug.Log("OnInitFailed: "+ error);
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
