using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Scripting.Customer;
using Scripting.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Scripting
{
    public class GameManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] public Upgrades upgrades;

        [Header("Gameplay")]
        [SerializeField] private float dayLength = 60.0f * 2.5f;

        [Header("Components")]
        [SerializeField] private ShiftManager shiftManager;

        [Header("Map")]
        [SerializeField] private List<GameObject> spawnPoints;

        [SerializeField] private List<AiSpot> aiSpots;
        [SerializeField] private List<GameObject> customerPrefabs;

        [Header("Graphics")]
        [SerializeField] private GameObject mainCanvas;

        [SerializeField] private Image fade;

        [Header("Items")]
        [SerializeField] public GameObject distractionChairs;

        [SerializeField] public GameObject distractionPaintings;
        [SerializeField] public GameObject baseballBat;
        [SerializeField] public GameObject cigar;
        [SerializeField] public GameObject phone;
        [SerializeField] public GameObject assistant;
        [SerializeField] public GameObject bodyguard;

        public GameObject MainCanvas => mainCanvas;

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

        private void Awake()
        {
            Singleton = this;
            IsNightTime = true;
            
            fade.gameObject.SetActive(true);
            StartCoroutine(FadeOut());
        }

        private void Start()
        {
            SpecialStoreManager.Singleton.SetRandomUpgrade();
            shiftManager.SetIsNightTime(true);
        }

        private IEnumerator FadeOut()
        {
            var elapsedTime = 0f;
            while (elapsedTime < 2f)
            {
                elapsedTime += Time.deltaTime;

                var fadeValue = (2f - elapsedTime);
                if (fadeValue < 0) fadeValue = 0;

                fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, fadeValue);

                yield return null;
            }
        }

        private float _dayTimer;
        private float _spawnTimer;

        public bool IsNightTime { get; private set; }

        public void StartDay()
        {
            IsNightTime = false;
            
            shiftManager.SetIsNightTime(false);
            _dayTimer = dayLength;
        }

        private void Update()
        {
            if (IsNightTime) return;

            _dayTimer -= Time.deltaTime;

            shiftManager.LerpShiftState((dayLength - _dayTimer) / dayLength);

            if (_dayTimer <= 0)
            {
                DayFinished();
            }

            _spawnTimer -= Time.deltaTime;
            // SPAWNRATE CALL "SpawnCustomer()";

            if (upgrades.money <= 0)
            {
                Bankrupt();
            }

            if (_spawnTimer <= 0)
            {
                _spawnTimer = 5.0f;
                SpawnCustomer();
            }
        }

        public void Bankrupt()
        {
            if (IsNightTime) return;
            // Wasted
            IsNightTime = true;
        }

        public void StressmeterTooHigh()
        {
            // if (IsNightTime) return;
            // // Wasted
            // IsNightTime = true;
        }

        public void DayFinished()
        {
            IsNightTime = true;
            SpecialStoreManager.Singleton.SetRandomUpgrade();
            shiftManager.SetIsNightTime(true);
            
            _customerMotors.ForEach(n => n.WalkOut());
            _customerMotors.Clear();
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
            return aiSpots;
        }

        #region graveyard

        private void InitialUpgrades()
        {
            if (upgrades.chairs)
            {
                distractionChairs.SetActive(true);
                aiSpots = aiSpots.Concat(distractionChairs.GetComponentsInChildren<AiSpot>()).ToList();
            }

            if (upgrades.paintings)
            {
                distractionPaintings.SetActive(true);
                aiSpots = aiSpots.Concat(distractionPaintings.GetComponentsInChildren<AiSpot>()).ToList();
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

        #endregion graveyard
    }
}
