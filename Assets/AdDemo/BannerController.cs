using System.Collections.Generic;
using com.unity3d.mediation;
using Nefta;
using Nefta.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class BannerController : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Button _show;
        [SerializeField] private Button _hide;
        [SerializeField] private Text _status;
        
        private LevelPlayBannerAd _banner;

        private bool IsNoFill(int errorCode)
        {
            return errorCode == 509 || errorCode == 606 || errorCode == 706 || errorCode == 1058 || errorCode == 1158;
        }

        private double _bidFloor;
        private double _calculatedBidFloor;
        private int _bidNoFillCount;

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

        public void SetInsights(Dictionary<string, Insight> insights)
        {
            var pred_total_value = insights["pred_total_value"]._float;
            var pred_ecpm_banner = insights["pred_ecpm_banner"]._float;
            var user_value_spread = pred_total_value - pred_ecpm_banner;
                
            if (user_value_spread > 0)
            {
                _calculatedBidFloor = pred_ecpm_banner + user_value_spread;
                _bidFloor = _calculatedBidFloor;
                
                var configuration = WaterfallConfiguration.Builder().SetCeiling(_bidFloor).Build();
                IronSource.Agent.SetWaterfallConfiguration(configuration, AdFormat.Banner);
            }
        }
        
        private void OnShowClick()
        {
            LoadAd();
            
            _hide.interactable = true;
        }

        private void LoadAd()
        {
            string adUnitId = "vpkt794d6ruyfwr4";
#if UNITY_IOS
            adUnitId = "4gyff1ux8ch1qz7y";
#endif
            
            _banner = new LevelPlayBannerAd(adUnitId, LevelPlayAdSize.BANNER, LevelPlayBannerPosition.TopCenter, null, true, true);
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
            SetStatus($"OnAdLoadFailed {error}");
            
            Adapter.OnExternalMediationRequestFailed(Adapter.AdType.Banner, _bidFloor, _calculatedBidFloor, error);
            
            if (IsNoFill(error.ErrorCode) && _bidFloor > 0)
            {
                _bidNoFillCount++;
            
                if (_bidNoFillCount == 1)
                {
                    _bidFloor *= 0.95;
                    var configuration = WaterfallConfiguration.Builder().SetCeiling(_bidFloor * 1.5).SetFloor(_bidFloor).Build();
                    IronSource.Agent.SetWaterfallConfiguration(configuration, AdFormat.Banner);

                    LoadAd();
                }
                else if (_bidNoFillCount == 2)
                {
                    IronSource.Agent.SetWaterfallConfiguration(WaterfallConfiguration.Empty(), AdFormat.Banner);

                    LoadAd();
                }   
            }
        }
        
        private void OnHideClick()
        {
            _hide.interactable = false;
            _banner.DestroyAd();
        }
        
        private void OnAdLoaded(LevelPlayAdInfo adInfo)
        {
            SetStatus($"OnAdLoaded {adInfo}");

            _bidNoFillCount = 0;
            Adapter.OnExternalMediationRequestLoaded(Adapter.AdType.Banner, _bidFloor, _calculatedBidFloor, adInfo);
            // might try increase the bid again
            // if (_bidFloor < _calculatedBidFloor)
            // {
            //    _bidFloor = _calculatedBidFloor;
            //}
        }

        private void OnAdClicked(LevelPlayAdInfo adInfo)
        {
            var type = (Type) Random.Range(0, 7);
            var status = (Status)Random.Range(0, 3);
            var source = (Source)Random.Range(0, 7);
            var value = Random.Range(0, 101);
            Adapter.Record(new ProgressionEvent(type, status)
                { _source = source, _name = $"progression_{type}_{status} {source} {value}", _value = value });
            
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
    }
}