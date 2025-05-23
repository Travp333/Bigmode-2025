﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripting.Customer;
using Scripting.Desk;
using Scripting.Player;
using Scripting.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Scripting
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        public List<ShowOnlyOnce> powerTutorials;

        [SerializeField]
        MusicSwitcher mus;

        [SerializeField]
        GameObject successScreen;

        [SerializeField]
        GameObject safeMoneyAmountUI;

        [SerializeField]
        GameObject totalMoneyAmountUI;

        [SerializeField]
        int day1Quota = 15000,
            day2Quota = 30000,
            day3Quota = 40000,
            day4Quota = 50000,
            day5Quota = 70000,
            day6Quota = 90000,
            day7Quota = 100000,
            day8Quota = 120000,
            day9Quota = 140000,
            day10Quota = 200000;

        [SerializeField]
        float day1AISpawnRate = 13.33f,
            day2AISpawnRate = 13.33f,
            day3AISpawnRate = 13.33f,
            day4AISpawnRate = 11.42f,
            day5AISpawnRate = 11.42f,
            day6AISpawnRate = 11.42f,
            day7AISpawnRate = 8,
            day8AISpawnRate = 5.333f,
            day9AISpawnRate = 4,
            day10AISpawnRate = 2.66f;

        [SerializeField]
        int day1PentagramReward = 7500,
            day2PentagramReward = 15000,
            day3PentagramReward = 20000,
            day4PentagramReward = 25000,
            day5PentagramReward = 35000,
            day6PentagramReward = 45000,
            day7PentagramReward = 50000,
            day8PentagramReward = 60000,
            day9PentagramReward = 70000,
            day10PentagramReward = 100000;

        public int day;

        [SerializeField]
        TextMeshProUGUI dayTrackerUI;

        [SerializeField]
        GameObject EOLScreen;

        [SerializeField]
        GameObject wastedScreen;

        [SerializeField]
        Animator doorAnim;

        [SerializeField] private float loanAgreementMultiplicator = 2.0f;

        [SerializeField]
        private GameObject QuotaMetUI, MoneyDifferenceUI, MoneyDifferenceUIPOS;

        [SerializeField]
        private TextMeshProUGUI MoneyUI, QuotaUI;

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

        private float _increasedMotherfuckerTimer;
        private float _decreasedMotherfuckerTimer;
        private float _devilTime;
        private int _finalMoneyTally;

        public GameObject MainCanvas => mainCanvas;
        public Movement Player => player;
        private readonly List<CustomerMotor> _customerMotors = new();

        private int _maxCustomers;
        private int _level = 1;
        private float _loanAgreementRunning;
        private int _moneyInSafe;
        private float _aiSpawnRateCounter = 10f;
        private float _aiSpawnRate;
        private float _dayTimer;

        public bool _endOfLifePlan;

        public float PercentTimeLeft => _dayTimer / dayLength;

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
            upgrades.tutorialDone = false;
            upgrades.powerTutorialDone = false;
            upgrades.money = 0;
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
            CustomerMotor.ResetId();
            Singleton = this;
            IsNightTime = true;

            SetInitValues();
            Contract.ResetTutorial();

            fade.gameObject.SetActive(true);
            StartCoroutine(FadeOut());
        }


        public int GetMoney() => upgrades.money;

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

        public bool IsNightTime { get; private set; }
        public float LoanAgreementMultiplicator => loanAgreementMultiplicator;

        public void StartDay()
        {
            day++;
            QuotaUI.text = "Today's Quota: $" + GetCurrentQuota();
            if (day == 1)
            {
                _aiSpawnRateCounter = day1AISpawnRate;
            }
            else if (day == 2)
            {
                _aiSpawnRateCounter = day2AISpawnRate;
            }
            else if (day == 3)
            {
                _aiSpawnRateCounter = day3AISpawnRate;
            }
            else if (day == 4)
            {
                _aiSpawnRateCounter = day4AISpawnRate;
            }
            else if (day == 5)
            {
                _aiSpawnRateCounter = day5AISpawnRate;
            }
            else if (day == 6)
            {
                _aiSpawnRateCounter = day6AISpawnRate;
            }
            else if (day == 7)
            {
                _aiSpawnRateCounter = day7AISpawnRate;
            }
            else if (day == 8)
            {
                _aiSpawnRateCounter = day8AISpawnRate;
            }
            else if (day == 9)
            {
                _aiSpawnRateCounter = day9AISpawnRate;
            }
            else if (day == 10)
            {
                _aiSpawnRateCounter = day10AISpawnRate;
            }
            else
            {
                //ERROR, DAY 11, 
            }
            //aiSpawnRate = aiSpawnRateCounter;


            dayTrackerUI.text = "DAY " + day + "/ 10";
            doorAnim.SetBool("opened", false);
            IsNightTime = false;
            //Debug.Log("Switching to Night mode!");
            shiftManager.SetIsNightTime(false);
            _dayTimer = dayLength;
            _maxCustomers = 30;
            //_maxCustomers = _level * 5 + _level;
        }

        private void Update()
        {
            //Debug.Log(IsNightTime);
            if (IsNightTime)
            {
                doorAnim.SetBool("opened", true);
                return;
            }

            _dayTimer -= Time.deltaTime;
            _loanAgreementRunning -= Time.deltaTime;
            _increasedMotherfuckerTimer -= Time.deltaTime;
            _decreasedMotherfuckerTimer -= Time.deltaTime;
            _devilTime -= Time.deltaTime;
            _aiSpawnRate -= Time.deltaTime;

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

            if (_aiSpawnRate <= 0)
            {
                _aiSpawnRate = _aiSpawnRateCounter;
                SpawnCustomer();
            }
        }

        private void ResetQuotaMetUI()
        {
            QuotaMetUI.SetActive(false);
        }

        public void MoneyStolen(int value)
        {
            ChangeMoneyAmount(-value);
        }

        public void ReturnStolenMoney(int value)
        {
            ChangeMoneyAmount(value);
        }

        void hideSafeMoneyUI()
        {
            safeMoneyAmountUI.GetComponentsInChildren<TextMeshProUGUI>()[0].text = "$" + _moneyInSafe;
            Invoke(nameof(hideSafeMoneyUI2), 5);
        }

        void hideSafeMoneyUI2()
        {
            safeMoneyAmountUI.SetActive(false);
        }

        void FinalQuotaStep2()
        {
            //Debug.Log("FINAL DAY #2");
            _finalMoneyTally += _moneyInSafe;
            totalMoneyAmountUI.GetComponentsInChildren<TextMeshProUGUI>()[0].text = "$" + _finalMoneyTally;
            _moneyInSafe = 0;
            safeMoneyAmountUI.GetComponentsInChildren<TextMeshProUGUI>()[0].text = "";
            Invoke(nameof(FinalQuotaStep3), 3f);
        }

        void FinalQuotaStep3()
        {
            //Debug.Log("FINAL DAY #3");
            _finalMoneyTally = _finalMoneyTally + upgrades.money;
            totalMoneyAmountUI.GetComponentsInChildren<TextMeshProUGUI>()[0].text = "$" + _finalMoneyTally;
            ChangeMoneyAmount(-upgrades.money);
            Invoke(nameof(FinalQuotaStep4), 3f);
        }

        void FinalQuotaStep4()
        {
            //Debug.Log("FINAL DAY #4");
            if (_finalMoneyTally > 1000000)
            {
                totalMoneyAmountUI.SetActive(false);
                successScreen.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;
                player.enabled = false;
            }
            else
            {
                PlayDeathScene();
            }
        }

        public void DayFinished()
        {
            IsNightTime = true;
            Player.ResetStressLevel();
            AiController.Singleton.UnlockEverything();
            SpecialStoreManager.Singleton.SetRandomUpgrade();
            shiftManager.SetIsNightTime(true);

            if (_level < 3)
            {
                TutorialManager.Singleton.ShowOrderNumber(8);
            }

            if (nightResetsTimer)
            {
                _loanAgreementRunning = 0.0f;
                _increasedMotherfuckerTimer = 0.0f;
                _decreasedMotherfuckerTimer = 0.0f;
                _devilTime = 0.0f;
            }

            if (day == 10)
            {
                safeMoneyAmountUI.SetActive(true);
                totalMoneyAmountUI.SetActive(true);
                //FINAL DAY!!! CHECK FINAL QUOTA!!!
                //Debug.Log("FINAL DAY #1");
                Invoke(nameof(FinalQuotaStep2), 3f);
                return;
            }

            var todaysQuota = GetCurrentQuota();
            QuotaUI.text = "Today's Quota: $" + todaysQuota;

            if (upgrades.money > todaysQuota)
            {
                safeMoneyAmountUI.SetActive(true);
                QuotaMetUI.SetActive(true);
                Invoke(nameof(ResetQuotaMetUI), 3.5f);
                ChangeMoneyAmount(-todaysQuota);
                //upgrades.money -= todaysQuota;
                safeMoneyAmountUI.GetComponentsInChildren<TextMeshProUGUI>()[0].text = "$" + _moneyInSafe;
                _moneyInSafe += todaysQuota;
                Invoke(nameof(hideSafeMoneyUI), 3.5f);
            }
            else
            {
                if (_endOfLifePlan)
                {
                    EOLScreen.SetActive(true);
                    Invoke(nameof(ResetEOLScreen), 3.5f);
                    _endOfLifePlan = false;
                    _moneyInSafe += todaysQuota;
                    upgrades.money = 0;
                    //Debug.Log("END OF LIFE PLAN ACTIVATED");
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

        void ResetEOLScreen()
        {
            EOLScreen.SetActive(false);
        }

        public int GetCurrentQuota()
        {
            if (day == 1)
            {
                return day1Quota;
            }
            else if (day == 2)
            {
                return day2Quota;
            }
            else if (day == 3)
            {
                return day3Quota;
            }
            else if (day == 4)
            {
                return day4Quota;
            }
            else if (day == 5)
            {
                return day5Quota;
            }
            else if (day == 6)
            {
                return day6Quota;
            }
            else if (day == 7)
            {
                return day7Quota;
            }
            else if (day == 8)
            {
                return day8Quota;
            }
            else if (day == 9)
            {
                return day9Quota;
            }
            else if (day == 10)
            {
                return day10Quota;
            }
            else return 0;
            //return 40000 * (int) Math.Ceiling(Mathf.Pow(1.165f, _level - 1));
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

        public void ActivateLoanAgreement()
        {
            _loanAgreementRunning = loanAgreementTime;
            upgrades.loanAgreement = false;
        }

        public void Dismissal()
        {
            _customerMotors.ToList().ForEach(n =>
            {
                n.Pay(true);
                n.WalkOut();
                n.anim.SetBool("conversing", false);
                n.anim.SetBool("Sitting", false);
                n.anim.SetBool("isSneaking", false);
                n.anim.Play("WALK");
                n.IsHit();
                n.InterruptStealing();
                n.InterruptSpraying();
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
            mus.gameObject.SetActive(false);
            wastedScreen.SetActive(true);
            Debug.Log("DEAD");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            player.enabled = false;
            // TODO: PLAY DEATH SCENE
        }

        public void ResetScene()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("Office", LoadSceneMode.Single);
        }

        public void ReturnToMenuScene()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        private void OnGUI()
        {
            uiPowerDocumentLoan.SetActive(upgrades.loanAgreement);
            uiPowerDocumentTEC.SetActive(upgrades.temporaryEmploymentContract);
            uiPowerDocumentEOL.SetActive(upgrades.endOfLifePlan);
            uiPowerDocumentDismissal.SetActive(upgrades.dismissal);
            uiPowerDocumentHell.SetActive(upgrades.hellishContract);
            uiPowerDocumentFist.SetActive(upgrades.powerFistRequisition);

            MoneyUI.text = "$" + upgrades.money;
        }

        public void ChangeMoneyAmount(int amount)
        {
            upgrades.money += amount;
            if (amount < 0)
            {
                var moneyUI = Instantiate(MoneyDifferenceUI, MoneyDifferenceUIPOS.transform);
                //MoneyDifferenceUI.gameObject.SetActive(true);
                moneyUI.GetComponent<TextMeshProUGUI>().text = "- $" + amount * -1;
                moneyUI.GetComponent<TextMeshProUGUI>().color = Color.red;
            }
            else
            {
                var moneyUI = Instantiate(MoneyDifferenceUI, MoneyDifferenceUIPOS.transform);
                //MoneyDifferenceUI.gameObject.SetActive(true);
                moneyUI.GetComponent<TextMeshProUGUI>().text = "+ $" + amount;
                moneyUI.GetComponent<TextMeshProUGUI>().color = Color.green;
            }
        }

        public List<CustomerMotor> GetCustomerList() => _customerMotors;

        public bool PayAssistant()
        {
            if (upgrades.priceAssistant <= upgrades.money)
            {
                ChangeMoneyAmount(-upgrades.priceAssistant);
                return true;
            }

            return false;
        }

        public bool PayBodyguard()
        {
            if (upgrades.priceBodyguard <= upgrades.money)
            {
                ChangeMoneyAmount(-upgrades.priceBodyguard);
                return true;
            }

            return false;
        }

        public bool PayPhone()
        {
            if (upgrades.pricePhone <= upgrades.money)
            {
                ChangeMoneyAmount(-upgrades.pricePhone);
                return true;
            }

            return false;
        }

        public bool PayCigar()
        {
            if (upgrades.priceCigar <= upgrades.money)
            {
                ChangeMoneyAmount(-upgrades.priceCigar);
                return true;
            }

            return false;
        }

        public bool PayBaseballBat()
        {
            if (upgrades.priceBaseballBat <= upgrades.money)
            {
                ChangeMoneyAmount(-upgrades.priceBaseballBat);
                return true;
            }

            return false;
        }

        public bool PayPaintings()
        {
            if (upgrades.pricePaintings <= upgrades.money)
            {
                ChangeMoneyAmount(-upgrades.pricePaintings);
                return true;
            }

            return false;
        }

        public bool PayChairs()
        {
            if (upgrades.priceChairs <= upgrades.money)
            {
                ChangeMoneyAmount(-upgrades.priceChairs);
                return true;
            }

            return false;
        }

        public bool IsPause { get; private set; }

        public void SetIsPauseMenu(bool p0)
        {
            IsPause = p0;
        }

        public int GetPentagramReward()
        {
            float result = 0f;
            switch (day)
            {
                case 1:
                    if (IsLoanAgreementRunning)
                    {
                        result = day1PentagramReward * loanAgreementMultiplicator;
                    }
                    else
                    {
                        result = day1PentagramReward;
                    }

                    break;
                case 2:
                    if (IsLoanAgreementRunning)
                    {
                        result = day2PentagramReward * loanAgreementMultiplicator;
                    }
                    else
                    {
                        result = day2PentagramReward;
                    }

                    break;
                case 3:
                    if (IsLoanAgreementRunning)
                    {
                        result = day3PentagramReward * loanAgreementMultiplicator;
                    }
                    else
                    {
                        result = day3PentagramReward;
                    }

                    break;
                case 4:
                    if (IsLoanAgreementRunning)
                    {
                        result = day4PentagramReward * loanAgreementMultiplicator;
                    }
                    else
                    {
                        result = day4PentagramReward;
                    }

                    break;
                case 5:
                    if (IsLoanAgreementRunning)
                    {
                        result = day5PentagramReward * loanAgreementMultiplicator;
                    }
                    else
                    {
                        result = day5PentagramReward;
                    }

                    break;
                case 6:
                    if (IsLoanAgreementRunning)
                    {
                        result = day6PentagramReward * loanAgreementMultiplicator;
                    }
                    else
                    {
                        result = day6PentagramReward;
                    }

                    break;
                case 7:
                    if (IsLoanAgreementRunning)
                    {
                        result = day7PentagramReward * loanAgreementMultiplicator;
                    }
                    else
                    {
                        result = day7PentagramReward;
                    }

                    break;
                case 8:
                    if (IsLoanAgreementRunning)
                    {
                        result = day8PentagramReward * loanAgreementMultiplicator;
                    }
                    else
                    {
                        result = day8PentagramReward;
                    }

                    break;
                case 9:
                    if (IsLoanAgreementRunning)
                    {
                        result = day9PentagramReward * loanAgreementMultiplicator;
                    }
                    else
                    {
                        result = day9PentagramReward;
                    }

                    break;
                case 10:
                    if (IsLoanAgreementRunning)
                    {
                        result = day10PentagramReward * loanAgreementMultiplicator;
                    }
                    else
                    {
                        result = day10PentagramReward;
                    }

                    break;
            }

            return (int) result;
        }

        public void HidePowerDocumentTutorial()
        {
            powerTutorials.ForEach(n => n.Hide());
            upgrades.powerTutorialDone = true;
        }
    }
}
