using UnityEngine;

namespace Scripting.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Upgrades", menuName = "Scriptable Objects/Upgrades")]
    public class Upgrades : ScriptableObject
    {
        public bool chairs;
        public bool paintings;
        public bool baseballBat;
        public bool cigar;
        public bool phone;
        
        public float money;
    }
}
