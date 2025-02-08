using System.Collections.Generic;
using System.Linq;
using Scripting.Customer;
using Scripting.ScriptableObjects;
using UnityEngine;

namespace Scripting
{
    public class UpgradeButton : MonoBehaviour
    {
        public Upgrades.UpgradeTypes myUpgradeType;
        public bool beenPressed;
        private bool _bShowFlavorText;
        private int _flavorTextCountdown;
        private Rect _flavorRect;
        [SerializeField] private UpgradeButton twin;
        private Texture _defaultTexture;
        [SerializeField] private Texture testTexture;
        public MeshRenderer _myMeshRenderer;
        public bool special;

        [SerializeField]
        bool blocked;

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
            if (blocked)
            {
                return;
            }

            if (beenPressed)
            {
                return;
            }

            var man = GameManager.Singleton;

            switch (myUpgradeType)
            {
                case Upgrades.UpgradeTypes.Chairs:
                    if (man.PayChairs())
                    {
                        man.upgrades.chairs = true;
                        man.distractionChairs.SetActive(true);
                        AiController.Singleton.ActivateChairDistractionSpots();
                    }
                    else
                    {
                        return;
                    }

                    break;
                case Upgrades.UpgradeTypes.Paintings:
                    if (man.PayPaintings())
                    {
                        man.upgrades.paintings = true;
                        man.distractionPaintings.SetActive(true);
                        AiController.Singleton.ActivatePaintingDistractionSpots();
                    }
                    else
                    {
                        return;
                    }

                    break;
                case Upgrades.UpgradeTypes.BaseballBat:
                    if (man.PayBaseballBat())
                    {
                        man.upgrades.baseballBat = true;
                        man.baseballBat.SetActive(true);
                    }
                    else
                    {
                        return;
                    }

                    break;
                case Upgrades.UpgradeTypes.Cigar:
                    if (man.PayCigar())
                    {
                        man.upgrades.cigar = true;
                        man.cigar.SetActive(true);
                    }
                    else
                    {
                        return;
                    }

                    break;
                case Upgrades.UpgradeTypes.Phone:
                    if (man.PayPhone())
                    {
                        man.upgrades.phone = true;
                        man.phone.SetActive(true);
                    }
                    else
                    {
                        return;
                    }

                    break;
                case Upgrades.UpgradeTypes.Bodyguard:
                    if (man.PayBodyguard())
                    {
                        man.upgrades.bodyguard = true;
                        man.bodyguard.SetActive(true);
                    }
                    else
                    {
                        return;
                    }

                    break;
                case Upgrades.UpgradeTypes.Assistant:
                    if (man.PayAssistant())
                    {
                        man.upgrades.assistant = true;
                        man.assistant.SetActive(true);
                        AiController.Singleton.SetAssistantActive();
                    }
                    else
                    {
                        return;
                    }

                    break;
                case Upgrades.UpgradeTypes.Dismissal:
                    man.upgrades.dismissal = true;
                    break;
                case Upgrades.UpgradeTypes.HellishContract:
                    man.upgrades.hellishContract = true;
                    break;
                case Upgrades.UpgradeTypes.PowerFistRequisition:
                    man.upgrades.powerFistRequisition = true;
                    break;
                case Upgrades.UpgradeTypes.LoanAgreement:
                    man.upgrades.loanAgreement = true;
                    break;
                case Upgrades.UpgradeTypes.TemporaryEmploymentContract:
                    man.upgrades.temporaryEmploymentContract = true;
                    break;
                case Upgrades.UpgradeTypes.EndOfLifePlan:
                    man.upgrades.endOfLifePlan = true;
                    break;
                default:
                    break;
            }

            beenPressed = true;
            if (special)
            {
                CRT.Instance.Vend();
                List<Material> temp = _myMeshRenderer.materials.ToList();
                temp[1] = SpecialStoreManager.Singleton.fetchUpgradeMaterial(14);
                _myMeshRenderer.SetMaterials(temp);
            }
            else
            {
                VendingMachine.Instance.Vend();
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
                        twin._myMeshRenderer.material.mainTexture = twin.testTexture;
                        twin._myMeshRenderer.material.DisableKeyword("_EMISSION");
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
                    return GameManager.Singleton.upgrades.chairsFlavorText.Replace("$X",
                        $"${GameManager.Singleton.upgrades.priceChairs}");
                case Upgrades.UpgradeTypes.Paintings:
                    return GameManager.Singleton.upgrades.paintingsFlavorText.Replace("$X",
                        $"${GameManager.Singleton.upgrades.pricePaintings}");
                case Upgrades.UpgradeTypes.BaseballBat:
                    return GameManager.Singleton.upgrades.baseballBatFlavorText.Replace("$X",
                        $"${GameManager.Singleton.upgrades.priceBaseballBat}");
                case Upgrades.UpgradeTypes.Cigar:
                    return GameManager.Singleton.upgrades.cigarFlavorText.Replace("$X",
                        $"${GameManager.Singleton.upgrades.priceCigar}");
                case Upgrades.UpgradeTypes.Phone:
                    return GameManager.Singleton.upgrades.phoneFlavorText.Replace("$X",
                        $"${GameManager.Singleton.upgrades.pricePhone}");
                case Upgrades.UpgradeTypes.Bodyguard:
                    return GameManager.Singleton.upgrades.bodyguardFlavorText.Replace("$X",
                        $"${GameManager.Singleton.upgrades.priceBodyguard}");
                case Upgrades.UpgradeTypes.Assistant:
                    return GameManager.Singleton.upgrades.assistantFlavorText.Replace("$X",
                        $"${GameManager.Singleton.upgrades.priceAssistant}");
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
                //
                // if (GetComponent<Outline>().OutlineActive)
                // {
                //     
                // }
                //
                _myMeshRenderer.SetMaterials(temp);
            }
            else
            {
                _myMeshRenderer.material.mainTexture = _defaultTexture;
                _myMeshRenderer.material.EnableKeyword("_EMISSION");
            }
        }
    }
}
