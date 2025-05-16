using System.Collections;
using System.Collections.Generic;
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
        string AdUnitId = "doucurq8qtlnuz7p";
#else
        string AdUnitId = "kftiv52431x91zuk";
#endif
        private const string FloorPriceInsightName = "calculated_user_floor_price_rewarded";
        
        [SerializeField] private Text _title;
        [SerializeField] private Button _load;
        [SerializeField] private Button _show;
        [SerializeField] private Text _status;
        
        private LevelPlayRewardedAd _rewarded;
        
        private bool _isLoadRequested;
        private double _bidFloor;
        private double _calculatedBidFloor;
        
        private void GetInsightsAndLoad()
        {
            _isLoadRequested = true;
            
            Adapter.GetBehaviourInsight(new string[] { FloorPriceInsightName }, OnBehaviourInsight);
            
            StartCoroutine(LoadFallback());
        }
        
        private void OnBehaviourInsight(Dictionary<string, Insight> insights)
        {
            _calculatedBidFloor = 0f;
            if (insights.TryGetValue(FloorPriceInsightName, out var insight)) {
                _calculatedBidFloor = insight._float;
            }
            
            Debug.Log($"OnBehaviourInsight for Rewarded calculated bid floor: {_calculatedBidFloor}");
            
            if (_isLoadRequested)
            {
                Load();
            }
        }

        private void Load()
        {
            _isLoadRequested = false;
            
            if (_calculatedBidFloor == 0)
            {
                IronSource.Agent.SetWaterfallConfiguration(WaterfallConfiguration.Empty(), AdFormat.RewardedVideo);
            }
            else
            {
                var configuration = WaterfallConfiguration.Builder()
                    .SetFloor(_bidFloor)
                    .SetCeiling(_bidFloor + 200) // when using SetFloor, SetCeiling has to be used as well
                    .Build();
                IronSource.Agent.SetWaterfallConfiguration(configuration, AdFormat.RewardedVideo);   
            }
            
            _rewarded = new LevelPlayRewardedAd(AdUnitId);
            _rewarded.OnAdLoaded += OnAdLoaded;
            _rewarded.OnAdLoadFailed += OnAdLoadFailed;
            _rewarded.OnAdDisplayed += OnAdDisplayed;
            _rewarded.OnAdDisplayFailed += OnAdDisplayFailed;
            _rewarded.OnAdRewarded += OnAdRewarded;
            _rewarded.OnAdClicked += OnAdClicked;
            _rewarded.OnAdInfoChanged += OnAdInfoChanged;
            _rewarded.OnAdClosed += OnAdClosed;
            _rewarded.LoadAd();
            
            SetStatus($"Loading Rewarded calculatedFloor: {_calculatedBidFloor}");
        }
        
        private void OnAdLoadFailed(LevelPlayAdError error)
        {
            Adapter.OnExternalMediationRequestFailed(Adapter.AdType.Rewarded, _bidFloor, _calculatedBidFloor, error);
            
            SetStatus($"OnAdLoadFailed {error}");
            
            // or automatically retry with a delay 
            //StartCoroutine(ReTryLoad());
        }
        
        private void OnAdLoaded(LevelPlayAdInfo info)
        {
            Adapter.OnExternalMediationRequestLoaded(Adapter.AdType.Rewarded, _bidFloor, _calculatedBidFloor, info);
            
            SetStatus($"OnAdLoaded {info}");
            _show.interactable = true;
        }
        
        public void Init()
        {
            _load.onClick.AddListener(OnLoadClick);
            _show.onClick.AddListener(OnShowClick);

            _load.interactable = false;
            _show.interactable = false;
        }
        
        public void OnReady()
        {
            _load.interactable = true;
        }
        
        private void OnLoadClick()
        {
            GetInsightsAndLoad();

            AddDemoGameEventExample();
        }
        
        private void OnShowClick()
        {
            _show.interactable = false;
            if (_rewarded.IsAdReady())
            {
                SetStatus("Showing rewarded");
                _rewarded.ShowAd();
            }
            else
            {
                SetStatus("Rewarded not ready");
            }
        }
        
        private IEnumerator LoadFallback()
        {
            yield return new WaitForSeconds(5f);

            if (_isLoadRequested)
            {
                _calculatedBidFloor = 0;
                Load();
            }
        }

        private IEnumerator ReTryLoad()
        {
            yield return new WaitForSeconds(5f);
            
            GetInsightsAndLoad();
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
    }
}