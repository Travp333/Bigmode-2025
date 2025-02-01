using System;
using UnityEngine;

namespace Scripting.Customer
{
    public class AiSpot : MonoBehaviour
    {
        public bool isChair;
        public bool IsOccupied { get; private set; }

        public void Arrive(Action callback = null)
        {
            IsOccupied = true;
            
            callback?.Invoke();
        }

        public void Leave()
        {
            IsOccupied = false;
        }
    }
}
