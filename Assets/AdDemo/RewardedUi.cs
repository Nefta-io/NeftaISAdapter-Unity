using UnityEngine;
using UnityEngine.UI;

namespace AdDemo
{
    public class RewardedUi : MonoBehaviour
    {
#if UNITY_IOS
        public const string AdUnitA = "p3dh8r1mm3ua8fvv";
        public const string AdUnitB = "doucurq8qtlnuz7p";
#else
        public const string AdUnitA = "x3helvrx8elhig4z";
        public const string AdUnitB = "kftiv52431x91zuk";
#endif
        private IRewarded _logic;
      
        [SerializeField] private Toggle _load;
        [SerializeField] private Button _show;
        [SerializeField] private Text _status;
        
        public bool IsAutoLoad { get; private set; }
        
        public void Init(IRewarded logic)
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
            Debug.Log($"Rewarded: {status}");
        }
    }
}