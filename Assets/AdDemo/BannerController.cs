using System.Collections;
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
        
        [SerializeField] private Text _title;
        [SerializeField] private Button _show;
        [SerializeField] private Button _hide;
        [SerializeField] private Text _status;
        
        private LevelPlayBannerAd _banner;
        private double _requestedFloorPrice;
        private AdInsight _usedInsight;
        
        private void GetInsightsAndLoad()
        {
            Adapter.GetInsights(Insights.Banner, Load, 5);
        }
        
        private void Load(Insights insights)
        {
            _requestedFloorPrice = 0f;
            _usedInsight = insights._banner;
            if (_usedInsight != null) {
                _requestedFloorPrice = _usedInsight._floorPrice;
            }
            
            SetStatus($"Loading Banner with floor: {_requestedFloorPrice}");
            var config = new LevelPlayBannerAd.Config.Builder()
                .SetPosition(com.unity3d.mediation.LevelPlayBannerPosition.TopCenter)
                .SetSize(com.unity3d.mediation.LevelPlayAdSize.BANNER)
                .SetBidFloor(_requestedFloorPrice)
                .Build();
            _banner = new LevelPlayBannerAd(AdUnitId, config);
            _banner.OnAdLoaded += OnAdLoaded;
            _banner.OnAdLoadFailed += OnAdLoadFailed;
            _banner.OnAdDisplayed += OnAdDisplayed;
            _banner.OnAdDisplayFailed += OnAdDisplayFailed;
            _banner.OnAdClicked += OnAdClicked;
            _banner.OnAdExpanded += OnAdExpanded;
            _banner.OnAdCollapsed += OnAdCollapsed;
            _banner.OnAdLeftApplication += OnAdLeftApplication;
            _banner.LoadAd();
        }
        
        private void OnAdLoadFailed(LevelPlayAdError error)
        {
            Adapter.OnExternalMediationRequestFailed(Adapter.AdType.Banner, _usedInsight, _requestedFloorPrice, error);
            
            SetStatus($"OnAdLoadFailed {error}");
            
            StartCoroutine(ReTryLoad());
        }
        
        private void OnAdLoaded(LevelPlayAdInfo adInfo)
        {
            Adapter.OnExternalMediationRequestLoaded(Adapter.AdType.Banner, _usedInsight, _requestedFloorPrice, adInfo);
            
            SetStatus($"OnAdLoaded {adInfo}");
        }
        
        private IEnumerator ReTryLoad()
        {
            yield return new WaitForSeconds(5f);
            
            GetInsightsAndLoad();
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
            
            SetStatus("Loading...");
            
            _show.interactable = false;
            _hide.interactable = true;

            AddDemoGameEventExample();
        }
        
        private void OnHideClick()
        {
            _banner.DestroyAd();
            
            _show.interactable = true;
            _hide.interactable = false;
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
            var source = (Source)Random.Range(1, 7);
            var value = Random.Range(0, 101);
            Adapter.Record(new ProgressionEvent(type, status)
                { _source = source, _name = $"progression_{type}_{status} {source} {value}", _value = value });
        }
    }
}