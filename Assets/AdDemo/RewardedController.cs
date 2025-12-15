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
            Ready
        }

        private class AdRequest
        {
            public string AdUnitId;
            public LevelPlayRewardedAd Rewarded;
            public State State;
            public AdInsight Insight;
            public double Revenue;

            public AdRequest(string adUnitId)
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
                AfterLoadFail();
            }

            public void AfterLoadFail()
            {
                Instance.StartCoroutine(ReTryLoad());

                Instance.OnTrackLoad(false);
            }
        
            private IEnumerator ReTryLoad()
            {
                yield return new WaitForSeconds(5f);
               
                State = State.Idle;
                Instance.RetryLoading();
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
                
                Instance.RetryLoading();
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

                Instance.RetryLoading();
            }
        }

        private AdRequest _adRequestA;
        private AdRequest _adRequestB;
        private bool _isFirstResponseReceived;
        
        [SerializeField] private Text _title;
        [SerializeField] private Toggle _load;
        [SerializeField] private Text _loadText;
        [SerializeField] private Button _show;
        [SerializeField] private Text _status;
        
        public static RewardedController Instance;
        
        private void StartLoading()
        {
            Load(_adRequestA, _adRequestB.State);
            Load(_adRequestB, _adRequestA.State);
        }

        private void Load(AdRequest request, State otherState)
        {
            if (request.State == State.Idle)
            {
                if (otherState != State.LoadingWithInsights)
                {
                    GetInsightsAndLoad(request);
                }
                else if (_isFirstResponseReceived)
                {
                    LoadDefault(request);
                }
            }
        }
        
        private void GetInsightsAndLoad(AdRequest adRequest)
        {
            adRequest.State = State.LoadingWithInsights;
            
            Adapter.GetInsights(Insights.Rewarded, adRequest.Insight, (Insights insights) =>
            {
                SetStatus($"LoadWithInsights: {insights}");
                if (insights._rewarded != null)
                {
                    adRequest.Insight = insights._rewarded;
                    var config = new LevelPlayRewardedAd.Config.Builder()
                        .SetBidFloor(adRequest.Insight._floorPrice)
                        .Build();
                    adRequest.Rewarded = new LevelPlayRewardedAd(AdUnitA, config);
                    
                    Adapter.OnExternalMediationRequest(adRequest.Rewarded, adRequest.Insight);
                    
                    adRequest.Load();
                }
                else
                {
                    adRequest.AfterLoadFail();
                }
            }, 5);
        }
        
        private void LoadDefault(AdRequest adRequest)
        {
            adRequest.State = State.Loading;
            
            SetStatus($"Loading {adRequest.AdUnitId} as Default");

            adRequest.Rewarded = new LevelPlayRewardedAd(adRequest.AdUnitId);
            
            Adapter.OnExternalMediationRequest(adRequest.Rewarded);
            
            adRequest.Load();
        }
        
        private void Awake()
        {
            Instance = this;
            
            _adRequestA = new AdRequest(AdUnitA);
            _adRequestA = new AdRequest(AdUnitB);
            
            _load.onValueChanged.AddListener(OnLoadChanged);
            _show.onClick.AddListener(OnShowClick);

            _show.interactable = false;
        }
        
        private void OnLoadChanged(bool isOn)
        {
            if (isOn)
            {
                StartLoading();   
            }
            
            AddDemoGameEventExample();
        }
        
        private void OnShowClick()
        {
            var isShown = false;
            if (_adRequestA.State == State.Ready)
            {
                if (_adRequestB.State == State.Ready && _adRequestB.Revenue > _adRequestA.Revenue)
                {
                    isShown = TryShow(_adRequestB);
                }
                if (!isShown)
                {
                    isShown = TryShow(_adRequestA);
                }
            }
            if (!isShown && _adRequestB.State == State.Ready)
            {
                TryShow(_adRequestB);
            }
            UpdateShowButton();
        }

        private bool TryShow(AdRequest request)
        {
            request.State = State.Idle;
            request.Revenue = -1;

            if (request.Rewarded.IsAdReady())
            {
                request.Rewarded.ShowAd();
                return true;
            }
            RetryLoading();
            return false;
        }
        
        public void RetryLoading()
        {
            if (_load.isOn)
            {
                StartLoading();
            }
        }

        public void OnTrackLoad(bool success)
        {
            if (success)
            {
                UpdateShowButton();
            }

            _isFirstResponseReceived = true;
            RetryLoading();
        }
        
        private void SetStatus(string status)
        {
            _status.text = status;
            Debug.Log($"NeftaPluginIS Rewarded {status}");
        }

        private void UpdateShowButton()
        {
            _show.interactable = _adRequestA.State == State.Ready || _adRequestB.State == State.Ready;
        }

        private void AddDemoGameEventExample()
        {
            var category = (ResourceCategory) Random.Range(0, 9);
            var method = (SpendMethod)Random.Range(0, 8);
            var value = Random.Range(0, 101);
            Adapter.Record(new SpendEvent(category) { _method = method, _name = $"spend_{category} {method} {value}", _value = value });
        }
    }
}