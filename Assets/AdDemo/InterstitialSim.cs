using System;
using System.Collections;
using System.Reflection;
using Nefta;
using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class InterstitialSim : MonoBehaviour
    {
        private const string AdUnitA = "Track A";
        private const string AdUnitB = "Track B";
        
        private const int TimeoutInSeconds = 5;
        private readonly Color DefaultColor = new Color(0.6509804f, 0.1490196f, 0.7490196f, 1f);
        private readonly Color FillColor = Color.green;
        private readonly Color NoFillColor = Color.red;

        private enum State
        {
            Idle,
            LoadingWithInsights,
            Loading,
            Ready
        }
        
        private class AdRequest
        {
            public readonly string AdUnitId;
            public SLevelPlayInterstitialAd Interstitial;
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
        
        [Header("Controls")]
        [SerializeField] private RectTransform _rootRect;
        [SerializeField] private Toggle _load;
        [SerializeField] private Button _show;
        [SerializeField] private Text _status;
        
        [Header("Track A")]
        [SerializeField] private Image _aFill2Renderer;
        [SerializeField] private Button _aFill2;
        [SerializeField] private Image _aFill1Renderer;
        [SerializeField] private Button _aFill1;
        [SerializeField] private Image _aNoFillRenderer;
        [SerializeField] private Button _aNoFill;
        [SerializeField] private Image _aOtherRenderer;
        [SerializeField] private Button _aOther;
        [SerializeField] private Text _aStatus;
        
        [Header("Track B")]
        [SerializeField] private Image _bFill2Renderer;
        [SerializeField] private Button _bFill2;
        [SerializeField] private Image _bFill1Renderer;
        [SerializeField] private Button _bFill1;
        [SerializeField] private Image _bNoFillRenderer;
        [SerializeField] private Button _bNoFill;
        [SerializeField] private Image _bOtherRenderer;
        [SerializeField] private Button _bOther;
        [SerializeField] private Text _bStatus;
        
        public static InterstitialSim Instance;
        
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
            
            Adapter.GetInsights(Insights.Interstitial, adRequest.Insight, (Insights insights) => {
                var insight = insights._interstitial;
                SetStatus($"Load with Insights: {insight}");
                if (insight != null)
                {
                    adRequest.Insight = insights._interstitial;
                    var config = new LevelPlayInterstitialAd.Config.Builder()
                        .SetBidFloor(adRequest.Insight._floorPrice)
                        .Build();
                    adRequest.Interstitial = new SLevelPlayInterstitialAd(adRequest.AdUnitId, config);

                    Adapter.OnExternalMediationRequest(adRequest.Interstitial, adRequest.Insight);

                    adRequest.Load();
                }
                else
                {
                    adRequest.AfterLoadFail();
                }
            }, TimeoutInSeconds);
        }
        
        private void LoadDefault(AdRequest adRequest)
        {
            adRequest.State = State.Loading;
            
            SetStatus($"Loading {adRequest.AdUnitId} as Default");

            adRequest.Interstitial = new SLevelPlayInterstitialAd(adRequest.AdUnitId);
            
            Adapter.OnExternalMediationRequest(adRequest.Interstitial);
            
            adRequest.Load();
        }
        
        private void Awake()
        {
            Instance = this;
            
            _adRequestA = new AdRequest(AdUnitA);
            _adRequestB = new AdRequest(AdUnitB);
            
            ToggleTrackA(false);
            _aFill2.onClick.AddListener(() => { SimOnAdLoadedEvent(_adRequestA, true); });
            _aFill1.onClick.AddListener(() => { SimOnAdLoadedEvent(_adRequestA, false); });
            _aNoFill.onClick.AddListener(() => { SimOnAdFailedEvent(_adRequestA, 2); });
            _aOther.onClick.AddListener(() => { SimOnAdFailedEvent(_adRequestA, 0); });
            
            ToggleTrackB(false);
            _bFill2.onClick.AddListener(() => { SimOnAdLoadedEvent(_adRequestB, true); });
            _bFill1.onClick.AddListener(() => { SimOnAdLoadedEvent(_adRequestB, false); });
            _bNoFill.onClick.AddListener(() => { SimOnAdFailedEvent(_adRequestB, 2); });
            _bOther.onClick.AddListener(() => { SimOnAdFailedEvent(_adRequestB, 0); });
            
            
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
            else
            {
                if (_adRequestA.State != State.Ready)
                {
                    _adRequestA.State = State.Idle;
                }

                if (_adRequestB.State != State.Ready)
                {
                    _adRequestB.State = State.Idle;
                }
            }
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
            request.Revenue = 0;
            
            if (request.Interstitial.IsAdReady())
            {
                SetStatus($"Showing {request.AdUnitId}");
                request.Interstitial.ShowAd(request.AdUnitId);
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
            Debug.Log($"NeftaPluginIS Simulator: {status}");
        }
        
        private void UpdateShowButton()
        {
            _show.interactable = _adRequestA.State == State.Ready || _adRequestB.State == State.Ready;
        }

        public class SLevelPlayInterstitialAd : LevelPlayInterstitialAd
        {
            public class Internal : IPlatformInterstitialAd
            {
                private string _adUnitId;
                private string _adId;
                private string _auctionId;
                private string _json;
                
                public  com.unity3d.mediation.LevelPlayAdInfo AdInfo;
                
                public Internal(string adUnitId)
                {
                    _adUnitId = adUnitId;
                    _adId = Guid.NewGuid().ToString();
                    _auctionId = "a_" + _adId;
                }
                
                public void Dispose()
                {
                }

                public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;
                public event Action<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;
                public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;
                public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClosed;
                public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;
                public event Action<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;
                public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdInfoChanged;
                public string AdId => _adId;
                public string AdUnitId => _adUnitId;
                public void LoadAd()
                {
                    if (AdUnitId == AdUnitA)
                    {
                        var floor = Instance._adRequestA.Insight?._floorPrice ?? -1;
                    
                        Instance.ToggleTrackA(true);
                        Instance._aStatus.text = $"{AdUnitId} loading " + (floor >= 0 ? "as Optimized": "as Default");
                    }
                    else
                    {
                        var floor = Instance._adRequestB.Insight?._floorPrice ?? -1;
                    
                        Instance.ToggleTrackB(true);
                        Instance._bStatus.text = $"{AdUnitId} loading " + (floor >= 0 ? "as Optimized": "as Default");
                    }
                }

                public void ShowAd(string placementName)
                {
                    Type type = typeof(LevelPlayImpressionData);
                    ConstructorInfo ctor = type.GetConstructor(
                        BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        new[] { typeof(string) },
                        null);
                    var impression = (LevelPlayImpressionData)ctor.Invoke(new object[] { _json });
                    Adapter.OnLevelPlayImpression(impression);

                    var simulatorAdPrefab = Resources.Load<SimulatorAd>("SimulatorAd");
                    var simAd = Instantiate(simulatorAdPrefab, Instance._rootRect);
                    simAd.Init("Interstitial",
                        () => { OnAdDisplayed?.Invoke(AdInfo); },
                        () => { OnAdClicked?.Invoke(AdInfo); },
                        null,
                        () => {
                            OnAdClosed?.Invoke(AdInfo);
                            AdInfo = null;
                        });
                
                    if (AdUnitId == AdUnitA)
                    {
                        Instance._aStatus.text = "Showing A";
                    }
                    else
                    {
                        Instance._bStatus.text = "Showing B";
                    }
                }

                public bool IsAdReady()
                {
                    return AdInfo != null;
                }

                public void TriggerAdLoad(double revenue)
                {
                    _json = $"{{\"adId\":\"{AdId}\",\"adUnitId\":\"{AdUnitId}\",\"auctionId\":\"{_auctionId}\",\"revenue\":{revenue},\"precision\":\"BID\",\"adNetwork\":\"simulator network\",\"adFormat\":\"interstitial\",\"creativeId\":\"simulator creative\"}}";
                    Type type = typeof(com.unity3d.mediation.LevelPlayAdInfo);
                    ConstructorInfo ctor = type.GetConstructor(
                        BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        new[] { typeof(string) },
                        null);
                    AdInfo = (com.unity3d.mediation.LevelPlayAdInfo)ctor.Invoke(new object[] { _json });
                    
                    OnAdLoaded?.Invoke(AdInfo);
                }
                
                public void TriggerAdLoadFailed(int status)
                {
                    Type type = typeof(com.unity3d.mediation.LevelPlayAdError);
                    ConstructorInfo ctor = type.GetConstructor(
                        BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        new[] { typeof(string) },
                        null);
                    var json = $"{{\"errorCode\":{(status == 2 ? 509 : 100)},\"errorMessage\":\"simulator message\",\"adUnitId\":\"{AdUnitId}\",\"adId\":\"{AdId}\"}}";
                    var error = (com.unity3d.mediation.LevelPlayAdError)ctor.Invoke(new object[] { json });
                    
                    OnAdLoadFailed?.Invoke(error);
                }
            }
            
            public Internal _internal;
            
            public SLevelPlayInterstitialAd(string adUnitId) : base(adUnitId)
            {
                Set(adUnitId);
            }

            public SLevelPlayInterstitialAd(string adUnitId, Config config) : base(adUnitId, config)
            {
                Set(adUnitId);
            }

            private void Set(string adUnitId)
            {
                _internal = new Internal(adUnitId);
                
                var internalField = typeof(LevelPlayInterstitialAd).GetField("m_InterstitialAd", BindingFlags.Instance | BindingFlags.NonPublic);
                internalField.SetValue(this, _internal);
                
                var setupMethod = typeof(LevelPlayInterstitialAd).GetMethod("SetupEvents", BindingFlags.Instance | BindingFlags.NonPublic);
                setupMethod.Invoke(this, null);
            }
        }
        
        private void SimOnAdLoadedEvent(AdRequest request, bool high)
        {
            var revenue = high ? 0.002 : 0.001;
            if (request.Interstitial._internal.AdInfo != null)
            {
                request.Interstitial._internal.AdInfo = null;
                if (request == _adRequestA)
                {
                    if (high)
                    {
                        _aFill2Renderer.color = DefaultColor;
                        _aFill2.interactable = false;
                    }
                    else
                    {
                        _aFill1Renderer.color = DefaultColor;
                        _aFill1.interactable = false;
                    }
                }
                if (request == _adRequestB)
                {
                    if (high)
                    {
                        _bFill2Renderer.color = DefaultColor;
                        _bFill2.interactable = false;
                    }
                    else
                    {
                        _bFill1Renderer.color = DefaultColor;
                        _bFill1.interactable = false;
                    }
                }
                return;
            }
            
            if (request == _adRequestA)
            {
                ToggleTrackA(false);
                if (high)
                {
                    _aFill2Renderer.color = FillColor;
                    _aFill2.interactable = true;
                }
                else
                {
                    _aFill1Renderer.color = FillColor;
                    _aFill1.interactable = true;
                }
                _aStatus.text = $"{request.AdUnitId} loaded {revenue}";
            }
            else
            {
                ToggleTrackB(false);
                if (high)
                {
                    _bFill2Renderer.color = FillColor;
                    _bFill2.interactable = true;
                }
                else
                {
                    _bFill1Renderer.color = FillColor;
                    _bFill1.interactable = true;
                }
                _bStatus.text = $"{request.AdUnitId} loaded {revenue}";
            }
            
            request.Interstitial._internal.TriggerAdLoad(revenue);
        }

        private void ToggleTrackA(bool isOn)
        {
            _aFill2.interactable = isOn;
            _aFill1.interactable = isOn;
            _aNoFill.interactable = isOn;
            _aOther.interactable = isOn;
            if (isOn)
            {
                _aFill2Renderer.color = DefaultColor;
                _aFill1Renderer.color = DefaultColor;
                _aNoFillRenderer.color = DefaultColor;
                _aOtherRenderer.color = DefaultColor;
            }
        }

        private void ToggleTrackB(bool isOn)
        {
            _bFill2.interactable = isOn;
            _bFill1.interactable = isOn;
            _bNoFill.interactable = isOn;
            _bOther.interactable = isOn;
            if (isOn)
            {
                _bFill2Renderer.color = DefaultColor;
                _bFill1Renderer.color = DefaultColor;
                _bNoFillRenderer.color = DefaultColor;
                _bOtherRenderer.color = DefaultColor;
            }
        }
        
        private void SimOnAdFailedEvent(AdRequest adRequest, int status)
        {
            if (adRequest == _adRequestA)
            {
                if (status == 2)
                {
                    _aNoFillRenderer.color = NoFillColor;
                }
                else
                {
                    _aOtherRenderer.color = NoFillColor;
                }
                _aStatus.text = $"{adRequest.AdUnitId} failed";
                ToggleTrackA(false);
            }
            else
            {
                if (status == 2)
                {
                    _bNoFillRenderer.color = NoFillColor;
                }
                else
                {
                    _bOtherRenderer.color = NoFillColor;
                }
                _bStatus.text = $"{adRequest.AdUnitId} failed";
                ToggleTrackB(false);
            }
            
            adRequest.Interstitial._internal.TriggerAdLoadFailed(status); 
        }
    }
}