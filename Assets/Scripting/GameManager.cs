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
        [SerializeField]
        private GameObject uiPowerDocumentHell,
            uiPowerDocumentDismissal,
            uiPowerDocumentFist,
            uiPowerDocumentLoan,
            uiPowerDocumentTEC,
            uiPowerDocumentEOL;

        [Header("Data")]
        [SerializeField] public Upgrades upgrades;

        [Header("Gameplay")]
        [SerializeField] private float dayLength = 60.0f * 2.5f;

        [SerializeField] private float loanAgreementTime = 30.0f;
        [SerializeField] private float devilDealTime = 45.0f;
        [SerializeField] private bool nightResetsTimer;

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

        [Header("Huell")]
        [SerializeField] private GameObject huell;

        [SerializeField] private int numHuells = 3;

        public bool IsIncreasedMotherfuckerSpawn { get; set; }
        public bool IsDecreasedMotherfuckerSpawn { get; set; }
        public bool IsLoanAgreementRunning => _loanAgreementRunning > 0f;
        public bool IsDevilTimeRunning => _devilTime > 0.0f;

        private float _increasedMotherfuckerTimer = 0.0f;
        private float _decreasedMotherfuckerTimer = 0.0f;
        private float _devilTime = 0.0f;

        public GameObject MainCanvas => mainCanvas;
        public Movement Player => player;
        private readonly List<CustomerMotor> _customerMotors = new();

        private int _maxCustomers;
        private int _level = 1;
        private float _loanAgreementRunning;
        private float _moneyInSafe;

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
            upgrades.temporaryEmploymentContract = true;
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

            _maxCustomers = _level * 5 + _level;
        }

        private void Update()
        {
            if (IsNightTime) return;

            _dayTimer -= Time.deltaTime;
            _loanAgreementRunning -= Time.deltaTime;
            _increasedMotherfuckerTimer -= Time.deltaTime;
            _decreasedMotherfuckerTimer -= Time.deltaTime;
            _devilTime -= Time.deltaTime;
            _spawnTimer -= Time.deltaTime;

            if (_loanAgreementRunning < 0f)
            {
                _loanAgreementRunning = 0f;
            }

            if (_increasedMotherfuckerTimer < 0f)
            {
                _increasedMotherfuckerTimer = 0f;
            }

            if (_decreasedMotherfuckerTimer < 0f)
            {
                _decreasedMotherfuckerTimer = 0f;
            }

            if (_devilTime < 0f)
            {
                _devilTime = 0f;
            }

            shiftManager.LerpShiftState((dayLength - _dayTimer) / dayLength);

            if (_dayTimer <= 0)
            {
                DayFinished();
            }
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

        private bool _endOfLifePlan;

        public void DayFinished()
        {
            IsNightTime = true;
            AiController.Singleton.UnlockEverything();
            SpecialStoreManager.Singleton.SetRandomUpgrade();
            shiftManager.SetIsNightTime(true);

            if (nightResetsTimer)
            {
                _loanAgreementRunning = 0.0f;
                _increasedMotherfuckerTimer = 0.0f;
                _decreasedMotherfuckerTimer = 0.0f;
                _devilTime = 0.0f;
            }

            var todaysQuota = GetCurrentQuota();

            if (upgrades.money > todaysQuota)
            {
                upgrades.money -= todaysQuota;
                _moneyInSafe += todaysQuota;
            }
            else
            {
                if (_endOfLifePlan)
                {
                    _endOfLifePlan = false;
                    _moneyInSafe += todaysQuota;
                    upgrades.money = 0;
                    Debug.Log("END OF LIFE PLAN ACTIVATED");
                }
                else
                {
                    PlayDeathScene();
                }
            }

            _level++;

            _customerMotors.ForEach(n => n.WalkOut());
            _customerMotors.Clear();
        }

        public float GetCurrentQuota()
        {
            return 40000f * Mathf.Pow(1.165f, _level - 1);
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

        public void SpawnMotherfucker(bool force = false)
        {
            if (_customerMotors.Count <= _maxCustomers || force)
            {
                var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                var customerPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];

                var go = Instantiate(customerPrefab, spawnPoint.transform.position, Quaternion.identity);

                var comp = go.GetComponent<CustomerMotor>();
                _customerMotors.Add(comp);

                comp.SetIsMotherfucker(true);
            }
        }

        public void SpawnNormalGuy(bool force = false)
        {
            if (_customerMotors.Count <= _maxCustomers || force)
            {
                var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                var customerPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];

                var go = Instantiate(customerPrefab, spawnPoint.transform.position, Quaternion.identity);

                var comp = go.GetComponent<CustomerMotor>();
                _customerMotors.Add(comp);

                comp.SetIsMotherfucker(false);
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
            if (IsIncreasedMotherfuckerSpawn)
            {
                _increasedMotherfuckerTimer = 30.0f;
            }

            _devilTime = devilDealTime;

            upgrades.hellishContract = false;
        }

        public void DoFistStuff(bool wasMotherfucker)
        {
            // TODO: implement
            IsIncreasedMotherfuckerSpawn = !wasMotherfucker;
            IsDecreasedMotherfuckerSpawn = wasMotherfucker;

            if (IsIncreasedMotherfuckerSpawn)
            {
                _increasedMotherfuckerTimer = 30.0f;
            }

            if (IsDecreasedMotherfuckerSpawn)
            {
                _decreasedMotherfuckerTimer = 30.0f;
            }

            upgrades.powerFistRequisition = false;
        }

        public void SpawnTec()
        {
            for (var i = 0; i < numHuells; i++)
                SpawnTecHuell();

            upgrades.temporaryEmploymentContract = false;
        }

        public void GetExtraLife()
        {
            // TODO: implement

            _endOfLifePlan = true;

            upgrades.endOfLifePlan = false;
        }

        private void SpawnTecHuell()
        {
            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            var instance = Instantiate(huell, spawnPoint.transform.position, Quaternion.identity);
            instance.SetActive(true);
            var huellScript = instance.GetComponent<HuellController>();

            huellScript.TecMode = true;
        }

        public void PlayDeathScene()
        {
            Debug.Log("DEAD");
            // TODO: PLAY DEATH SCENE
        }

        public void ResetScene()
        {
            SceneManager.LoadScene("Office", LoadSceneMode.Single);
        }

        private void OnGUI()
        {
            if (upgrades.loanAgreement)
            {
                uiPowerDocumentLoan.SetActive(true);
            }
            else
            {
                uiPowerDocumentLoan.SetActive(false);
            }

            if (upgrades.temporaryEmploymentContract)
            {
                uiPowerDocumentTEC.SetActive(true);
            }
            else
            {
                uiPowerDocumentTEC.SetActive(false);
            }

            if (upgrades.endOfLifePlan)
            {
                uiPowerDocumentEOL.SetActive(true);
            }
            else
            {
                uiPowerDocumentEOL.SetActive(false);
            }

            if (upgrades.dismissal)
            {
                uiPowerDocumentDismissal.SetActive(true);
            }
            else
            {
                uiPowerDocumentDismissal.SetActive(false);
            }

            if (upgrades.hellishContract)
            {
                uiPowerDocumentHell.SetActive(true);
            }
            else
            {
                uiPowerDocumentHell.SetActive(false);
            }

            if (upgrades.powerFistRequisition)
            {
                uiPowerDocumentFist.SetActive(true);
            }
            else
            {
                uiPowerDocumentFist.SetActive(false);
            }

            var text = string.Empty;
            text += "Active Power: \n";
            // text += $"La: {upgrades.loanAgreement}\n";
            // text += $"Tec: {upgrades.temporaryEmploymentContract}\n";
            // text += $"Eel: {upgrades.endOfLifePlan}\n";
            // text += $"Ds: {upgrades.dismissal}\n";
            // text += $"Penta: {upgrades.hellishContract}\n";
            // text += $"Fist: {upgrades.powerFistRequisition}\n";
            text += $"Money: {upgrades.money}\n";
            text += $"Safe: {_moneyInSafe}\n";
            text += $"Active Customers Ids: {(string.Join(',', _customerMotors.Select(m => m.Id).ToList()))}";

            GUI.Label(new Rect(5, Screen.height / 2f, 200, 500), text);
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

        public List<CustomerMotor> GetCustomerList()
        {
            return _customerMotors;
        }
    }
}
