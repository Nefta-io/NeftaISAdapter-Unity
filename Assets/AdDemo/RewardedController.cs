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
        
        public void Init()
        {
            IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
            IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
            
            _load.onClick.AddListener(OnLoadClick);
            _show.onClick.AddListener(OnShowClick);

            _show.interactable = false;
        }
        
        private void OnLoadClick()
        {
            var category = (ResourceCategory) Random.Range(0, 9);
            var method = (SpendMethod)Random.Range(0, 8);
            var value = Random.Range(0, 101);
            Adapter.Record(new SpendEvent(category) { _method = method, _name = $"spend_{category} {method} {value}", _value = value });
            
            SetStatus("Loading rewarded...");
            IronSource.Agent.loadRewardedVideo();
        }
        
        private void OnShowClick()
        {
            _show.interactable = false;
            if (IronSource.Agent.isRewardedVideoAvailable())
            {
                SetStatus("Showing rewarded");
                IronSource.Agent.showRewardedVideo();
            }
            else
            {
                SetStatus("Rewarded not ready");
            }
        }
        
        private void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
        {
            SetStatus($"Loaded {adInfo.adNetwork} {adInfo.adUnit}");
            _show.interactable = true;
        }
        
        private void RewardedVideoOnAdUnavailable()
        {
            SetStatus("Rewarded unavailable");
            _show.interactable = true;
        }
        
        private void RewardedVideoOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
        {
            SetStatus($"Display failed {adInfo.adNetwork} {adInfo.adUnit}");
        }
        
        private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            SetStatus($"Displayed {adInfo.adNetwork} {adInfo.adUnit}");
        }
        
        private void RewardedVideoOnAdClickedEvent(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo adInfo)
        {
            SetStatus($"Rewarded ad clicked {adInfo.adNetwork} {adInfo.adUnit}");
        }
        
        private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
            SetStatus($"Hidden {adInfo.adNetwork} {adInfo.adUnit}");
        }
        
        private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo adInfo)
        {
            SetStatus($"Rewarded ad received reward {adInfo.adNetwork} {adInfo.adUnit}");
        }
        
        private void SetStatus(string status)
        {
            _status.text = status;
            Debug.Log(status);
        }
        
    }
}