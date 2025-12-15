using System.Collections;
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
            Ready
        }
        
        private class AdRequest
        {
            public string AdUnitId;
            public LevelPlayInterstitialAd Interstitial;
            public State State;
            public AdInsight Insight;
            public double Revenue;

            public AdRequest(string adUnitId)
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

        public static InterstitialController Instance;
        
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
            
            Adapter.GetInsights(Insights.Interstitial, adRequest.Insight, (Insights insights) =>
            {
                SetStatus($"LoadWithInsights: {insights}");
                if (insights._interstitial != null)
                {
                    adRequest.Insight = insights._interstitial;
                    var config = new LevelPlayInterstitialAd.Config.Builder()
                        .SetBidFloor(adRequest.Insight._floorPrice)
                        .Build();
                    adRequest.Interstitial = new LevelPlayInterstitialAd(adRequest.AdUnitId, config);
                    
                    Adapter.OnExternalMediationRequest(adRequest.Interstitial, adRequest.Insight);
                    
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

            adRequest.Interstitial = new LevelPlayInterstitialAd(adRequest.AdUnitId);
            
            Adapter.OnExternalMediationRequest(adRequest.Interstitial);
            
            adRequest.Load();
        }

        private void Awake()
        {
            Instance = this;

            _adRequestA = new AdRequest(AdUnitA);
            _adRequestB = new AdRequest(AdUnitB);
            
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

            if (request.Interstitial.IsAdReady())
            {
                request.Interstitial.ShowAd();
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
            Debug.Log($"NeftaPluginIS Interstitial {status}");
        }
        
        private void UpdateShowButton()
        {
            _show.interactable = _adRequestA.State == State.Ready || _adRequestB.State == State.Ready;
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