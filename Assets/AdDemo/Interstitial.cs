using System.Threading.Tasks;
using Nefta;
using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class Interstitial : MonoBehaviour
    {
#if UNITY_IOS
        private const string AdUnitA = "g7xalw41x4i1bj5t";
        private const string AdUnitB = "q0z1act0tdckh4mg";
#else
        private const string AdUnitA = "0u6jgm23ggqso85n";
        private const string AdUnitB = "wrzl86if1sqfxquc";
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
            public LevelPlayInterstitialAd Interstitial;
            public State State;
            public AdInsight Insight;
            public double Revenue;

            public Track(string adUnitId)
            {
                AdUnitId = adUnitId;
            }

            public void Load()
            {
                Interstitial.OnAdLoaded += OnAdLoaded;
                Interstitial.OnAdLoadFailed += OnAdLoadFailed;
                Interstitial.OnAdDisplayed += OnAdDisplayed;
                Interstitial.OnAdDisplayFailed += OnAdDisplayFailed;
                Interstitial.OnAdClicked += OnAdClicked;
                Interstitial.OnAdInfoChanged += OnAdInfoChanged;
                Interstitial.OnAdClosed += OnAdClosed;
                Interstitial.LoadAd();
            }
            
            private void OnAdLoadFailed(LevelPlayAdError error)
            {
                Adapter.OnExternalMediationRequestFailed(error);

                Instance.SetStatus($"OnAdLoadFailed {AdUnitId}: {error}");

                Interstitial = null;
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

        public static Interstitial Instance;
        
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
            
            Adapter.GetInsights(Insights.Interstitial, track.Insight, (Insights insights) =>
            {
                SetStatus($"LoadWithInsights: {insights}");
                if (insights._interstitial != null)
                {
                    track.Insight = insights._interstitial;
                    var config = new LevelPlayInterstitialAd.Config.Builder()
                        .SetBidFloor(track.Insight._floorPrice)
                        .Build();
                    track.Interstitial = new LevelPlayInterstitialAd(track.AdUnitId, config);
                    
                    Adapter.OnExternalMediationRequest(track.Interstitial, track.Insight);
                    
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
            
            SetStatus($"Loading {track.AdUnitId} as Default");

            track.Interstitial = new LevelPlayInterstitialAd(track.AdUnitId);
            
            Adapter.OnExternalMediationRequest(track.Interstitial);
            
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
            if (request.Interstitial.IsAdReady())
            {
                request.State = State.Shown;
                request.Interstitial.ShowAd();
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
            Debug.Log($"NeftaPluginIS Interstitial {status}");
        }
        
        private void UpdateShowButton()
        {
            _show.interactable = _trackA.State == State.Ready || _trackB.State == State.Ready;
        }
    }
}