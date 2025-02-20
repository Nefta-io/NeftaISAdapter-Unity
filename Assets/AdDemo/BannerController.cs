using com.unity3d.mediation;
using Nefta;
using Nefta.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class BannerController : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Button _show;
        [SerializeField] private Button _hide;
        [SerializeField] private Text _status;

        public void Init()
        {
            IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
            IronSourceBannerEvents.onAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
            IronSourceBannerEvents.onAdClickedEvent += BannerOnAdClickedEvent;
            IronSourceBannerEvents.onAdScreenPresentedEvent += BannerOnAdScreenPresentedEvent;
            IronSourceBannerEvents.onAdScreenDismissedEvent += BannerOnAdScreenDismissedEvent;
            IronSourceBannerEvents.onAdLeftApplicationEvent += BannerOnAdLeftApplicationEvent;

            _title.text = "Banner info";
            _show.onClick.AddListener(OnShowClick);
            _hide.onClick.AddListener(OnHideClick);
            _hide.interactable = false;
        }
        
        private void OnShowClick()
        {
            var type = (Type) Random.Range(0, 7);
            var status = (Status)Random.Range(0, 3);
            var source = (Source)Random.Range(0, 7);
            var value = Random.Range(0, 101);
            Adapter.Record(new ProgressionEvent(type, status)
                { _source = source, _name = $"progression_{type}_{status} {source} {value}", _value = value });
            
            IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.TOP);
            _hide.interactable = true;
        }
        
        private void OnHideClick()
        {
            _hide.interactable = false;
            IronSource.Agent.destroyBanner();
        }
        
        private void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo)
        {
            SetStatus($"Loaded {adInfo.adNetwork} {adInfo.adUnit}");
        }
        
        private void BannerOnAdLoadFailedEvent(IronSourceError ironSourceError)
        {
            SetStatus($"Load failed {ironSourceError}");
        }

        private void BannerOnAdClickedEvent(IronSourceAdInfo adInfo)
        {
            SetStatus($"Clicked {adInfo.adNetwork} {adInfo.adUnit}");
        }

        void BannerOnAdScreenPresentedEvent(IronSourceAdInfo adInfo)
        {
            SetStatus($"Presented {adInfo.adNetwork} {adInfo.adUnit}");
        }

        void BannerOnAdScreenDismissedEvent(IronSourceAdInfo adInfo)
        {
            SetStatus($"Dismissed {adInfo.adNetwork} {adInfo.adUnit}");
        }

        void BannerOnAdLeftApplicationEvent(IronSourceAdInfo adInfo)
        {
            SetStatus($"LeftApplication {adInfo.adNetwork} {adInfo.adUnit}");
        }
        
        private void SetStatus(string status)
        {
            _status.text = status;
            Debug.Log(status);
        }
    }
}