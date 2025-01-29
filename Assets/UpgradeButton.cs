using AI;
using Scripting;
using Scripting.ScriptableObjects;
using System.Linq;
using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
    public Upgrades.UpgradeTypes myUpgradeType;
    public float moneyAmount = 0;
    public bool beenPressed = false;
    private bool _bShowFlavorText = false;
    private int _flavorTextCountdown = 0;
    private Rect _flavorRect;
    [SerializeField]
    UpgradeButton twin;
    [SerializeField] Texture testTexture;

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
        if (beenPressed)
        {
            return;
        }

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
            case Upgrades.UpgradeTypes.dismissal:
                GameManager.Singleton.upgrades.dismissal = true;
                break;
            case Upgrades.UpgradeTypes.hellishContract:
                GameManager.Singleton.upgrades.hellishContract = true;
                break;
            case Upgrades.UpgradeTypes.powerFistRequisition:
                GameManager.Singleton.upgrades.powerFistRequisition = true;
                break;
            case Upgrades.UpgradeTypes.loanAgreement:
                GameManager.Singleton.upgrades.loanAgreement = true;
                break;
            case Upgrades.UpgradeTypes.temporaryEmploymentContract:
                GameManager.Singleton.upgrades.temporaryEmploymentContract = true;
                break;
            case Upgrades.UpgradeTypes.endOfLifePlan:
                GameManager.Singleton.upgrades.endOfLifePlan = true;
                break;
            default:
                break;
        }

        beenPressed = true;
        if (testTexture != null)
        {
            var tempRenderer = this.GetComponent<MeshRenderer>();
            tempRenderer.material.mainTexture = testTexture;
            tempRenderer.material.DisableKeyword("_EMISSION");
        }
        if (twin != null)
        {
            twin.beenPressed = beenPressed;
            if (twin.testTexture != null)
            {
                var tempRenderer = twin.GetComponent<MeshRenderer>();
                tempRenderer.material.mainTexture = testTexture;
                tempRenderer.material.DisableKeyword("_EMISSION");
            }
        }
        //Destroy(gameObject);
    }

    public void ShowFlavorText()
    {
        _bShowFlavorText = true;
        _flavorTextCountdown = 10;
    }

    public string FetchFlavorText()
    {
        switch (myUpgradeType)
        {
            case Upgrades.UpgradeTypes.chairs:
                return GameManager.Singleton.upgrades.chairsFlavorText;
            case Upgrades.UpgradeTypes.paintings:
                return GameManager.Singleton.upgrades.paintingsFlavorText;
            case Upgrades.UpgradeTypes.baseballBat:
                return GameManager.Singleton.upgrades.baseballBatFlavorText;
            case Upgrades.UpgradeTypes.cigar:
                return GameManager.Singleton.upgrades.cigarFlavorText;
            case Upgrades.UpgradeTypes.phone:
                return GameManager.Singleton.upgrades.phoneFlavorText;
            case Upgrades.UpgradeTypes.bodyguard:
                return GameManager.Singleton.upgrades.bodyguardFlavorText;
            case Upgrades.UpgradeTypes.assistant:
                return GameManager.Singleton.upgrades.assistantFlavorText;
            case Upgrades.UpgradeTypes.dismissal:
                return GameManager.Singleton.upgrades.dismissalFlavorText;
            case Upgrades.UpgradeTypes.hellishContract:
                return GameManager.Singleton.upgrades.hellishContractFlavorText;
            case Upgrades.UpgradeTypes.powerFistRequisition:
                return GameManager.Singleton.upgrades.powerFistRequisitionFlavorText;
            case Upgrades.UpgradeTypes.loanAgreement:
                return GameManager.Singleton.upgrades.loanAgreementFlavorText;
            case Upgrades.UpgradeTypes.temporaryEmploymentContract:
                return GameManager.Singleton.upgrades.temporaryEmploymentContractFlavorText;
            case Upgrades.UpgradeTypes.endOfLifePlan:
                return GameManager.Singleton.upgrades.endOfLifePlanFlavorText;
            default:
                return "error";
        }
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
                case Upgrades.UpgradeTypes.dismissal:
                    relevantText = GameManager.Singleton.upgrades.dismissalFlavorText;
                    break;
                case Upgrades.UpgradeTypes.hellishContract:
                    relevantText = GameManager.Singleton.upgrades.hellishContractFlavorText;
                    break;
                case Upgrades.UpgradeTypes.powerFistRequisition:
                    relevantText = GameManager.Singleton.upgrades.powerFistRequisitionFlavorText;
                    break;
                case Upgrades.UpgradeTypes.loanAgreement:
                    relevantText = GameManager.Singleton.upgrades.loanAgreementFlavorText;
                    break;
                case Upgrades.UpgradeTypes.temporaryEmploymentContract:
                    relevantText = GameManager.Singleton.upgrades.temporaryEmploymentContractFlavorText;
                    break;
                case Upgrades.UpgradeTypes.endOfLifePlan:
                    relevantText = GameManager.Singleton.upgrades.endOfLifePlanFlavorText;
                    break;
                default:
                    break;
            }

            GUI.Label(_flavorRect, relevantText);
        }
    }
}
