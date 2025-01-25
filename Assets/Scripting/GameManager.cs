using System.Collections.Generic;
using System.Linq;
using AI;
using Scripting.Customer;
using Scripting.ScriptableObjects;
using UnityEngine;

namespace Scripting
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> spawnPoints;
        [SerializeField] private List<GameObject> customerPrefabs;

        [SerializeField] private GameObject customerChairEnterSpot;

        [SerializeField] private Upgrades upgrades;

        [SerializeField] private GameObject distractionChairs;
        [SerializeField] private GameObject distractionPaintings;

        [SerializeField] private GameObject baseballBat;
        [SerializeField] private GameObject cigar;

        private readonly List<CustomerMotor> _customerMotors = new();
        private CustomerMotor _currentCustomer;

        private List<AiSpot> _aiSpots = new();

        private void Awake()
        {
            if (upgrades.chairs)
            {
                distractionChairs.SetActive(true);
                _aiSpots = _aiSpots.Concat(distractionChairs.GetComponentsInChildren<AiSpot>()).ToList();
            }

            if (upgrades.paintings)
            {
                distractionPaintings.SetActive(true);
                _aiSpots = _aiSpots.Concat(distractionPaintings.GetComponentsInChildren<AiSpot>()).ToList();
            }

            if (upgrades.baseballBat)
            {
                baseballBat.SetActive(true);
            }

            if (upgrades.cigar)
            {
                cigar.SetActive(true);
            }
        }

        public void SpawnCustomer()
        {
            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            var customerPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];

            var go = Instantiate(customerPrefab, spawnPoint.transform.position, Quaternion.identity);

            _customerMotors.Add(go.GetComponent<CustomerMotor>());
        }

        public void FinalizeCustomer()
        {
            if (!_currentCustomer) return;

            _currentCustomer.LeaveDesk();
            _customerMotors.Remove(_currentCustomer);
        }

        public List<AiSpot> GetDistractionSpots()
        {
            return _aiSpots;
        }
    }
}
