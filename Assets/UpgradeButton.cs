using Scripting;
using Scripting.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
    public Upgrades.UpgradeTypes myUpgradeType;
    public float moneyAmount;
    public bool beenPressed;
    private bool _bShowFlavorText;
    private int _flavorTextCountdown;
    private Rect _flavorRect;
    [SerializeField] private UpgradeButton twin;
    private Texture _defaultTexture;
    [SerializeField] private Texture testTexture;
    private MeshRenderer _myMeshRenderer;
    public bool special;

    private void Awake()
    {
        _myMeshRenderer = GetComponent<MeshRenderer>();
        _defaultTexture = _myMeshRenderer.material.mainTexture;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        var temp = GameManager.Singleton.MainCanvas.GetComponent<RectTransform>();
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
            case Upgrades.UpgradeTypes.Chairs:
                GameManager.Singleton.upgrades.chairs = true;
                GameManager.Singleton.distractionChairs.SetActive(true);
                // GameManager.Singleton._aiSpots = GameManager.Singleton._aiSpots.Concat(GameManager.Singleton.distractionChairs.GetComponentsInChildren<AiSpot>()).ToList();
                break;
            case Upgrades.UpgradeTypes.Paintings:
                GameManager.Singleton.upgrades.paintings = true;
                GameManager.Singleton.distractionPaintings.SetActive(true);
                // GameManager.Singleton._aiSpots = GameManager.Singleton._aiSpots.Concat(GameManager.Singleton.distractionPaintings.GetComponentsInChildren<AiSpot>()).ToList();
                break;
            case Upgrades.UpgradeTypes.BaseballBat:
                GameManager.Singleton.upgrades.baseballBat = true;
                GameManager.Singleton.baseballBat.SetActive(true);
                break;
            case Upgrades.UpgradeTypes.Cigar:
                GameManager.Singleton.upgrades.cigar = true;
                GameManager.Singleton.cigar.SetActive(true);
                break;
            case Upgrades.UpgradeTypes.Phone:
                GameManager.Singleton.upgrades.phone = true;
                GameManager.Singleton.phone.SetActive(true);
                break;
            case Upgrades.UpgradeTypes.Bodyguard:
                GameManager.Singleton.upgrades.bodyguard = true;
                GameManager.Singleton.bodyguard.SetActive(true);
                break;
            case Upgrades.UpgradeTypes.Assistant:
                GameManager.Singleton.upgrades.assistant = true;
                GameManager.Singleton.assistant.SetActive(true);
                break;
            case Upgrades.UpgradeTypes.Money:
                GameManager.Singleton.upgrades.money += moneyAmount;
                break;
            case Upgrades.UpgradeTypes.Dismissal:
                GameManager.Singleton.upgrades.dismissal = true;
                break;
            case Upgrades.UpgradeTypes.HellishContract:
                GameManager.Singleton.upgrades.hellishContract = true;
                break;
            case Upgrades.UpgradeTypes.PowerFistRequisition:
                GameManager.Singleton.upgrades.powerFistRequisition = true;
                break;
            case Upgrades.UpgradeTypes.LoanAgreement:
                GameManager.Singleton.upgrades.loanAgreement = true;
                break;
            case Upgrades.UpgradeTypes.TemporaryEmploymentContract:
                GameManager.Singleton.upgrades.temporaryEmploymentContract = true;
                break;
            case Upgrades.UpgradeTypes.EndOfLifePlan:
                GameManager.Singleton.upgrades.endOfLifePlan = true;
                break;
            default:
                break;
        }

        beenPressed = true;
        if (special)
        {
            List<Material> temp = _myMeshRenderer.materials.ToList();
            temp[1] = SpecialStoreManager.Singleton.fetchUpgradeMaterial(14);
            _myMeshRenderer.SetMaterials(temp);
        }
        else
        {
            if (testTexture != null)
            {
                _myMeshRenderer.material.mainTexture = testTexture;
                _myMeshRenderer.material.DisableKeyword("_EMISSION");
            }
            if (twin != null)
            {
                twin.beenPressed = beenPressed;
                if (twin.testTexture != null)
                {
                    _myMeshRenderer.material.mainTexture = testTexture;
                    _myMeshRenderer.material.DisableKeyword("_EMISSION");
                }
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
            case Upgrades.UpgradeTypes.Chairs:
                return GameManager.Singleton.upgrades.chairsFlavorText;
            case Upgrades.UpgradeTypes.Paintings:
                return GameManager.Singleton.upgrades.paintingsFlavorText;
            case Upgrades.UpgradeTypes.BaseballBat:
                return GameManager.Singleton.upgrades.baseballBatFlavorText;
            case Upgrades.UpgradeTypes.Cigar:
                return GameManager.Singleton.upgrades.cigarFlavorText;
            case Upgrades.UpgradeTypes.Phone:
                return GameManager.Singleton.upgrades.phoneFlavorText;
            case Upgrades.UpgradeTypes.Bodyguard:
                return GameManager.Singleton.upgrades.bodyguardFlavorText;
            case Upgrades.UpgradeTypes.Assistant:
                return GameManager.Singleton.upgrades.assistantFlavorText;
            case Upgrades.UpgradeTypes.Dismissal:
                return GameManager.Singleton.upgrades.dismissalFlavorText;
            case Upgrades.UpgradeTypes.HellishContract:
                return GameManager.Singleton.upgrades.hellishContractFlavorText;
            case Upgrades.UpgradeTypes.PowerFistRequisition:
                return GameManager.Singleton.upgrades.powerFistRequisitionFlavorText;
            case Upgrades.UpgradeTypes.LoanAgreement:
                return GameManager.Singleton.upgrades.loanAgreementFlavorText;
            case Upgrades.UpgradeTypes.TemporaryEmploymentContract:
                return GameManager.Singleton.upgrades.temporaryEmploymentContractFlavorText;
            case Upgrades.UpgradeTypes.EndOfLifePlan:
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
                case Upgrades.UpgradeTypes.Chairs:
                    relevantText = GameManager.Singleton.upgrades.chairsFlavorText;
                    break;
                case Upgrades.UpgradeTypes.Paintings:
                    relevantText = GameManager.Singleton.upgrades.paintingsFlavorText;
                    break;
                case Upgrades.UpgradeTypes.BaseballBat:
                    relevantText = GameManager.Singleton.upgrades.baseballBatFlavorText;
                    break;
                case Upgrades.UpgradeTypes.Cigar:
                    relevantText = GameManager.Singleton.upgrades.cigarFlavorText;
                    break;
                case Upgrades.UpgradeTypes.Phone:
                    relevantText = GameManager.Singleton.upgrades.phoneFlavorText;
                    break;
                case Upgrades.UpgradeTypes.Bodyguard:
                    relevantText = GameManager.Singleton.upgrades.bodyguardFlavorText;
                    break;
                case Upgrades.UpgradeTypes.Assistant:
                    relevantText = GameManager.Singleton.upgrades.assistantFlavorText;
                    break;
                case Upgrades.UpgradeTypes.Dismissal:
                    relevantText = GameManager.Singleton.upgrades.dismissalFlavorText;
                    break;
                case Upgrades.UpgradeTypes.HellishContract:
                    relevantText = GameManager.Singleton.upgrades.hellishContractFlavorText;
                    break;
                case Upgrades.UpgradeTypes.PowerFistRequisition:
                    relevantText = GameManager.Singleton.upgrades.powerFistRequisitionFlavorText;
                    break;
                case Upgrades.UpgradeTypes.LoanAgreement:
                    relevantText = GameManager.Singleton.upgrades.loanAgreementFlavorText;
                    break;
                case Upgrades.UpgradeTypes.TemporaryEmploymentContract:
                    relevantText = GameManager.Singleton.upgrades.temporaryEmploymentContractFlavorText;
                    break;
                case Upgrades.UpgradeTypes.EndOfLifePlan:
                    relevantText = GameManager.Singleton.upgrades.endOfLifePlanFlavorText;
                    break;
                default:
                    break;
            }

            GUI.Label(_flavorRect, relevantText);
        }
    }

    public void unpress()
    {
        if (beenPressed)
        {
            correctMaterial();
            beenPressed = false;
        }
    }

    public void correctMaterial()
    {
        if (special)
        {
            List<Material> temp = _myMeshRenderer.materials.ToList();
            temp[1] = SpecialStoreManager.Singleton.fetchUpgradeMaterial(myUpgradeType);
            _myMeshRenderer.SetMaterials(temp);
        }
        else
        {
            _myMeshRenderer.material.mainTexture = _defaultTexture;
            _myMeshRenderer.material.EnableKeyword("_EMISSION");
        }
    }
}
