using Nefta;
using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class AdDemoController : MonoBehaviour
    {
#if UNITY_IOS
        private const string _appKey = "1c0431145";
        private const string _neftaAppId = "5717797661310976";
#else // UNITY_ANDROID
        private const string _appKey = "1bb635bc5";
        private const string _neftaAppId = "5657497763315712";
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
            
            IronSource.Agent.setMetaData("is_test_suite", "enable");
            IronSourceEvents.onSegmentReceivedEvent += SegmentReceivedEvent;
            LevelPlay.OnInitSuccess += OnInitSuccess;
            LevelPlay.OnInitFailed += OnInitFailed;
            LevelPlay.Init(_appKey);
            
            LevelPlay.ValidateIntegration();
            
            _banner.Init();
            _interstitial.Init();
            _rewarded.Init();
            
            _networkToggle.onValueChanged.AddListener(OnNetworkToggled);
            _testSuiteButton.onClick.AddListener(OnTestSuiteClick);
            SetSegment(true);
            
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
            LevelPlay.LaunchTestSuite();
        }

        private void SetSegment(bool isIs)
        {
            LevelPlaySegment networkSegment = new LevelPlaySegment();
            networkSegment.SegmentName = isIs ? "is" : "nefta";
            LevelPlay.SetSegment(networkSegment);

            Debug.Log("Selected segment: "+ networkSegment.SegmentName);
        }

        private void SegmentReceivedEvent(string segment)
        {
            Debug.Log("SegmentReceivedEvent: "+ segment);
        }
    }
}
