using TMPro;
using UnityEngine;

public class FlavorTextManager : MonoBehaviour
{
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
        Singleton = this;
        _myTextField = GetComponent<TextMeshProUGUI>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void UpdateFlavorText(string newFlavorText)
    {
        if (_myTextField)
        {
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
            _myTextField.text = "";
        }
    }
}
