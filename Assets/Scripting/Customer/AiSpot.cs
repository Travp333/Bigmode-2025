using UnityEngine;

namespace AI
{
    public class AiSpot : MonoBehaviour
    {
        public bool IsBought { get; set; } = true;

        public bool IsOccupied { get; private set; }

        public void SetIsOccupied(bool value)
        {
            IsOccupied = value;
        }
    }
}
