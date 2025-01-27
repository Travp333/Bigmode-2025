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

        [SerializeField] private Upgrades upgrades;

        [SerializeField] private GameObject distractionChairs;
        [SerializeField] private GameObject distractionPaintings;

        [SerializeField] private GameObject baseballBat;
        [SerializeField] private GameObject cigar;
        [SerializeField] private GameObject phone;
        [SerializeField] private GameObject assistant;
        [SerializeField] private GameObject bodyguard;
        private readonly List<CustomerMotor> _customerMotors = new();
        private CustomerMotor _currentCustomer;

        private static GameManager _singleton;

        public static GameManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    Debug.Log($"{nameof(GameManager)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }

        private List<AiSpot> _aiSpots = new();

        private void Awake()
        {
            Singleton = this;

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

            if (upgrades.phone)
            {
                phone.SetActive(true);
            }
            if (upgrades.assistant)
            {
                assistant.SetActive(true);
            }   
            if (upgrades.bodyguard)
            {
                bodyguard.SetActive(true);
            }
            
        }

        private float _dayTimer = 60.0f;
        private float _spawnTimer = 0.0f;

        private bool _dayOver;

        void Update()
        {
            if (_dayOver) return;

            // SPAWNRATE CALL "SpawnCustomer()";

            if (upgrades.money <= 0)
            {
                Bankrupt();
            }

            _dayTimer -= Time.deltaTime;
            _spawnTimer -= Time.deltaTime;
         
            if (_dayTimer <= 0)
            {
                DayFinished();
            }

            if (_spawnTimer <= 0)
            {
                _spawnTimer = 5.0f;
                SpawnCustomer();
            }
        }

        public void Bankrupt()
        {
            if (_dayOver) return;
            // Wasted
            _dayOver = true;
        }

        public void StressmeterTooHigh()
        {
            if (_dayOver) return;
            // Wasted
            _dayOver = true;
        }

        public void DayFinished()
        {
            if (_dayOver) return;
            // Day is finished -> Shop-Scene
            _dayOver = true;
        }

        public void SpawnCustomer()
        {
            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            var customerPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];

            var go = Instantiate(customerPrefab, spawnPoint.transform.position, Quaternion.identity);

            _customerMotors.Add(go.GetComponent<CustomerMotor>());
        }

        public void FinalizeCustomer(CustomerMotor customerMotor)
        {
            _customerMotors.Remove(customerMotor);
        }

        public List<AiSpot> GetDistractionSpots()
        {
            return _aiSpots;
        }
    }
}
