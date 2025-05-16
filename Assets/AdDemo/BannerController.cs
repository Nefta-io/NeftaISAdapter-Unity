using System.Collections;
using System.Collections.Generic;
using Nefta;
using Nefta.Events;
using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class BannerController : MonoBehaviour
    {
#if UNITY_IOS
        string AdUnitId = "4gyff1ux8ch1qz7y";
#else
        string AdUnitId = "vpkt794d6ruyfwr4";
#endif
        private const string FloorPriceInsightName = "calculated_user_floor_price_banner";
        
        [SerializeField] private Text _title;
        [SerializeField] private Button _show;
        [SerializeField] private Button _hide;
        [SerializeField] private Text _status;
        
        private LevelPlayBannerAd _banner;

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
            
            Debug.Log($"OnBehaviourInsight for Banner calculated bid floor: {_calculatedBidFloor}");
            
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
                IronSource.Agent.SetWaterfallConfiguration(WaterfallConfiguration.Empty(), AdFormat.Banner);
            }
            else
            {
                var configuration = WaterfallConfiguration.Builder()
                    .SetFloor(_bidFloor)
                    .SetCeiling(_bidFloor + 200) // when using SetFloor, SetCeiling has to be used as well
                    .Build();
                IronSource.Agent.SetWaterfallConfiguration(configuration, AdFormat.Banner);   
            }
            
            _banner = new LevelPlayBannerAd(AdUnitId, com.unity3d.mediation.LevelPlayAdSize.BANNER, com.unity3d.mediation.LevelPlayBannerPosition.TopCenter, null, true, true);
            _banner.OnAdLoaded += OnAdLoaded;
            _banner.OnAdLoadFailed += OnAdLoadFailed;
            _banner.OnAdDisplayed += OnAdDisplayed;
            _banner.OnAdDisplayFailed += OnAdDisplayFailed;
            _banner.OnAdClicked += OnAdClicked;
            _banner.OnAdExpanded += OnAdExpanded;
            _banner.OnAdCollapsed += OnAdCollapsed;
            _banner.OnAdLeftApplication += OnAdLeftApplication;
            _banner.LoadAd();
            
            SetStatus($"Loading Banner calculatedFloor: {_calculatedBidFloor}");
        }
        
        private void OnAdLoadFailed(LevelPlayAdError error)
        {
            Adapter.OnExternalMediationRequestFailed(Adapter.AdType.Banner, _bidFloor, _calculatedBidFloor, error);
            
            SetStatus($"OnAdLoadFailed {error}");
            
            StartCoroutine(ReTryLoad());
        }
        
        private void OnAdLoaded(LevelPlayAdInfo adInfo)
        {
            Adapter.OnExternalMediationRequestLoaded(Adapter.AdType.Banner, _bidFloor, _calculatedBidFloor, adInfo);
            
            SetStatus($"OnAdLoaded {adInfo}");
        }

        public void Init()
        {
            _title.text = "Banner info";
            _show.onClick.AddListener(OnShowClick);
            _hide.onClick.AddListener(OnHideClick);

            _show.interactable = false;
            _hide.interactable = false;
        }

        public void OnReady()
        {
            _show.interactable = true;
        }
        
        private void OnShowClick()
        {
            GetInsightsAndLoad();
            
            _hide.interactable = true;

            AddDemoGameEventExample();
        }
        
        private void OnHideClick()
        {
            _hide.interactable = false;
            _banner.DestroyAd();
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

        private void OnAdClicked(LevelPlayAdInfo adInfo)
        {
            SetStatus($"OnAdClicked {adInfo}");
        }

        void OnAdDisplayed(LevelPlayAdInfo adInfo)
        {
            SetStatus($"OnAdDisplayed {adInfo}");
        }

        void OnAdDisplayFailed(LevelPlayAdDisplayInfoError error)
        {
            SetStatus($"OnAdDisplayFailed {error}");
        }
        
        void OnAdExpanded(LevelPlayAdInfo adInfo)
        {
            SetStatus($"OnAdDisplayed {adInfo}");
        }
        
        void OnAdCollapsed(LevelPlayAdInfo adInfo)
        {
            SetStatus($"OnAdCollapsed {adInfo}");
        }
        
        void OnAdLeftApplication(LevelPlayAdInfo adInfo)
        {
            SetStatus($"OnAdLeftApplication {adInfo}");
        }
        
        private void SetStatus(string status)
        {
            _status.text = status;
            Debug.Log(status);
        }

        private void AddDemoGameEventExample()
        {
            var type = (Type) Random.Range(0, 7);
            var status = (Status)Random.Range(0, 3);
            var source = (Source)Random.Range(0, 7);
            var value = Random.Range(0, 101);
            Adapter.Record(new ProgressionEvent(type, status)
                { _source = source, _name = $"progression_{type}_{status} {source} {value}", _value = value });
        }
    }
}