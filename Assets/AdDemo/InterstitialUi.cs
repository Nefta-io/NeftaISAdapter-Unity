using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class InterstitialUi : MonoBehaviour
    {
#if UNITY_IOS
        public const string AdUnitA = "g7xalw41x4i1bj5t";
        public const string AdUnitB = "q0z1act0tdckh4mg";
#else
        public const string AdUnitA = "0u6jgm23ggqso85n";
        public const string AdUnitB = "wrzl86if1sqfxquc";
#endif
        private IInterstitial _logic;
        
        [SerializeField] private Toggle _load;
        [SerializeField] private Text _status;
        [SerializeField] private Button _show;
        
        public bool IsAutoLoad { get; private set; }

        public void Init(IInterstitial logic)
        {
            _load.onValueChanged.AddListener(OnLoadChanged);
            _show.interactable = false;
            _show.onClick.AddListener(OnShowClick);
            gameObject.SetActive(true);

            _logic = logic;
            _logic.Init(this);
        }
        
        private void OnLoadChanged(bool isOn)
        {
            IsAutoLoad = isOn;
            if (IsAutoLoad)
            {
                _logic.Load();
            }
        }
        
        public void SetAvailability(bool isAvailable)
        {
            _show.interactable = isAvailable;
        }
        
        private void OnShowClick()
        {
            _logic.Show();
        }
        
        public void SetStatus(string status)
        {
            _status.text = status;
            Debug.Log($"Interstitial: {status}");
        }
    }
}