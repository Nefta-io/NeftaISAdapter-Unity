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
        private const string DynamicAdUnitId = "g7xalw41x4i1bj5t";
        private const string DefaultAdUnitId = "q0z1act0tdckh4mg";
#else
        private const string DynamicAdUnitId = "0u6jgm23ggqso85n";
        private const string DefaultAdUnitId = "wrzl86if1sqfxquc";
#endif
        
        private LevelPlayInterstitialAd _dynamicInterstitial;
        private double _dynamicAdRevenue;
        private AdInsight _dynamicInsight;
        private LevelPlayInterstitialAd _defaultInterstitial;
        private double _defaultAdRevenue;
        
        [SerializeField] private Text _title;
        [SerializeField] private Toggle _load;
        [SerializeField] private Text _loadText;
        [SerializeField] private Button _show;
        [SerializeField] private Text _status;
        
        private void StartLoading()
        {
            if (_dynamicInterstitial == null)
            {
                GetInsightsAndLoad(null); 
            }
            if (_defaultInterstitial == null)
            {
                LoadDefault();   
            }
        }
        
        private void GetInsightsAndLoad(AdInsight previousInsight)
        {
            Adapter.GetInsights(Insights.Interstitial, previousInsight, LoadWithInsights, 5);
        }
        
        private void LoadWithInsights(Insights insights)
        {
            _dynamicInsight = insights._interstitial;
            if (_dynamicInsight != null)
            {
                SetStatus($"Loading Dynamic Interstitial with: {_dynamicInsight}");

                var config = new LevelPlayInterstitialAd.Config.Builder()
                    .SetBidFloor(_dynamicInsight._floorPrice)
                    .Build();
                _dynamicInterstitial = new LevelPlayInterstitialAd(DynamicAdUnitId, config);
                Load(_dynamicInterstitial);

                Adapter.OnExternalMediationRequest(_dynamicInterstitial, _dynamicInsight);
            }
        }
        
        private void LoadDefault()
        {
            SetStatus($"Loading Default Interstitial {DefaultAdUnitId}");
            
            _defaultInterstitial = new LevelPlayInterstitialAd(DefaultAdUnitId);
            Load(_defaultInterstitial);
            
            Adapter.OnExternalMediationRequest(_defaultInterstitial);
        }
        
        private void OnAdLoadFailed(LevelPlayAdError error)
        {
            Adapter.OnExternalMediationRequestFailed(error);

            if (_dynamicInterstitial != null && error.AdId == _dynamicInterstitial.GetAdId())
            {
                SetStatus($"OnAdLoadFailed Dynamic {error}");

                _dynamicInterstitial = null;
                if (_load.isOn)
                {
                    StartCoroutine(ReTryLoad(true));
                }
            }
            else
            {
                SetStatus($"OnAdLoadFailed Default {error}");

                _defaultInterstitial = null; 
                if (_load.isOn)
                {
                    StartCoroutine(ReTryLoad(false));
                }
            }
        }
        
        private void OnAdLoaded(LevelPlayAdInfo info)
        {
            Adapter.OnExternalMediationRequestLoaded(info);

            if (_dynamicInterstitial != null && info.AdId == _dynamicInterstitial.GetAdId())
            {
                SetStatus($"OnAdLoaded Dynamic {info}");

                _dynamicAdRevenue = info.Revenue ?? 0;
            }
            else
            {
                SetStatus($"OnAdLoaded Default {info}");

                _defaultAdRevenue = info.Revenue ?? 0;
            }
            
            UpdateShowButton();
        }
        
        private void OnAdClicked(LevelPlayAdInfo info)
        {
            Adapter.OnLevelPlayClick(info);
            
            SetStatus($"OnAdClicked {info}");
        }
        
        private IEnumerator ReTryLoad(bool isDynamic)
        {
            yield return new WaitForSeconds(5f);

            if (_load.isOn)
            {
                if (isDynamic)
                {
                    if (_dynamicInterstitial == null)
                    {
                        GetInsightsAndLoad(_dynamicInsight);     
                    }
                }
                else
                {
                    if (_defaultInterstitial == null)
                    {
                        LoadDefault();     
                    }
                }
            }
        }

        public void Init()
        {
            _load.onValueChanged.AddListener(OnLoadChanged);
            _show.onClick.AddListener(OnShowClick);
            
            _load.interactable = false;
            _show.interactable = false;
        }
        
        public void OnReady()
        {
            _load.interactable = true;
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
            bool isShown = false;
            if (_dynamicAdRevenue >= 0)
            {
                if (_defaultAdRevenue > _dynamicAdRevenue)
                {
                    isShown = TryShowDefault();
                }
                if (!isShown)
                {
                    isShown = TryShowDynamic();
                }
            }
            if (!isShown && _defaultAdRevenue >= 0)
            {
                TryShowDefault();
            }
            UpdateShowButton();
        }
        
        private bool TryShowDynamic()
        {
            var isShown = false;
            if (_dynamicInterstitial.IsAdReady())
            {
                SetStatus("Showing Dynamic Interstitial");
                _dynamicInterstitial.ShowAd();
                isShown = true;
            }
            _dynamicAdRevenue = -1;
            _dynamicInterstitial = null;
            return isShown;
        }

        private bool TryShowDefault()
        {
            var isShown = false;
            if (_defaultInterstitial.IsAdReady())
            {
                SetStatus("Showing Default Interstitial");
                _defaultInterstitial.ShowAd();
                isShown = true;
            }
            _defaultAdRevenue = -1;
            _defaultInterstitial = null;
            return isShown;
        }

        private void Load(LevelPlayInterstitialAd interstitial)
        {
            interstitial.OnAdLoaded += OnAdLoaded;
            interstitial.OnAdLoadFailed += OnAdLoadFailed;
            interstitial.OnAdDisplayed += OnAdDisplayed;
            interstitial.OnAdDisplayFailed += OnAdDisplayFailed;
            interstitial.OnAdClicked += OnAdClicked;
            interstitial.OnAdInfoChanged += OnAdInfoChanged;
            interstitial.OnAdClosed += OnAdClosed;
            interstitial.LoadAd();
        }
        
        private void OnAdDisplayFailed(LevelPlayAdDisplayInfoError error)
        {
            SetStatus($"OnAdDisplayFailed {error}");
        }

        private void OnAdDisplayed(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdDisplayed {info}");
        }

        private void OnAdInfoChanged(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdInfoChanged {info}");
        }
        
        private void OnAdClosed(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdClosed {info}");
            
            // start new cycle
            if (_load.isOn)
            {
                StartLoading();
            }
        }

        private void SetStatus(string status)
        {
            _status.text = status;
            Debug.Log($"NeftaPluginIS Interstitial {status}");
        }
        
        private void UpdateShowButton()
        {
            _show.interactable = _dynamicAdRevenue >= 0 || _defaultAdRevenue >= 0;
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