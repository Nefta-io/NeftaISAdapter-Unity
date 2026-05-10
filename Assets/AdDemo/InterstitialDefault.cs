using System.Threading.Tasks;
using Nefta;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace AdDemo
{
    public class InterstitialDefault : IInterstitial
    {
        private InterstitialUi _ui;
        private LevelPlayInterstitialAd _interstitial;
        
        public void Init(InterstitialUi ui)
        {
            _ui = ui;
            
            _interstitial = new LevelPlayInterstitialAd(InterstitialUi.AdUnitA);
            _interstitial.OnAdLoaded += OnAdLoaded;
            _interstitial.OnAdLoadFailed += OnAdLoadFailed;
            _interstitial.OnAdDisplayed += OnAdDisplayed;
            _interstitial.OnAdDisplayFailed += OnAdDisplayFailed;
            _interstitial.OnAdClicked += OnAdClicked;
            _interstitial.OnAdClosed += OnAdClosed;
        }

        public void Load()
        {
            Adapter.OnExternalMediationRequest(_interstitial);
            
            _interstitial.LoadAd();
        }

        public void Show()
        {
            if (_interstitial.IsAdReady())
            {
                _interstitial.ShowAd();
            }
            else if (_ui.IsAutoLoad)
            {
                Load();
            }
            _ui.SetAvailability(false);
        }
        
        private void OnAdLoadFailed(LevelPlayAdError error)
        {
            Adapter.OnExternalMediationRequestFailed(error);

            _ui.SetStatus($"OnAdLoadFailed {error.AdUnitId}: {error}");

            _ = LoadWithDelay();
        }
        
        private void OnAdLoaded(LevelPlayAdInfo info)
        {
            Adapter.OnExternalMediationRequestLoaded(info);
                
            _ui.SetStatus($"Loaded {info.AdUnitId} at: {info.Revenue}");

            _ui.SetAvailability(true);
        }
        
        private void OnAdClicked(LevelPlayAdInfo info)
        {
            Adapter.OnLevelPlayClick(info);
            
            _ui.SetStatus($"OnAdClicked {info}");
        }
            
        private void OnAdDisplayFailed(LevelPlayAdDisplayInfoError error)
        {
            _ui.SetStatus($"OnAdDisplayFailed {error}");

            if (_ui.IsAutoLoad)
            {
                Load();
            }
        }

        private void OnAdDisplayed(LevelPlayAdInfo info)
        {
            _ui.SetStatus($"OnAdDisplayed {info}");
        }
        
        private void OnAdClosed(LevelPlayAdInfo info)
        {
            _ui.SetStatus($"OnAdClosed {info}");

            if (_ui.IsAutoLoad)
            {
                Load();
            }
        }
        
        private async Task LoadWithDelay()
        {
            await Task.Delay(5000);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            Load();
        }
    }
}