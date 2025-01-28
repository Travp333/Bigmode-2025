using AI;
using Scripting;
using Scripting.ScriptableObjects;
using System.Linq;
using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
    public Upgrades.UpgradeTypes myUpgradeType;
    public float moneyAmount = 0;
    private bool _bShowFlavorText = false;
    private int _flavorTextCountdown = 0;
    private Rect _flavorRect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        var temp = GameManager.Singleton.mainCanvas.GetComponent<RectTransform>();
        var tempTemp = new Rect(temp.rect.width / 2, temp.rect.height - 20.0f, 100.0f, 20.0f);
        //Debug.Log("Flavor text location: " + tempTemp.ToString());
        _flavorRect = tempTemp;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_bShowFlavorText)
        {
            if (_flavorTextCountdown > 0)
            {
                _flavorTextCountdown--;
            }
            else
            {
                _bShowFlavorText = false;
                _flavorTextCountdown = 0;
            }
        }
    }

    public void Pressed()
    {
        switch (myUpgradeType)
        {
            case Upgrades.UpgradeTypes.chairs:
                GameManager.Singleton.upgrades.chairs = true;
                GameManager.Singleton.distractionChairs.SetActive(true);
                GameManager.Singleton._aiSpots = GameManager.Singleton._aiSpots.Concat(GameManager.Singleton.distractionChairs.GetComponentsInChildren<AiSpot>()).ToList();
                break;
            case Upgrades.UpgradeTypes.paintings:
                GameManager.Singleton.upgrades.paintings = true;
                GameManager.Singleton.distractionPaintings.SetActive(true);
                GameManager.Singleton._aiSpots = GameManager.Singleton._aiSpots.Concat(GameManager.Singleton.distractionPaintings.GetComponentsInChildren<AiSpot>()).ToList();
                break;
            case Upgrades.UpgradeTypes.baseballBat:
                GameManager.Singleton.upgrades.baseballBat = true;
                GameManager.Singleton.baseballBat.SetActive(true);
                break;
            case Upgrades.UpgradeTypes.cigar:
                GameManager.Singleton.upgrades.cigar = true;
                GameManager.Singleton.cigar.SetActive(true);
                break;
            case Upgrades.UpgradeTypes.phone:
                GameManager.Singleton.upgrades.phone = true;
                GameManager.Singleton.phone.SetActive(true);
                break;
            case Upgrades.UpgradeTypes.bodyguard:
                GameManager.Singleton.upgrades.bodyguard = true;
                GameManager.Singleton.bodyguard.SetActive(true);
                break;
            case Upgrades.UpgradeTypes.assistant:
                GameManager.Singleton.upgrades.assistant = true;
                GameManager.Singleton.assistant.SetActive(true);
                break;
            case Upgrades.UpgradeTypes.money:
                GameManager.Singleton.upgrades.money += moneyAmount;
                break;
            default:
                break;
        }

        Destroy(gameObject);
    }

    public void ShowFlavorText()
    {
        _bShowFlavorText = true;
        _flavorTextCountdown = 10;
    }

    private void OnGUI()
    {
        if (_bShowFlavorText)
        {
            var relevantText = "";
            switch (myUpgradeType)
            {
                case Upgrades.UpgradeTypes.chairs:
                    relevantText = GameManager.Singleton.upgrades.chairsFlavorText;
                    break;
                case Upgrades.UpgradeTypes.paintings:
                    relevantText = GameManager.Singleton.upgrades.paintingsFlavorText;
                    break;
                case Upgrades.UpgradeTypes.baseballBat:
                    relevantText = GameManager.Singleton.upgrades.baseballBatFlavorText;
                    break;
                case Upgrades.UpgradeTypes.cigar:
                    relevantText = GameManager.Singleton.upgrades.cigarFlavorText;
                    break;
                case Upgrades.UpgradeTypes.phone:
                    relevantText = GameManager.Singleton.upgrades.phoneFlavorText;
                    break;
                case Upgrades.UpgradeTypes.bodyguard:
                    relevantText = GameManager.Singleton.upgrades.bodyguardFlavorText;
                    break;
                case Upgrades.UpgradeTypes.assistant:
                    relevantText = GameManager.Singleton.upgrades.assistantFlavorText;
                    break;
                default:
                    break;
            }

            GUI.Label(_flavorRect, relevantText);
        }
    }
}
