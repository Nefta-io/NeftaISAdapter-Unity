using System.Collections;
using System.Collections.Generic;
using Nefta;
using Nefta.Events;
using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class InterstitialController : MonoBehaviour
    {
        
#if UNITY_IOS
        string AdUnitId = "q0z1act0tdckh4mg";
#else
        string AdUnitId = "wrzl86if1sqfxquc";
#endif
        private const string FloorPriceInsightName = "calculated_user_floor_price_interstitial";
        
        [SerializeField] private Text _title;
        [SerializeField] private Button _load;
        [SerializeField] private Button _show;
        [SerializeField] private Text _status;

        private LevelPlayInterstitialAd _interstitial;
        
        private bool _isLoadRequested;
        private double _requestedFloorPrice;
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
            
            Debug.Log($"OnBehaviourInsight for Interstitial calculated bid floor: {_calculatedBidFloor}");
            
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
                _requestedFloorPrice = 0;
                IronSource.Agent.SetWaterfallConfiguration(WaterfallConfiguration.Empty(), AdFormat.Interstitial);
            }
            else
            {
                _requestedFloorPrice = _calculatedBidFloor;
                var configuration = WaterfallConfiguration.Builder()
                    .SetFloor(_requestedFloorPrice)
                    .SetCeiling(_requestedFloorPrice + 200) // when using SetFloor, SetCeiling has to be used as well
                    .Build();
                IronSource.Agent.SetWaterfallConfiguration(configuration, AdFormat.Interstitial);   
            }
            
            _interstitial = new LevelPlayInterstitialAd(AdUnitId);
            _interstitial.OnAdLoaded += OnAdLoaded;
            _interstitial.OnAdLoadFailed += OnAdLoadFailed;
            _interstitial.OnAdDisplayed += OnAdDisplayed;
            _interstitial.OnAdDisplayFailed += OnAdDisplayFailed;
            _interstitial.OnAdClicked += OnAdClicked;
            _interstitial.OnAdInfoChanged += OnAdInfoChanged;
            _interstitial.OnAdClosed += OnAdClosed;
            _interstitial.LoadAd();
            
            SetStatus($"Loading Interstitial calculatedFloor: {_calculatedBidFloor}, requested: {_requestedFloorPrice}");
        }
        
        private void OnAdLoadFailed(LevelPlayAdError error)
        {
            Adapter.OnExternalMediationRequestFailed(Adapter.AdType.Interstitial, _requestedFloorPrice, _calculatedBidFloor, error);
            
            SetStatus($"OnAdLoadFailed {error}");
            
            StartCoroutine(ReTryLoad());
        }
        
        private void OnAdLoaded(LevelPlayAdInfo info)
        {
            Adapter.OnExternalMediationRequestLoaded(Adapter.AdType.Interstitial, _requestedFloorPrice, _calculatedBidFloor, info);
            
            SetStatus($"OnAdLoaded {info}");
            _show.interactable = true;
        }
        
        private IEnumerator ReTryLoad()
        {
            yield return new WaitForSeconds(5f);
            
            GetInsightsAndLoad();
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
            if (_interstitial.IsAdReady())
            {
                SetStatus("Showing interstitial");
                _interstitial.ShowAd();
            }
            else
            {
                SetStatus("Interstitial not ready");
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
        
        private void OnAdDisplayFailed(LevelPlayAdDisplayInfoError error)
        {
            SetStatus($"OnAdDisplayFailed {error}");
        }

        private void OnAdDisplayed(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdDisplayed {info}");
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
            var method = (ReceiveMethod)Random.Range(0, 8);
            var value = Random.Range(0, 101);
            Adapter.Record(new ReceiveEvent(category) { _method = method, _name = $"receive_{category} {method} {value}", _value = value });
        }
    }
}