using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripting.Customer;
using Scripting.Player;
using Scripting.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Scripting
{
    public class GameManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] public Upgrades upgrades;

        [Header("Gameplay")]
        [SerializeField] private float dayLength = 60.0f * 2.5f;

        [SerializeField] private float loanAgreementTime = 30.0f;

        [Header("Components")]
        [SerializeField] private ShiftManager shiftManager;
        
        [Header("Map")]
        [SerializeField] private List<GameObject> spawnPoints;
        
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
        
        [Header("Player")]
        [SerializeField] private Movement player;
        
        [Header("Enemies")]
        
        [SerializeField] private List<GameObject> customerPrefabs;


        public GameObject MainCanvas => mainCanvas;
        public Movement Player => player;
        public bool IsLoanAgreementRunning => _loanAgreementRunning > 0f;
        
        private readonly List<CustomerMotor> _customerMotors = new();

        private int _maxCustomers;
        private int _level;
        private float _loanAgreementRunning;
        
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

        private void SetInitValues()
        {
            upgrades.money = 1000f;
            
            upgrades.chairs = false;
            upgrades.paintings = false;
            upgrades.baseballBat = false;
            upgrades.cigar = false;
            upgrades.phone = false;
            upgrades.bodyguard = false;
            upgrades.assistant = false;
            upgrades.dismissal = false;
            upgrades.hellishContract = false;
            upgrades.powerFistRequisition = false;
            upgrades.loanAgreement = false;
            upgrades.temporaryEmploymentContract = false;
            upgrades.endOfLifePlan = false;
        }
        
        private void Awake()
        {
            SetInitValues();
            
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

                var fadeValue = 2f - elapsedTime;
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
            _level++;
            
            _maxCustomers = _level * 5 + _level;
        }

        private void Update()
        {
            if (IsNightTime) return;

            _dayTimer -= Time.deltaTime;
            _loanAgreementRunning -= Time.deltaTime;

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
                _spawnTimer = 10.0f;
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
            AiController.Singleton.UnlockEverything();
            SpecialStoreManager.Singleton.SetRandomUpgrade();
            shiftManager.SetIsNightTime(true);
            
            _customerMotors.ForEach(n => n.WalkOut());
            _customerMotors.Clear();
        }

        public void SpawnCustomer()
        {
            if (_customerMotors.Count <= _maxCustomers)
            {
                var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                var customerPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];

                var go = Instantiate(customerPrefab, spawnPoint.transform.position, Quaternion.identity);

                _customerMotors.Add(go.GetComponent<CustomerMotor>());
            }
        }

        public void RemoveCustomer(CustomerMotor customerMotor)
        {
            _customerMotors.Remove(customerMotor);
        }
        
        #region graveyard

        private void InitialUpgrades()
        {
            if (upgrades.chairs)
            {
                distractionChairs.SetActive(true);
            }

            if (upgrades.paintings)
            {
                distractionPaintings.SetActive(true);
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
       
        public void ActivateLoanAgreement()
        {
            _loanAgreementRunning = loanAgreementTime;
            upgrades.loanAgreement = false;
        }

        public void Dismissal()
        {
            _customerMotors.ToList().ForEach(n =>
            {
                n.Pay();
                n.WalkOut();
            });
            
            _customerMotors.Clear();
            
            upgrades.dismissal = false;
        }

        public void DoPentagrammLogic()
        {
            // TODO: implement
            
            
            upgrades.hellishContract = false;
        }

        public void DoFistStuff()
        {
            // TODO: implement
            
            upgrades.powerFistRequisition = false;
        }

        public void SpawnTec()
        {
            // TODO: implement - is part of AI and Balancing
            
            upgrades.temporaryEmploymentContract = false;
        }

        public void GetExtraLife()
        {
            // TODO: implement
            
            upgrades.endOfLifePlan = false;
        }

        public void ResetScene()
        {
            SceneManager.LoadScene("Office", LoadSceneMode.Single);
        }
        
        private void OnGUI()
        {
            var text = string.Empty;
            
            text += "Active Power: \n";
            
            text += $"La: {upgrades.loanAgreement}\n";
            text += $"Tec: {upgrades.temporaryEmploymentContract}\n";
            text += $"Eel: {upgrades.endOfLifePlan}\n";
            text += $"Ds: {upgrades.dismissal}\n";
            text += $"Penta: {upgrades.hellishContract}\n";
            text += $"Fist: {upgrades.powerFistRequisition}\n";
            text += $"Money: {upgrades.money}\n";
            
            GUI.Label(new Rect(5, Screen.height/2f, 200, 500),text);
        }

        private List<GameObject> _vandalismSpots = new();
        
        public void RegisterVandalismSpot(GameObject aiSpot)
        {
            _vandalismSpots.Add(aiSpot);
        }

        public void RemoveVandalismSpot(GameObject aiSpot)
        {
            _vandalismSpots.Remove(aiSpot);
        }
    }
}
