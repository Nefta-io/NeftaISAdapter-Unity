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
        string AdUnitId = "doucurq8qtlnuz7p";
#else
        string AdUnitId = "kftiv52431x91zuk";
#endif
        
        [SerializeField] private Text _title;
        [SerializeField] private Button _load;
        [SerializeField] private Text _loadText;
        [SerializeField] private Button _show;
        [SerializeField] private Text _status;
        
        private LevelPlayRewardedAd _rewarded;
        private double _requestedFloorPrice;
        private AdInsight _usedInsight;
        private bool _isLoading;
        
        private void GetInsightsAndLoad()
        {
            Adapter.GetInsights(Insights.Rewarded, Load, 5);
        }
        
        private void Load(Insights insights)
        {
            _requestedFloorPrice = 0f;
            _usedInsight = insights._rewarded;
            if (_usedInsight != null) {
                _requestedFloorPrice = _usedInsight._floorPrice;
            }

            SetStatus($"Loading Interstitial with floor: {_requestedFloorPrice}");
            var config = new LevelPlayRewardedAd.Config.Builder()
                .SetBidFloor(_requestedFloorPrice)
                .Build();
            _rewarded = new LevelPlayRewardedAd(AdUnitId, config);
            _rewarded.OnAdLoaded += OnAdLoaded;
            _rewarded.OnAdLoadFailed += OnAdLoadFailed;
            _rewarded.OnAdDisplayed += OnAdDisplayed;
            _rewarded.OnAdDisplayFailed += OnAdDisplayFailed;
            _rewarded.OnAdRewarded += OnAdRewarded;
            _rewarded.OnAdClicked += OnAdClicked;
            _rewarded.OnAdInfoChanged += OnAdInfoChanged;
            _rewarded.OnAdClosed += OnAdClosed;
            _rewarded.LoadAd();
        }
        
        private void OnAdLoadFailed(LevelPlayAdError error)
        {
            Adapter.OnExternalMediationRequestFailed(Adapter.AdType.Rewarded, _usedInsight, _requestedFloorPrice, error);
            
            SetStatus($"OnAdLoadFailed {error}");
            
            StartCoroutine(ReTryLoad());
        }
        
        private void OnAdLoaded(LevelPlayAdInfo info)
        {
            Adapter.OnExternalMediationRequestLoaded(Adapter.AdType.Rewarded, _usedInsight, _requestedFloorPrice, info);
            
            SetStatus($"OnAdLoaded {info}");
            _show.interactable = true;
        }
        
        private IEnumerator ReTryLoad()
        {
            yield return new WaitForSeconds(5f);

            if (_isLoading)
            {
                GetInsightsAndLoad();   
            }
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
            if (_isLoading)
            {
                SetLoadingButton(false);
            }
            else
            {
                SetStatus("GetInsightsAndLoad...");
                GetInsightsAndLoad();
                SetLoadingButton(true);
                AddDemoGameEventExample();
            }
        }
        
        private void OnShowClick()
        {
            if (_rewarded.IsAdReady())
            {
                SetStatus("Showing rewarded");
                _rewarded.ShowAd();
            }
            else
            {
                SetStatus("Rewarded not ready");
            }

            _load.interactable = true;
            _show.interactable = false;
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
        
        private void SetLoadingButton(bool isLoading)
        {
            _isLoading = isLoading;
            if (_isLoading)
            {
                _loadText.text = "Cancel";
            }
            else
            {
                _loadText.text = "Load Rewarded";
            }
        }
    }
}