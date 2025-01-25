using System.Collections;
using UnityEngine;

namespace Scripting.Desk
{
    public class ContractStack : MonoBehaviour
    {
        [SerializeField] private GameObject contract;
        [SerializeField] private Transform downPoint;
        [SerializeField] private Transform upPoint;
        [SerializeField] private GameObject attachPoint;

        private GameObject _currentContract;

        // private 
        
        private void OnMouseDown()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _currentContract = Instantiate(contract, downPoint.position, downPoint.rotation, attachPoint.transform);

                // StartCoroutine(DoUpAnimation());
            }

            if (Input.GetMouseButtonDown(1))
            {
                Destroy(_currentContract);
                _currentContract = null;
            }
        }
    }
}
