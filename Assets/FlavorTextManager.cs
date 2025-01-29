using Scripting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlavorTextManager : MonoBehaviour
{
    TextMeshProUGUI myTextField;
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
                Debug.Log($"{nameof(GameManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
        myTextField = this.GetComponent<TextMeshProUGUI>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void updateFlavorText(string newFlavorText)
    {
        if (myTextField != null)
        {
            myTextField.text = newFlavorText;
        }
        else
            Debug.Log("myTextField is null!!!!!!!!!!");
    }

    public void updateFlavorText(UpgradeButton relevantButton)
    {
        updateFlavorText(relevantButton.FetchFlavorText());
    }

    public void updateFlavorText(lightSwitch relevantLightSwitch)
    {
        updateFlavorText(relevantLightSwitch.FetchFlavorText());
    }

    public void clearFlavorText()
    {
        if (myTextField != null)
        {
            myTextField.text = "";
        }
    }
}
