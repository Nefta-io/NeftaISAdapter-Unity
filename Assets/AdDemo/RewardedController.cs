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
        private const string DynamicAdUnitId = "p3dh8r1mm3ua8fvv";
        private const string DefaultAdUnitId = "doucurq8qtlnuz7p";
#else
        private const string DynamicAdUnitId = "x3helvrx8elhig4z";
        private const string DefaultAdUnitId = "kftiv52431x91zuk";
#endif
        
        private LevelPlayRewardedAd _dynamicRewarded;
        private double _dynamicAdRevenue;
        private AdInsight _dynamicInsight;
        private LevelPlayRewardedAd _defaultRewarded;
        private double _defaultAdRevenue;
        
        [SerializeField] private Text _title;
        [SerializeField] private Toggle _load;
        [SerializeField] private Text _loadText;
        [SerializeField] private Button _show;
        [SerializeField] private Text _status;
        
        private void StartLoading()
        {
            if (_dynamicRewarded == null)
            {
                GetInsightsAndLoad(null);   
            }
            if (_defaultRewarded == null)
            {
                LoadDefault();   
            }
        }
        
        private void GetInsightsAndLoad(AdInsight previousInsight)
        {
            Adapter.GetInsights(Insights.Rewarded, previousInsight, LoadWithInsights, 5);
        }
        
        private void LoadWithInsights(Insights insights)
        {
            _dynamicInsight = insights._rewarded;
            if (_dynamicInsight != null) {
                SetStatus($"Loading Dynamic Rewarded with: {_dynamicInsight}");
                
                var config = new LevelPlayRewardedAd.Config.Builder()
                    .SetBidFloor(_dynamicInsight._floorPrice)
                    .Build();
                _dynamicRewarded = new LevelPlayRewardedAd(DynamicAdUnitId, config);
                Load(_dynamicRewarded);
                
                Adapter.OnExternalMediationRequest(_dynamicRewarded, _dynamicInsight);
            }
        }

        private void LoadDefault()
        {
            SetStatus($"Loading Default Rewarded {DefaultAdUnitId}");
            
            _defaultRewarded = new LevelPlayRewardedAd(DefaultAdUnitId);
            Load(_defaultRewarded);
            
            Adapter.OnExternalMediationRequest(_defaultRewarded);
        }
        
        private void OnAdLoadFailed(LevelPlayAdError error)
        {
            Adapter.OnExternalMediationRequestFailed(error);

            if (_dynamicRewarded != null && error.AdId == _dynamicRewarded.GetAdId())
            {
                SetStatus($"OnAdLoadFailed Dynamic {error}");

                if (_load.isOn)
                {
                    StartCoroutine(ReTryLoad(true));
                }
                else
                {
                    _dynamicRewarded = null; 
                }
            }
            else
            {
                SetStatus($"OnAdLoadFailed Default {error}");

                if (_load.isOn)
                {
                    StartCoroutine(ReTryLoad(false));
                }
                else
                {
                    _defaultRewarded = null; 
                }
            }
        }
        
        private void OnAdLoaded(LevelPlayAdInfo info)
        {
            Adapter.OnExternalMediationRequestLoaded(info);
            
            if (_dynamicRewarded != null && info.AdId == _dynamicRewarded.GetAdId())
            {
                SetStatus($"OnAdLoaded Dynamic {info}");

                _dynamicAdRevenue = info.Revenue ?? 0;
            }
            else
            {
                SetStatus($"OnAdLoaded Default {info}");

                _defaultAdRevenue = info.Revenue ?? 0;
            }
            
            UpdateShowButton();
        }
        
        private IEnumerator ReTryLoad(bool isDynamic)
        {
            yield return new WaitForSeconds(5f);

            if (_load.isOn)
            {
                if (isDynamic)
                {
                    GetInsightsAndLoad(_dynamicInsight);   
                }
                else
                {
                    LoadDefault();
                }
            }
        }
        
        private void OnAdClicked(LevelPlayAdInfo info)
        {
            Adapter.OnLevelPlayClick(info);
            
            SetStatus($"OnAdClicked {info}");
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
            if (_dynamicAdRevenue >= 0)
            {
                if (_defaultAdRevenue > _dynamicAdRevenue)
                {
                    isShown = TryShowDefault();
                }
                if (!isShown)
                {
                    isShown = TryShowDynamic();
                }
            }
            if (!isShown && _defaultAdRevenue >= 0)
            {
                TryShowDefault();
            }
            UpdateShowButton();
        }

        private bool TryShowDynamic()
        {
            var isShown = false;
            if (_dynamicRewarded.IsAdReady())
            {
                _dynamicRewarded.ShowAd();
                isShown = true;
            }
            _dynamicAdRevenue = -1;
            _dynamicRewarded = null;
            return isShown;
        }

        private bool TryShowDefault()
        {
            var isShown = false;
            if (_defaultRewarded.IsAdReady())
            {
                _defaultRewarded.ShowAd();
                isShown = true;
            }
            _defaultAdRevenue = -1;
            _defaultRewarded = null;
            return isShown;
        }
        
        private void Load(LevelPlayRewardedAd rewarded)
        {
            rewarded.OnAdLoaded += OnAdLoaded;
            rewarded.OnAdLoadFailed += OnAdLoadFailed;
            rewarded.OnAdDisplayed += OnAdDisplayed;
            rewarded.OnAdDisplayFailed += OnAdDisplayFailed;
            rewarded.OnAdRewarded += OnAdDynamicRewarded;
            rewarded.OnAdClicked += OnAdClicked;
            rewarded.OnAdInfoChanged += OnAdInfoChanged;
            rewarded.OnAdClosed += OnAdClosed;
            rewarded.LoadAd();
        }
        
        private void OnAdDisplayFailed(LevelPlayAdDisplayInfoError error)
        {
            SetStatus($"OnAdDisplayFailed {error}");
        }
        
        private void OnAdDisplayed(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdDisplayed {info}");
        }
        
        private void OnAdDynamicRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
        {
            SetStatus($"OnAdRewarded {info}: {reward}");
        }
        
        private void OnAdInfoChanged(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdInfoChanged {info}");
        }
        
        private void OnAdClosed(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdClosed {info}");
            
            // start new cycle
            if (_load.isOn)
            {
                StartLoading();
            }
        }
        
        private void SetStatus(string status)
        {
            _status.text = status;
            Debug.Log($"NeftaPluginIS Rewarded {status}");
        }

        private void UpdateShowButton()
        {
            _show.interactable = _dynamicAdRevenue >= 0 || _defaultAdRevenue >= 0;
        }

        private void AddDemoGameEventExample()
        {
            var category = (ResourceCategory) Random.Range(0, 9);
            var method = (SpendMethod)Random.Range(0, 8);
            var value = Random.Range(0, 101);
            Adapter.Record(new SpendEvent(category) { _method = method, _name = $"spend_{category} {method} {value}", _value = value });
        }
    }
}