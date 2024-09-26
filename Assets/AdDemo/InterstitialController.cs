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

        public void Init()
        {
            IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
            IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
            IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
            IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
            IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent;
            IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
            IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;
            
            _load.onClick.AddListener(OnLoadClick);
            _show.onClick.AddListener(OnShowClick);
            
            _show.interactable = false;
        }

        private void OnLoadClick()
        {
            var category = (ResourceCategory) Random.Range(0, 9);
            var method = (ReceiveMethod)Random.Range(0, 8);
            var value = Random.Range(0, 101);
            Adapter.Record(new ReceiveEvent(category) { _method = method, _name = $"receive_{category} {method} {value}", _value = value });
            
            SetStatus("Loading interstitial...");
            IronSource.Agent.loadInterstitial();
        }
        
        private void OnShowClick()
        {
            _show.interactable = false;
            if (IronSource.Agent.isInterstitialReady())
            {
                SetStatus("Showing interstitial");
                IronSource.Agent.showInterstitial();
            }
            else
            {
                SetStatus("Interstitial not ready");
            }
        }

        private void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo)
        {
            SetStatus($"Loaded {adInfo.adNetwork} {adInfo.adUnit}");
            _show.interactable = true;
        }

        private void InterstitialOnAdLoadFailed(IronSourceError ironSourceError)
        {
            SetStatus($"Load failed {ironSourceError}");
        }
        
        private void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
        {
            SetStatus($"Display failed {adInfo.adNetwork} {adInfo.adUnit}");
        }

        private void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            SetStatus($"Opened {adInfo.adNetwork} {adInfo.adUnit}");
        }

        private void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo)
        {
            SetStatus($"Clicked {adInfo.adNetwork} {adInfo.adUnit}");
        }

        private void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo adInfo)
        {
            SetStatus($"Displayed {adInfo.adNetwork} {adInfo.adUnit}");
        }
        
        private void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
            SetStatus($"Hidden {adInfo.adNetwork} {adInfo.adUnit}");
        }

        private void SetStatus(string status)
        {
            _status.text = status;
            Debug.Log(status);
        }
    }
}