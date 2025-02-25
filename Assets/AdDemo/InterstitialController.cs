using com.unity3d.mediation;
using Nefta;
using Nefta.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class InterstitialController : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Button _load;
        [SerializeField] private Button _show;
        [SerializeField] private Text _status;

        private LevelPlayInterstitialAd _interstitial;

        public void Init()
        {
            _load.onClick.AddListener(OnLoadClick);
            _show.onClick.AddListener(OnShowClick);
            
            _load.interactable = false;
            _show.interactable = false;
        }
        
        public void OnReady()
        {
            _load.interactable = true;
        }

        private void OnLoadClick()
        {
            var category = (ResourceCategory) Random.Range(0, 9);
            var method = (ReceiveMethod)Random.Range(0, 8);
            var value = Random.Range(0, 101);
            Adapter.Record(new ReceiveEvent(category) { _method = method, _name = $"receive_{category} {method} {value}", _value = value });

            string adUnitId = "wrzl86if1sqfxquc";
#if UNITY_IOS
            adUnitId = "q0z1act0tdckh4mg";
#endif
            
            _interstitial = new LevelPlayInterstitialAd(adUnitId);
            _interstitial.OnAdLoaded += OnAdLoaded;
            _interstitial.OnAdLoadFailed += OnAdLoadFailed;
            _interstitial.OnAdDisplayed += OnAdDisplayed;
            _interstitial.OnAdDisplayFailed += OnAdDisplayFailed;
            _interstitial.OnAdClicked += OnAdClicked;
            _interstitial.OnAdInfoChanged += OnAdInfoChanged;
            _interstitial.OnAdClosed += OnAdClosed;
            _interstitial.LoadAd();
            
            SetStatus("Loading interstitial...");
        }
        
        private void OnShowClick()
        {
            _show.interactable = false;
            if (_interstitial.IsAdReady())
            {
                SetStatus("Showing interstitial");
                _interstitial.ShowAd();
            }
            else
            {
                SetStatus("Interstitial not ready");
            }
        }

        private void OnAdLoaded(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdLoaded {info}");
            _show.interactable = true;
            
            Adapter.OnExternalAdLoad(Adapter.AdType.Interstitial, 0.4f);
        }

        private void OnAdLoadFailed(LevelPlayAdError error)
        {
            SetStatus($"OnAdLoadFailed {error}");
            
            Adapter.OnExternalAdFail(Adapter.AdType.Interstitial, 0.4f, error.ErrorCode);
        }
        
        private void OnAdDisplayFailed(LevelPlayAdDisplayInfoError error)
        {
            SetStatus($"OnAdDisplayFailed {error}");
        }

        private void OnAdDisplayed(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdDisplayed {info}");
        }

        private void OnAdClicked(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdClicked {info}");
        }

        private void OnAdInfoChanged(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdInfoChanged {info}");
        }
        
        private void OnAdClosed(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdClosed {info}");
        }

        private void SetStatus(string status)
        {
            _status.text = status;
            Debug.Log(status);
        }
    }
}