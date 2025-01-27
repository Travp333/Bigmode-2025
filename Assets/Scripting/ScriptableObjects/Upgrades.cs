using NUnit.Framework;
using System;
using UnityEngine;

namespace Scripting.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Upgrades", menuName = "Scriptable Objects/Upgrades")]
    public class Upgrades : ScriptableObject
    {
        //upgrade types here
        public enum UpgradeTypes
        {
            chairs,
            paintings,
            baseballBat,
            cigar,
            phone,
            bodyguard,
            assistant,
            money
        }

        //upgrade status here?
        public bool chairs;
        public bool paintings;
        public bool baseballBat;
        public bool cigar;
        public bool phone;
        public bool bodyguard;
        public bool assistant;
        public float money;

        public string chairsFlavorText = "adds chairs";
        public string paintingsFlavorText = "adds paintings";
        public string baseballBatFlavorText = "adds baseball bat";
        public string cigarFlavorText = "adds cigar";
        public string phoneFlavorText = "adds phone";
        public string bodyguardFlavorText = "adds bodyguard";
        public string assistantFlavorText = "adds assistant";
    }

}
