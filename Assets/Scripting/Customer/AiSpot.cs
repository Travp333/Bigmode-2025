using System;
using UnityEditor;
using UnityEngine;

namespace Scripting.Customer
{
    public class AiSpot : MonoBehaviour
    {
        public bool isChair;
        public bool IsOccupied { get; private set; }

        public void Lock(Action callback = null)
        {
            IsOccupied = true;
            
            callback?.Invoke();
        }

        public void Unlock()
        {
            IsOccupied = false;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.up * 0.5f,
                "IsOccupied: " + IsOccupied);
     
        }
#endif
    }
}
