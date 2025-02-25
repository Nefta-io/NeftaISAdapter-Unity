using com.unity3d.mediation;
using Nefta;
using Nefta.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class RewardedController : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Button _load;
        [SerializeField] private Button _show;
        [SerializeField] private Text _status;
        
        private LevelPlayRewardedAd _rewarded;
        
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
            var method = (SpendMethod)Random.Range(0, 8);
            var value = Random.Range(0, 101);
            Adapter.Record(new SpendEvent(category) { _method = method, _name = $"spend_{category} {method} {value}", _value = value });
            
            string adUnitId = "kftiv52431x91zuk";
#if UNITY_IOS
            adUnitId = "doucurq8qtlnuz7p";
#endif

            _rewarded = new LevelPlayRewardedAd(adUnitId);
            _rewarded.OnAdLoaded += OnAdLoaded;
            _rewarded.OnAdLoadFailed += OnAdLoadFailed;
            _rewarded.OnAdDisplayed += OnAdDisplayed;
            _rewarded.OnAdDisplayFailed += OnAdDisplayFailed;
            _rewarded.OnAdRewarded += OnAdRewarded;
            _rewarded.OnAdClicked += OnAdClicked;
            _rewarded.OnAdInfoChanged += OnAdInfoChanged;
            _rewarded.OnAdClosed += OnAdClosed;
            _rewarded.LoadAd();
            
            SetStatus("Loading rewarded...");
        }
        
        private void OnShowClick()
        {
            _show.interactable = false;
            if (_rewarded.IsAdReady())
            {
                SetStatus("Showing rewarded");
                _rewarded.ShowAd();
            }
            else
            {
                SetStatus("Rewarded not ready");
            }
        }
        
        private void OnAdLoaded(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdLoaded {info}");
            _show.interactable = true;
            
            Adapter.OnExternalAdLoad(Adapter.AdType.Rewarded, 0.4f);
        }
        
        private void OnAdLoadFailed(LevelPlayAdError error)
        {
            SetStatus($"OnAdLoadFailed {error}");
            _show.interactable = true;
            
            Adapter.OnExternalAdFail(Adapter.AdType.Rewarded, 0.4f, error.ErrorCode);
        }
        
        private void OnAdDisplayFailed(LevelPlayAdDisplayInfoError error)
        {
            SetStatus($"OnAdDisplayFailed {error}");
        }
        
        private void OnAdDisplayed(LevelPlayAdInfo info)
        {
            SetStatus($"OnAdDisplayed {info}");
        }
        
        private void OnAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
        {
            SetStatus($"OnAdRewarded {info}: {reward}");
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