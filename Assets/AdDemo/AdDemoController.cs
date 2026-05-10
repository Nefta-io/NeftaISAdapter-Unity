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
        [SerializeField] private Button _testSuiteButton;
        
        [SerializeField] private GameObject _groupPanel;
        [SerializeField] private Button _defaultButton;
        [SerializeField] private Button _optimizedButton;
        [SerializeField] private Button _simulatorButton;
        
        [SerializeField] private InterstitialUi _interstitialUi;
        [SerializeField] private RewardedUi _rewardedUi;
        
        [SerializeField] private InterstitialSim _interstitialSimulator;
        [SerializeField] private RewardedSim _rewardedSimulator;
        
        private void Awake()
        {
            _title.text = $"IronSource Integration {LevelPlay.PluginVersion}";
            
            Adapter.EnableLogging(true);
            Adapter.InitWithAppId(_neftaAppId, config => {
                Debug.Log($"[NeftaPluginIS] Should skip Nefta optimization: {config._skipOptimization} for: {config._nuid}");
            });
            
            IronSource.Agent.setMetaData("is_test_suite", "enable");
            LevelPlay.OnInitFailed += OnInitFailed;
            // Done implicitly in Adapter Init
            //LevelPlay.OnImpressionDataReady += Adapter.OnLevelPlayImpression;
            var demoConfig = Resources.Load<DemoConfig>("DemoConfig");
            if (demoConfig != null)
            {
                var apiKey = demoConfig.GetApiKey();
                LevelPlay.Init(apiKey);
                
                LevelPlay.ValidateIntegration();
            }
            
            _testSuiteButton.onClick.AddListener(OnTestSuiteClick);
            
            _defaultButton.onClick.AddListener(OnDefaultClick);
            _optimizedButton.onClick.AddListener(OnOptimizedClick);
            _simulatorButton.onClick.AddListener(OnSimulatorClick);
        }

        private void OnDefaultClick()
        {
            _groupPanel.SetActive(false);
            
            _interstitialUi.Init(new InterstitialDefault());
            _rewardedUi.Init(new RewardedDefault());
        }
        
        private void OnOptimizedClick()
        {
            _groupPanel.SetActive(false);
            
            _interstitialUi.Init(new InterstitialOptimized());
            _rewardedUi.Init(new RewardedOptimized());
        }
        
        private void OnSimulatorClick()
        {
            _groupPanel.SetActive(false);
            
            _interstitialSimulator.Init();
            _rewardedSimulator.Init();
        }

        private void OnInitFailed(LevelPlayInitError error)
        {
            Debug.Log("OnInitFailed: "+ error);
        }
        
        private void OnApplicationPause(bool isPaused)
        {
            LevelPlay.SetPauseGame(isPaused);
        }
        
        private void OnTestSuiteClick()
        {
            LevelPlay.LaunchTestSuite();
        }
    }
}
