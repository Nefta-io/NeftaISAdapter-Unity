using System.Threading.Tasks;
using Nefta;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace AdDemo
{
    public class RewardedOptimized : IRewarded
    {
        private enum State
        {
            Idle,
            LoadingWithInsights,
            Loading,
            Ready,
            Shown
        }

        private class Track
        {
            public readonly string AdUnitId;
            public LevelPlayRewardedAd Rewarded;
            public State State;
            public AdInsight Insight;
            public double Revenue;

            public Track(string adUnitId)
            {
                AdUnitId = adUnitId;
            }
            
            public void Load()
            {
                Rewarded.OnAdLoaded += OnAdLoaded;
                Rewarded.OnAdLoadFailed += OnAdLoadFailed;
                Rewarded.OnAdDisplayed += OnAdDisplayed;
                Rewarded.OnAdDisplayFailed += OnAdDisplayFailed;
                Rewarded.OnAdClicked += OnAdClicked;
                Rewarded.OnAdRewarded += OnAdRewarded;
                Rewarded.OnAdInfoChanged += OnAdInfoChanged;
                Rewarded.OnAdClosed += OnAdClosed;
                Rewarded.LoadAd();
            }
            
            private void OnAdLoadFailed(LevelPlayAdError error)
            {
                Adapter.OnExternalMediationRequestFailed(error);

                Instance._ui.SetStatus($"OnAdLoadFailed {AdUnitId}: {error}");

                Rewarded = null;
                RestartAfterFailedLoad();
            }

            public void RestartAfterFailedLoad()
            {
                _ = RetryLoadWithDelay();

                Instance.OnTrackLoad(false);
            }
        
            private async Task RetryLoadWithDelay()
            {
                await Task.Delay((int)(Adapter.GetRetryDelayInSeconds(Insight) * 1000));
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    return;
                }
#endif
                State = State.Idle;
                Instance.RetryLoadTracks();
            }
            
            private void OnAdLoaded(LevelPlayAdInfo info)
            {
                Adapter.OnExternalMediationRequestLoaded(info);
                
                Instance._ui.SetStatus($"Loaded {AdUnitId} at: {info.Revenue}");

                Insight = null;
                Revenue = info.Revenue ?? 0;
                State = State.Ready;

                Instance.OnTrackLoad(true);
            }
        
            private void OnAdClicked(LevelPlayAdInfo info)
            {
                Adapter.OnLevelPlayClick(info);
            
                Instance._ui.SetStatus($"OnAdClicked {info}");
            }
            
            private void OnAdDisplayFailed(LevelPlayAdDisplayInfoError error)
            {
                Instance._ui.SetStatus($"OnAdDisplayFailed {error}");
                
                State = State.Idle;
                Instance.RetryLoadTracks();
            }
            
            private void OnAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
            {
                Instance._ui.SetStatus($"OnAdRewarded {info}: {reward}");
            }

            private void OnAdDisplayed(LevelPlayAdInfo info)
            {
                Instance._ui.SetStatus($"OnAdDisplayed {info}");
            }

            private void OnAdInfoChanged(LevelPlayAdInfo info)
            {
                Instance._ui.SetStatus($"OnAdInfoChanged {info}");
                Revenue = info.Revenue ?? 0;
            }
        
            private void OnAdClosed(LevelPlayAdInfo info)
            {
                Instance._ui.SetStatus($"OnAdClosed {info}");

                State = State.Idle;
                Instance.RetryLoadTracks();
            }
        }

        private Track _trackA;
        private Track _trackB;
        private bool _isFirstResponseReceived;

        private RewardedUi _ui;
        public static RewardedOptimized Instance;

        public void Init(RewardedUi ui)
        {
            _ui = ui;
            
            Instance = this;
            
            _trackA = new Track(RewardedUi.AdUnitA);
            _trackB = new Track(RewardedUi.AdUnitB);
        }
        
        public void Load()
        {
            LoadTrack(_trackA, _trackB.State);
            LoadTrack(_trackB, _trackA.State);
        }

        private void LoadTrack(Track track, State otherState)
        {
            if (track.State == State.Idle)
            {
                if (otherState == State.LoadingWithInsights || otherState == State.Shown)
                {
                    if (_isFirstResponseReceived)
                    {
                        LoadDefault(track);
                    }
                }
                else
                {
                    GetInsightsAndLoad(track); 
                }
            }
        }
        
        private void GetInsightsAndLoad(Track track)
        {
            track.State = State.LoadingWithInsights;
            
            Adapter.GetInsights(Insights.Rewarded, track.Insight, (Insights insights) =>
            {
                _ui.SetStatus($"LoadWithInsights: {insights}");
                if (insights._rewarded != null)
                {
                    track.Insight = insights._rewarded;
                    if (track.Insight._floorPrice >= 0)
                    {
                        var config = new LevelPlayRewardedAd.Config.Builder()
                            .SetBidFloor(track.Insight._floorPrice)
                            .Build();
                        track.Rewarded = new LevelPlayRewardedAd(track.AdUnitId, config);   
                    }
                    else
                    {
                        track.Rewarded = new LevelPlayRewardedAd(track.AdUnitId);   
                    }
                    
                    Adapter.OnExternalMediationRequest(track.Rewarded, track.Insight);
                    
                    track.Load();
                }
                else
                {
                    track.RestartAfterFailedLoad();
                }
            });
        }
        
        private void LoadDefault(Track track)
        {
            track.State = State.Loading;
            
            _ui.SetStatus($"Loading {track.AdUnitId} as Default");

            track.Rewarded = new LevelPlayRewardedAd(track.AdUnitId);
            
            Adapter.OnExternalMediationRequest(track.Rewarded);
            
            track.Load();
        }
        
        public void Show()
        {
            var isShown = false;
            if (_trackA.State == State.Ready)
            {
                if (_trackB.State == State.Ready && _trackB.Revenue > _trackA.Revenue)
                {
                    isShown = TryShow(_trackB);
                }
                if (!isShown)
                {
                    isShown = TryShow(_trackA);
                }
            }
            if (!isShown && _trackB.State == State.Ready)
            {
                TryShow(_trackB);
            }
            _ui.SetAvailability(_trackA.State == State.Ready || _trackB.State == State.Ready);
        }

        private bool TryShow(Track track)
        {
            track.Revenue = -1;
            if (track.Rewarded.IsAdReady())
            {
                track.State = State.Shown;
                track.Rewarded.ShowAd();
                return true;
            }
            track.State = State.Idle;
            RetryLoadTracks();
            return false;
        }
        
        public void RetryLoadTracks()
        {
            if (_ui.IsAutoLoad)
            {
                Load();
            }
        }

        public void OnTrackLoad(bool success)
        {
            if (success)
            {
                _ui.SetAvailability(true);
            }

            _isFirstResponseReceived = true;
            RetryLoadTracks();
        }
    }
}