using UnityEngine;

namespace Scripting.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Upgrades", menuName = "Scriptable Objects/Upgrades")]
    public class Upgrades : ScriptableObject
    {
        //upgrade types here
        public enum UpgradeTypes
        {
            Chairs,
            Paintings,
            BaseballBat,
            Cigar,
            Phone,
            Bodyguard,
            Assistant,
            Money,
            Dismissal,
            HellishContract,
            PowerFistRequisition,
            LoanAgreement,
            TemporaryEmploymentContract,
            EndOfLifePlan
        }

        //upgrade status here?
        public bool chairs;
        public bool paintings;
        public bool baseballBat;
        public bool cigar;
        public bool phone;
        public bool bodyguard;
        public bool assistant;
        
        public int money;

        public int priceChairs = 1;
        public int pricePaintings = 1;
        public int priceBaseballBat = 1;
        public int priceCigar = 1;
        public int pricePhone = 1;
        public int priceBodyguard = 1;
        public int priceAssistant = 1;
            
        public bool dismissal;
        public bool hellishContract;
        public bool powerFistRequisition;
        public bool loanAgreement;
        public bool temporaryEmploymentContract;
        public bool endOfLifePlan;
        
        public string chairsFlavorText = "adds chairs";
        public string paintingsFlavorText = "adds paintings";
        public string baseballBatFlavorText = "adds baseball bat";
        public string cigarFlavorText = "adds cigar";
        public string phoneFlavorText = "adds phone";
        public string bodyguardFlavorText = "adds bodyguard";
        public string assistantFlavorText = "adds assistant";

        public string dismissalFlavorText = "adds dismissal";
        public string hellishContractFlavorText = "adds hellish contract";
        public string powerFistRequisitionFlavorText = "adds power fist requisition";
        public string loanAgreementFlavorText = "adds loan agreement";
        public string temporaryEmploymentContractFlavorText = "adds temporary employment contract";
        public string endOfLifePlanFlavorText = "adds end of life plan";
    }
}
