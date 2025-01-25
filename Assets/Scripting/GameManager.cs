using System.Collections.Generic;
using System.Linq;
using Scripting.Customer;
using UnityEngine;

namespace Scripting
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> spawnPoints;
        [SerializeField] private List<GameObject> customerPrefabs;

        [SerializeField] private GameObject customerChairEnterSpot;
        [SerializeField] private GameObject customerChairLeaveSpot;

        private readonly List<CustomerMotor> _customerMotors = new();
        private CustomerMotor _currentCustomer;

        public void SpawnCustomer()
        {
            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            var customerPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];

            var go = Instantiate(customerPrefab, spawnPoint.transform.position, Quaternion.identity);

            _customerMotors.Add(go.GetComponent<CustomerMotor>());
        }

        public void NextCustomer()
        {
            _currentCustomer = _customerMotors.OrderBy(n => n.StressMeter).FirstOrDefault();
            if (_currentCustomer)
            {
                _currentCustomer.GoToDesk(customerChairEnterSpot.transform);
            }
        }

        public void FinalizeCustomer()
        {
            if (!_currentCustomer) return;
            
            _currentCustomer.LeaveDesk();
            _customerMotors.Remove(_currentCustomer);
        }
    }
}
