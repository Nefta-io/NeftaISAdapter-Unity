using System.Threading.Tasks;
using Nefta;
using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class Rewarded : MonoBehaviour
    {
#if UNITY_IOS
        private const string AdUnitA = "p3dh8r1mm3ua8fvv";
        private const string AdUnitB = "doucurq8qtlnuz7p";
#else
        private const string AdUnitA = "x3helvrx8elhig4z";
        private const string AdUnitB = "kftiv52431x91zuk";
#endif
        
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

                Instance.SetStatus($"OnAdLoadFailed {AdUnitId}: {error}");

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
                await Task.Delay(5000);
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
                
                Instance.SetStatus($"Loaded {AdUnitId} at: {info.Revenue}");

                Insight = null;
                Revenue = info.Revenue ?? 0;
                State = State.Ready;

                Instance.OnTrackLoad(true);
            }
        
            private void OnAdClicked(LevelPlayAdInfo info)
            {
                Adapter.OnLevelPlayClick(info);
            
                Instance.SetStatus($"OnAdClicked {info}");
            }
            
            private void OnAdDisplayFailed(LevelPlayAdDisplayInfoError error)
            {
                Instance.SetStatus($"OnAdDisplayFailed {error}");
                
                State = State.Idle;
                Instance.RetryLoadTracks();
            }
            
            private void OnAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
            {
                Instance.SetStatus($"OnAdRewarded {info}: {reward}");
            }

            private void OnAdDisplayed(LevelPlayAdInfo info)
            {
                Instance.SetStatus($"OnAdDisplayed {info}");
            }

            private void OnAdInfoChanged(LevelPlayAdInfo info)
            {
                Instance.SetStatus($"OnAdInfoChanged {info}");
            }
        
            private void OnAdClosed(LevelPlayAdInfo info)
            {
                Instance.SetStatus($"OnAdClosed {info}");

                State = State.Idle;
                Instance.RetryLoadTracks();
            }
        }

        private Track _trackA;
        private Track _trackB;
        private bool _isFirstResponseReceived;
        
        [SerializeField] private Text _title;
        [SerializeField] private Toggle _load;
        [SerializeField] private Text _loadText;
        [SerializeField] private Button _show;
        [SerializeField] private Text _status;
        
        public static Rewarded Instance;
        
        private void LoadTracks()
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
                SetStatus($"LoadWithInsights: {insights}");
                if (insights._rewarded != null)
                {
                    track.Insight = insights._rewarded;
                    var config = new LevelPlayRewardedAd.Config.Builder()
                        .SetBidFloor(track.Insight._floorPrice)
                        .Build();
                    track.Rewarded = new LevelPlayRewardedAd(AdUnitA, config);
                    
                    Adapter.OnExternalMediationRequest(track.Rewarded, track.Insight);
                    
                    track.Load();
                }
                else
                {
                    track.RestartAfterFailedLoad();
                }
            }, 5);
        }
        
        private void LoadDefault(Track track)
        {
            track.State = State.Loading;
            
            SetStatus($"Loading {track.AdUnitId} as Default");

            track.Rewarded = new LevelPlayRewardedAd(track.AdUnitId);
            
            Adapter.OnExternalMediationRequest(track.Rewarded);
            
            track.Load();
        }
        
        private void Awake()
        {
            Instance = this;
            
            _trackA = new Track(AdUnitA);
            _trackB = new Track(AdUnitB);
            
            _load.onValueChanged.AddListener(OnLoadChanged);
            _show.onClick.AddListener(OnShowClick);

            _show.interactable = false;
        }
        
        private void OnLoadChanged(bool isOn)
        {
            if (isOn)
            {
                LoadTracks();   
            }
        }
        
        private void OnShowClick()
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
            UpdateShowButton();
        }

        private bool TryShow(Track request)
        {
            request.Revenue = -1;
            if (request.Rewarded.IsAdReady())
            {
                request.State = State.Shown;
                request.Rewarded.ShowAd();
                return true;
            }
            request.State = State.Idle;
            RetryLoadTracks();
            return false;
        }
        
        public void RetryLoadTracks()
        {
            if (_load.isOn)
            {
                LoadTracks();
            }
        }

        public void OnTrackLoad(bool success)
        {
            if (success)
            {
                UpdateShowButton();
            }

            _isFirstResponseReceived = true;
            RetryLoadTracks();
        }
        
        private void SetStatus(string status)
        {
            _status.text = status;
            Debug.Log($"NeftaPluginIS Rewarded {status}");
        }

        private void UpdateShowButton()
        {
            _show.interactable = _trackA.State == State.Ready || _trackB.State == State.Ready;
        }
    }
}