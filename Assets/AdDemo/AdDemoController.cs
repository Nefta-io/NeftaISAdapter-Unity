using Nefta;
using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class AdDemoController : MonoBehaviour
    {
#if UNITY_IOS
        private const string _neftaAppId = "5717797661310976";
#else // UNITY_ANDROID
        private const string _neftaAppId = "5657497763315712";
#endif

        [SerializeField] private Text _title;
        [SerializeField] private Toggle _networkToggle;
        [SerializeField] private Button _testSuiteButton;

        private bool _isSimulator;
        
        private void Awake()
        {
            string apiKey = null;
            var demoConfig = Resources.Load<DemoConfig>("DemoConfig");
            if (demoConfig != null)
            {
                apiKey = demoConfig.GetApiKey();
                ToggleUI(demoConfig._isSimulator);
                
                _title.GetComponent<Button>().onClick.AddListener(() => ToggleUI(!_isSimulator));
            }

            _title.text = $"IronSource Integration {LevelPlay.PluginVersion}";
            
            Adapter.EnableLogging(true);
            Adapter.Init(_neftaAppId);
            Adapter.OnReady += config =>
            {
                Debug.Log($"[NeftaPluginIS] Should bypass Nefta optimization? {config._skipOptimization}");
            };
            
            IronSource.Agent.setMetaData("is_test_suite", "enable");
            LevelPlay.OnInitFailed += OnInitFailed;
            // Done implicitly in Adapter Init
            //LevelPlay.OnImpressionDataReady += Adapter.OnLevelPlayImpression;
            LevelPlay.Init(apiKey);
            
            LevelPlay.ValidateIntegration();
            
            _networkToggle?.onValueChanged.AddListener(OnNetworkToggled);
            _testSuiteButton?.onClick.AddListener(OnTestSuiteClick);
            SetSegment(false);
        }
        
        private void OnApplicationPause(bool isPaused)
        {
            LevelPlay.SetPauseGame(isPaused);
        }

        private void OnInitFailed(LevelPlayInitError error)
        {
            Debug.Log("OnInitFailed: "+ error);
        }

        private void OnNetworkToggled(bool isOn)
        {
            //SetSegment(isOn);
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

        private void ToggleUI(bool isSimulator)
        {
            _isSimulator = isSimulator;
            
            transform.Find("pnl_content/InterstitialSimulatorController").gameObject.SetActive(isSimulator);
            transform.Find("pnl_content/RewardedSimulatorController").gameObject.SetActive(isSimulator);
            
            transform.Find("pnl_content/TestingController").gameObject.SetActive(!isSimulator);
            transform.Find("pnl_content/InterstitialController").gameObject.SetActive(!isSimulator);
            transform.Find("pnl_content/RewardedController").gameObject.SetActive(!isSimulator);
        }
    }
}
