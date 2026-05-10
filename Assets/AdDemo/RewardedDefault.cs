using System.Threading.Tasks;
using Nefta;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace AdDemo
{
    public class RewardedDefault : IRewarded
    {
        private RewardedUi _ui;
        private LevelPlayRewardedAd _rewarded;
        
        public void Init(RewardedUi ui)
        {
            _ui = ui;
            
            _rewarded = new LevelPlayRewardedAd(RewardedUi.AdUnitA);
            _rewarded.OnAdLoaded += OnAdLoaded;
            _rewarded.OnAdLoadFailed += OnAdLoadFailed;
            _rewarded.OnAdDisplayed += OnAdDisplayed;
            _rewarded.OnAdDisplayFailed += OnAdDisplayFailed;
            _rewarded.OnAdClicked += OnAdClicked;
            _rewarded.OnAdRewarded += OnAdRewarded;
            _rewarded.OnAdClosed += OnAdClosed;
        }

        public void Load()
        {
            Adapter.OnExternalMediationRequest(_rewarded);
            
            _rewarded.LoadAd();
        }

        public void Show()
        {
            if (_rewarded.IsAdReady())
            {
                _rewarded.ShowAd();
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
        
        private void OnAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
        {
            _ui.SetStatus($"OnAdRewarded {info}: {reward}");
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