using TMPro;
using UnityEngine;

namespace Scripting
{
    public class FlavorTextManager : MonoBehaviour
    {
        [SerializeField]
        GameObject uibg;
        Animator uibganim;
        private TextMeshProUGUI _myTextField;
        private static FlavorTextManager _singleton;

        public static FlavorTextManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    Debug.Log($"{nameof(FlavorTextManager)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }

        private void Awake()
        {
            uibganim = uibg.GetComponent<Animator>();
            Singleton = this;
            _myTextField = GetComponent<TextMeshProUGUI>();
        }


        public void UpdateFlavorText(string newFlavorText)
        {
            if (_myTextField)
            {
                uibganim.SetBool("DOWN", false);
                uibganim.SetBool("UP", true);
                _myTextField.text = newFlavorText;
            }
            else
                Debug.Log("myTextField is null!!!!!!!!!!");
        }
   

        public void UpdateFlavorText(UpgradeButton relevantButton)
        {
            UpdateFlavorText(relevantButton.FetchFlavorText());
        }

        public void UpdateFlavorText(LightSwitch relevantLightSwitch)
        {
            UpdateFlavorText(relevantLightSwitch.FetchFlavorText());
        }

        public void ClearFlavorText()
        {
            if (_myTextField != null)
            {
                uibganim.SetBool("DOWN", true);
                uibganim.SetBool("UP", false);
                _myTextField.text = "";
            }
        }
    }
}
