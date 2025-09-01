using System.Collections;
using Nefta;
using Nefta.Events;
using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class RewardedController : MonoBehaviour
    {
#if UNITY_IOS
        private string _dynamicAdUnitId = "p3dh8r1mm3ua8fvv";
        private string _defaultAdUnitId = "doucurq8qtlnuz7p";
#else
        private string _dynamicAdUnitId = "x3helvrx8elhig4z"
        private string _defaultAdUnitId = "kftiv52431x91zuk";
#endif
        
        private LevelPlayRewardedAd _dynamicRewarded;
        private bool _isDynamicLoaded;
        private AdInsight _dynamicAdUnitInsight;
        private LevelPlayRewardedAd _defaultRewarded;
        private bool _isDefaultLoaded;
        
        [SerializeField] private Text _title;
        [SerializeField] private Toggle _load;
        [SerializeField] private Text _loadText;
        [SerializeField] private Button _show;
        [SerializeField] private Text _status;
        
        private void StartLoading()
        {
            if (_dynamicRewarded == null)
            {
                GetInsightsAndLoad();   
            }
            if (_defaultRewarded == null)
            {
                LoadDefault();   
            }
        }
        
        private void GetInsightsAndLoad()
        {
            Adapter.GetInsights(Insights.Rewarded, Load, 5);
        }
        
        private void Load(Insights insights)
        {
            _dynamicAdUnitInsight = insights._rewarded;
            if (_dynamicAdUnitInsight != null)
            {
                SetStatus($"Loading Rewarded with floor: {_dynamicAdUnitInsight._floorPrice}");
                var config = new LevelPlayRewardedAd.Config.Builder()
                    .SetBidFloor(_dynamicAdUnitInsight._floorPrice)
                    .Build();
                _dynamicRewarded = new LevelPlayRewardedAd(_dynamicAdUnitId, config);
                _dynamicRewarded.OnAdLoaded += OnAdLoadedDynamic;
                _dynamicRewarded.OnAdLoadFailed += OnAdLoadFailedDynamic;
                _dynamicRewarded.OnAdDisplayed += OnAdDisplayed;
                _dynamicRewarded.OnAdDisplayFailed += OnAdDisplayFailed;
                _dynamicRewarded.OnAdRewarded += OnAdRewarded;
                _dynamicRewarded.OnAdClicked += OnAdClicked;
                _dynamicRewarded.OnAdInfoChanged += OnAdInfoChanged;
                _dynamicRewarded.OnAdClosed += OnAdClosed;
                _dynamicRewarded.LoadAd();
            }
        }

        private void LoadDefault()
        {
            _defaultRewarded = new LevelPlayRewardedAd(_defaultAdUnitId);
            _defaultRewarded.OnAdLoaded += OnAdLoadedDefault;
            _defaultRewarded.OnAdLoadFailed += OnAdLoadFailedDefault;
            _defaultRewarded.OnAdDisplayed += OnAdDisplayed;
            _defaultRewarded.OnAdDisplayFailed += OnAdDisplayFailed;
            _defaultRewarded.OnAdRewarded += OnAdRewarded;
            _defaultRewarded.OnAdClicked += OnAdClicked;
            _defaultRewarded.OnAdInfoChanged += OnAdInfoChanged;
            _defaultRewarded.OnAdClosed += OnAdClosed;
            _defaultRewarded.LoadAd();
        }
        
        private void OnAdLoadFailedDynamic(LevelPlayAdError error)
        {
            Adapter.OnExternalMediationRequestFailed(Adapter.AdType.Rewarded, _dynamicAdUnitInsight, _dynamicAdUnitInsight._floorPrice, error);
            
            SetStatus($"OnAdLoadFailed Dynamic {error}");

            _dynamicRewarded = null;
            StartCoroutine(ReTryLoad(true));
        }
        
        private void OnAdLoadFailedDefault(LevelPlayAdError error)
        {
            Adapter.OnExternalMediationRequestFailed(Adapter.AdType.Rewarded, null, 0, error);
            
            SetStatus($"OnAdLoadFailed Default {error}");
            
            _defaultRewarded = null;
            StartCoroutine(ReTryLoad(false));
        }
        
        private void OnAdLoadedDynamic(LevelPlayAdInfo info)
        {
            Adapter.OnExternalMediationRequestLoaded(Adapter.AdType.Rewarded, _dynamicAdUnitInsight, _dynamicAdUnitInsight._floorPrice, info);
            
            SetStatus($"OnAdLoaded Dynamic {info}");

            _isDynamicLoaded = true;
            
            UpdateShowButton();
        }
        
        private void OnAdLoadedDefault(LevelPlayAdInfo info)
        {
            Adapter.OnExternalMediationRequestLoaded(Adapter.AdType.Rewarded, null, 0, info);
            
            SetStatus($"OnAdLoaded Default {info}");
            
            _isDefaultLoaded = true;
            
            UpdateShowButton();
        }
        
        private IEnumerator ReTryLoad(bool isDynamic)
        {
            yield return new WaitForSeconds(5f);

            if (_load.isOn)
            {
                if (isDynamic)
                {
                    GetInsightsAndLoad();      
                }
                else
                {
                    LoadDefault();   
                }
            }
        }
        
        public void Init()
        {
            _load.onValueChanged.AddListener(OnLoadChanged);
            _show.onClick.AddListener(OnShowClick);

            _load.interactable = false;
            _show.interactable = false;
        }
        
        public void OnReady()
        {
            _load.interactable = true;
        }
        
        private void OnLoadChanged(bool isOn)
        {
            if (isOn)
            {
                StartLoading();   
            }
            
            AddDemoGameEventExample();
        }
        
        private void OnShowClick()
        {
            bool isShown = false;
            if (_isDynamicLoaded)
            {
                if (_dynamicRewarded.IsAdReady())
                {
                    _dynamicRewarded.ShowAd();
                    isShown = true;
                }
                _isDynamicLoaded = false;
                _dynamicRewarded = null;
            }
            if (!isShown && _isDefaultLoaded)
            {
                if (_defaultRewarded.IsAdReady())
                {
                    _defaultRewarded.ShowAd();
                }
                _isDefaultLoaded = false;
                _defaultRewarded = null;
            }
            
            UpdateShowButton();
        }
        
        private void OnAdDisplayFailed(LevelPlayAdDisplayInfoError error)
        {
            SetStatus($"OnAdDisplayFailed {error}");
        }
        
        private void OnAdDisplayed(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdDisplayed {info}");
        }
        
        private void OnAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
        {
            SetStatus($"OnAdRewarded {info}: {reward}");
        }
        
        private void OnAdClicked(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdClicked {info}");
        }
        
        private void OnAdInfoChanged(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdInfoChanged {info}");
        }
        
        private void OnAdClosed(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdClosed {info}");
            
            // start new load cycle
            if (_load.isOn)
            {
                //StartLoading();   
            }
        }
        
        private void SetStatus(string status)
        {
            _status.text = status;
            Debug.Log(status);
        }

        private void AddDemoGameEventExample()
        {
            var category = (ResourceCategory) Random.Range(0, 9);
            var method = (SpendMethod)Random.Range(0, 8);
            var value = Random.Range(0, 101);
            Adapter.Record(new SpendEvent(category) { _method = method, _name = $"spend_{category} {method} {value}", _value = value });
        }
        
        private void UpdateShowButton()
        {
            _show.interactable = _isDynamicLoaded || _isDefaultLoaded;
        }
    }
}