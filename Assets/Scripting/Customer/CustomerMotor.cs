using System;
using System.Collections;
using Scripting.Player;
using Scripting.Desk;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace Scripting.Customer
{
    public class CustomerMotor : MonoBehaviour
    {
        [SerializeField]
        int day1BadClientSpawnOdds = 0,
            day2BadClientSpawnOdds = 10,
            day3BadClientSpawnOdds = 9,
            day4BadClientSpawnOdds = 8,
            day5BadClientSpawnOdds = 7,
            day6BadClientSpawnOdds = 6,
            day7BadClientSpawnOdds = 5,
            day8BadClientSpawnOdds = 4,
            day9BadClientSpawnOdds = 4,
            day10BadClientSpawnOdds = 4;

        [SerializeField]
        int day1NPCReward = 8333,
            day2NPCReward = 11166,
            day3NPCReward = 11166,
            day4NPCReward = 12014,
            day5NPCReward = 14014,
            day6NPCReward = 15485,
            day7NPCReward = 14300,
            day8NPCReward = 14266,
            day9NPCReward = 14500,
            day10NPCReward = 18200;

        [SerializeField]
        AudioSource talkNoise;

        [SerializeField] private AudioSource myJingle;

        [SerializeField]
        public GameObject burlapSack;

        [SerializeField]
        float assistantConvoTime = 2f;

        [SerializeField]
        public GameObject documentAttachPoint;

        Color newColor;

        [SerializeField] private Rigidbody rb;
        [SerializeField] private NavMeshAgent agent;

        [SerializeField] private float secondsUntilFreakOut = 60.0f;
        [SerializeField] private float secondsUntilChangeActivity = 3.0f;

        [SerializeField] private CapsuleCollider capsuleCollider;

        [SerializeField]
        //how likely are you to play the conversation variant animation
        private int converseVariantProbability = 5;

        [SerializeField] private Billboard bubble;
        private SpriteRenderer bubSprite;

        public bool IsMotherfucker => _isMotherfucker;
        public bool IsThief => _isThief;

        private AiController _aiController;
        private float _changeTaskCooldown = 3.0f;
        private bool _done;


        private float _initialAgentSpeed;

        public void SetIsMotherfucker(bool value)
        {
            _isMotherfucker = value;
        }

        public float StressMeter { get; private set; }
        public int Id { get; set; }

        public static int NextId;

        private AiSpot _currentSpot;
        public Animator anim;
        private bool _conversing;
        private Movement _player;
        private bool _isMotherfucker;
        private bool _isThief;
        private VandalismSpot _vandalismSpot;
        private GameObject _thiefSpot;

        private List<string> _standardDocuments = new();
        private string _contractType;

        private int _paymentAmount;
        private int _penalty;

        public bool _runOut;
        public bool _sneakOut;

        private int _index;
        private bool convoWithAgent;

        private bool queuedSit;

        [SerializeField]
        int vandalSpawnOdds = 6;

        [SerializeField]
        int thiefSpawnOdds = 2;

        private CapsuleCollider _capsuleCollider;

        [SerializeField]
        public GameObject TutorialBubble;

        private LaunchDetection launch;

        public static void ResetId()
        {
            NextId = 0;
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            launch = GetComponent<LaunchDetection>();
            if (Id == 0)
            {
                TutorialManager.Singleton.ShowOrderNumber(12);
            }
            else
            {
                TutorialBubble.SetActive(false);
            }
        }

        private void Awake()
        {
            bubSprite = bubble.GetComponent<SpriteRenderer>();
            _capsuleCollider = GetComponents<CapsuleCollider>().FirstOrDefault(n => !n.isTrigger);
            _aiController = AiController.Singleton;
            _initialAgentSpeed = agent.speed;

            Id = NextId++;

            // DUDE i don't want to set it on every NPC so i set it here as hardcode bruv
            //secondsUntilFreakOut = 60;
            //secondsUntilChangeActivity = 10;
            //needed to find direction to face
            _player = FindFirstObjectByType<Movement>();
            //needed to affect animations
            anim = GetComponent<Animator>();
            _paymentAmount = 20000 + Random.Range(-5000, 5000);
            _penalty = 7000 * Random.Range(-500, 500);

            if (0 != Id)
            {
                _isMotherfucker = _aiController.HasVandalismSpots && GameManager.Singleton.day switch
                {
                    1 => Random.Range(0, day1BadClientSpawnOdds) == 1,
                    2 => Random.Range(0, day2BadClientSpawnOdds) == 1,
                    3 => Random.Range(0, day3BadClientSpawnOdds) == 1,
                    4 => Random.Range(0, day4BadClientSpawnOdds) == 1,
                    5 => Random.Range(0, day5BadClientSpawnOdds) == 1,
                    6 => Random.Range(0, day6BadClientSpawnOdds) == 1,
                    7 => Random.Range(0, day7BadClientSpawnOdds) == 1,
                    8 => Random.Range(0, day8BadClientSpawnOdds) == 1,
                    9 => Random.Range(0, day9BadClientSpawnOdds) == 1,
                    10 => Random.Range(0, day10BadClientSpawnOdds) == 1,
                    _ => false
                };
            }

            if (!_isMotherfucker && 0 != Id)
            {
                if (GameManager.Singleton.day == 1)
                {
                    _isThief = _aiController.HasVandalismSpots & (Random.Range(0, day1BadClientSpawnOdds) == 1);
                }
                else if (GameManager.Singleton.day == 2)
                {
                    _isThief = _aiController.HasVandalismSpots & (Random.Range(0, day2BadClientSpawnOdds) == 1);
                }
                else if (GameManager.Singleton.day == 3)
                {
                    _isThief = _aiController.HasVandalismSpots & (Random.Range(0, day3BadClientSpawnOdds) == 1);
                }
                else if (GameManager.Singleton.day == 4)
                {
                    _isThief = _aiController.HasVandalismSpots & (Random.Range(0, day4BadClientSpawnOdds) == 1);
                }
                else if (GameManager.Singleton.day == 5)
                {
                    _isThief = _aiController.HasVandalismSpots & (Random.Range(0, day5BadClientSpawnOdds) == 1);
                }
                else if (GameManager.Singleton.day == 6)
                {
                    _isThief = _aiController.HasVandalismSpots & (Random.Range(0, day6BadClientSpawnOdds) == 1);
                }
                else if (GameManager.Singleton.day == 7)
                {
                    _isThief = _aiController.HasVandalismSpots & (Random.Range(0, day7BadClientSpawnOdds) == 1);
                }
                else if (GameManager.Singleton.day == 8)
                {
                    _isThief = _aiController.HasVandalismSpots & (Random.Range(0, day8BadClientSpawnOdds) == 1);
                }
                else if (GameManager.Singleton.day == 9)
                {
                    _isThief = _aiController.HasVandalismSpots & (Random.Range(0, day9BadClientSpawnOdds) == 1);
                }
                else if (GameManager.Singleton.day == 10)
                {
                    _isThief = _aiController.HasVandalismSpots & (Random.Range(0, day10BadClientSpawnOdds) == 1);
                }
                else
                {
                    _isThief = false;
                    //error, day 11 
                }
            }

// #if UNITY_EDITOR
//             _isMotherfucker = true;
// #endif

            _aiController = FindFirstObjectByType<AiController>();
            _changeTaskCooldown = 0.1f;

            _standardDocuments = TrainingsData.StandardContractTypes;
            _index = Random.Range(0, _standardDocuments.Count);

            _contractType = _standardDocuments[_index];
        }


        //start conversation
        public void StartConversing()
        {
            agent.ResetPath();
            //talkNoise.Play();
            Unsit();
            _conversing = true;
            //Debug.Log("Starting Conversation");
            //anim.Play("Conversing");
        }

        //end conversatioon
        public void StopConversing()
        {
            //talkNoise.Stop();
            _conversing = false;
            if (anim != null)
            {
                anim.SetBool("conversing", false);
            }
        }

        private static bool _tutorialDone;
        private static readonly int Sitting = Animator.StringToHash("Sitting");

        public void ShowBubble()
        {
            if (!_tutorialDone)
            {
                _tutorialDone = true;
                TutorialManager.Singleton.ShowOrderNumber(1, true);
            }

            bubble.SetSprite(_index);
            _isBubbleVisible = true;
        }

        private bool _isBubbleVisible = false;

        //called in animation
        public void DecideToPlayVariant()
        {
            int rand = Random.Range(0, converseVariantProbability);
            if (rand - 1 >= 0)
            {
                if (rand == converseVariantProbability - 1)
                {
                    int rand2 = Random.Range(0, 3);
                    if (rand2 == 0)
                    {
                        //Debug.Log("PLAYING VARIANT 1");
                        anim.Play("Conversing Variant 1");
                    }
                    else if (rand2 == 1)
                    {
                        //Debug.Log("PLAYING VARIANT 2");
                        anim.Play("Conversing Variant 2");
                    }
                    else if (rand2 == 2)
                    {
                        //Debug.Log("PLAYING VARIANT 3");
                        anim.Play("Conversing Variant 3");
                    }
                    else
                    {
                        //Debug.Log("INVALID!!!" + rand2);
                    }
                }
            }
        }

        private void HandleConversing()
        {
            if (_conversing)
            {
                if (!convoWithAgent)
                {
                    var rotationVector = _player.Cam.transform.rotation.eulerAngles;

                    var newRotation = Quaternion.Euler(0, 180f + rotationVector.y, 0);

                    transform.rotation = newRotation;

                    // transform.rotation = Quaternion.LookRotation(-newForwardVector, transform.up);
                }

                if (!talkNoise.isPlaying)
                {
                    talkNoise.Play();
                }

                anim.SetBool("conversing", true);
            }
            else
            {
                anim.SetBool("conversing", false);
                if (talkNoise.isPlaying)
                {
                    talkNoise.Stop();
                }
            }
        }

        public bool IsGoodGuy => !_isMotherfucker && !_isThief;
        public bool WalksOut => _done || _sneakOut || _runOut;
        private bool _lockedAssistant;

        private void Update()
        {
            if (!agent.hasPath)
            {
                if (_runOut)
                {
                    agent.SetDestination(_aiController.GetRandomDespawnPoint().transform.position);
                }
                if (GameManager.Singleton.IsNightTime)
                {
                    Destroy(gameObject);
                }
            }

            if (!_aiController || WalksOut)
            {
                return;
            }

            if (_isSpraying && _vandalismSpot)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, _vandalismSpot.transform.rotation,
                    Time.deltaTime * 2f);
            }

            if (_isStealing)
            {
                transform.rotation =
                    Quaternion.Lerp(transform.rotation, _thiefSpot.transform.rotation, Time.deltaTime * 2f);
            }

            if (IsGoodGuy)
            {
                if (agent.enabled && agent.remainingDistance < .1f && queuedSit && !anim.GetBool(Sitting))
                {
                    StartCoroutine(Sitdown());
                }

                StressMeter += Time.deltaTime / (secondsUntilFreakOut * (_currentSpot ? 2f : 1f));
                if (_isBubbleVisible)
                {
                    newColor = bubSprite.color;
                    newColor.a = Mathf.Lerp(1, .2f, StressMeter);
                    bubSprite.color = newColor;
                }

                //handles rotation
                HandleConversing();

                if (StressMeter >= 1.0f)
                {
                    WalkOut();
                    return;
                }

                _changeTaskCooldown -= Time.deltaTime;

                if (_changeTaskCooldown <= 0)
                {
                    _changeTaskCooldown = secondsUntilChangeActivity + Random.Range(-3f, 3f);

                    if (_aiController.AssistantActive && !_aiController.AssistantLocked && !_isBubbleVisible &&
                        !_lockedAssistant)
                    {
                        _aiController.AssistantLocked = true;
                        _lockedAssistant = true;

                        if (queuedSit)
                        {
                            StartCoroutine(StandupAndThenGoToAssistant());
                        }
                        else
                        {
                            GoToAssistant();
                        }

                        return;
                    }

                    if (!_lockedAssistant)
                    {
                        var nextSpot = _aiController.GetFreeSpot();

                        if (_currentSpot)
                        {
                            _currentSpot?.Unlock();
                            Unsit();
                        }

                        if (nextSpot)
                        {
                            nextSpot.Lock();
                            _currentSpot = nextSpot;
                            if (_currentSpot.isChair)
                            {
                                queuedSit = true;
                            }

                            agent.SetDestination(nextSpot.transform.position);
                        }
                        else
                        {
                            agent.SetDestination(_aiController.GetRandomWaitingSpot());
                        }
                    }
                }
            }
            else if (_isMotherfucker)
            {
                if (!_vandalismSpot)
                {
                    _vandalismSpot = _aiController.GetRandomVandalismSpot();

                    if (_vandalismSpot)
                    {
                        _vandalismSpot.IsLocked = true;
                        StartCoroutine(GoToTargetWithCallback(_vandalismSpot.AiSpot.transform.position,
                            () =>
                            {
                                anim.Play("SprayPaint");
                                _isSpraying = true;
                            },
                            () =>
                            {
                                RunOut();
                                _isSpraying = false;
                            }, 20f));
                    }
                    else
                    {
                        // No spot left because of bug or something,
                        // i don't know, just ignore and go normal
                        _isMotherfucker = false;
                    }
                }
            }
            else if (_isThief)
            {
                if (!_thiefSpot)
                {
                    _thiefSpot = _aiController.GetThiefSpot();
                    if (_thiefSpot)
                    {
                        _aiController.SetThiefLocked(true);
                        StartCoroutine(GoToTargetWithCallback(_thiefSpot.transform.position,
                            () =>
                            {
                                anim.Play("Stealing");
                                _isStealing = true;
                            },
                            () =>
                            {
                                if (GameManager.Singleton.day == 1)
                                {
                                    _stolenMoney = Math.Min(GameManager.Singleton.GetMoney(),
                                        day1NPCReward + Random.Range(-1000, 1000));
                                }
                                else if (GameManager.Singleton.day == 2)
                                {
                                    _stolenMoney = Math.Min(GameManager.Singleton.GetMoney(),
                                        day2NPCReward + Random.Range(-1000, 1000));
                                }
                                else if (GameManager.Singleton.day == 3)
                                {
                                    _stolenMoney = Math.Min(GameManager.Singleton.GetMoney(),
                                        day3NPCReward + Random.Range(-1000, 1000));
                                }
                                else if (GameManager.Singleton.day == 4)
                                {
                                    _stolenMoney = Math.Min(GameManager.Singleton.GetMoney(),
                                        day4NPCReward + Random.Range(-1000, 1000));
                                }
                                else if (GameManager.Singleton.day == 5)
                                {
                                    _stolenMoney = Math.Min(GameManager.Singleton.GetMoney(),
                                        day5NPCReward + Random.Range(-1000, 1000));
                                }
                                else if (GameManager.Singleton.day == 6)
                                {
                                    _stolenMoney = Math.Min(GameManager.Singleton.GetMoney(),
                                        day6NPCReward + Random.Range(-1000, 1000));
                                }
                                else if (GameManager.Singleton.day == 7)
                                {
                                    _stolenMoney = Math.Min(GameManager.Singleton.GetMoney(),
                                        day7NPCReward + Random.Range(-1000, 1000));
                                }
                                else if (GameManager.Singleton.day == 8)
                                {
                                    _stolenMoney = Math.Min(GameManager.Singleton.GetMoney(),
                                        day8NPCReward + Random.Range(-1000, 1000));
                                }
                                else if (GameManager.Singleton.day == 9)
                                {
                                    _stolenMoney = Math.Min(GameManager.Singleton.GetMoney(),
                                        day9NPCReward + Random.Range(-1000, 1000));
                                }
                                else if (GameManager.Singleton.day == 10)
                                {
                                    _stolenMoney = Math.Min(GameManager.Singleton.GetMoney(),
                                        day10NPCReward + Random.Range(-1000, 1000));
                                }
                                else
                                {
                                    _stolenMoney = 0;
                                    //error, day 11 
                                }

                                GameManager.Singleton.MoneyStolen(_stolenMoney);
                                SneakOut();
                                _isStealing = false;
                                anim.SetBool("isSneaking", true);
                                burlapSack.SetActive(true);
                            }, 3f)); // TODO: SET TIEM
                    }
                    else
                    {
                        // No spot left because of bug or something,
                        // i don't know, just ignore and go normal
                        _isThief = false;
                    }
                }
            }
        }

        private void GoToAssistant()
        {
            StartCoroutine(GoToTargetWithCallback(_aiController.AssistantSpot.transform.position, () =>
                {
                    // TODO: Play Talk Animation with Assistant
                    StartConversing();
                    convoWithAgent = true;
                    DecideToPlayVariant();
                },
                () =>
                {
                    if (_lockedAssistant)
                    {
                        StopConversing();
                        // TODO: Stop Talk Animation with Assistant
                        Assistant.Singleton.PopBubbleGum();
                        ShowBubble();
                        _changeTaskCooldown = 0.0f;
                        convoWithAgent = false;
                    }
                }, assistantConvoTime, UnlockAssistant));
        }

        private IEnumerator StandupAndThenGoToAssistant()
        {
            Unsit();

            yield return new WaitForSeconds(0.5f);

            GoToAssistant();
        }

        private void Unsit()
        {
            if (queuedSit)
            {
                anim.SetBool("Sitting", false);
                rb.isKinematic = false;
                agent.enabled = true;
                _capsuleCollider.enabled = true;
            }

            _currentSpot?.Unlock();
            _currentSpot = null;

            queuedSit = false;
        }

        private IEnumerator Sitdown()
        {
            var elapsed = 0.0f;

            agent.enabled = false;
            _capsuleCollider.enabled = false;

            var position = _currentSpot.transform.position + new Vector3(-0.5f, 0f, 0f);
            var rotation = Quaternion.Euler(0, 90, 0);
            rb.isKinematic = true;

            anim.Play("Sit In Chair");
            anim.SetBool("Sitting", true);

            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;

                transform.rotation = Quaternion.Lerp(transform.rotation, rotation,
                    elapsed * (1f / 0.3f));
                transform.position = Vector3.Lerp(transform.position, position,
                    elapsed * (1f / 0.3f));

                yield return null;
            }
            //and its a chair???
        }

        private bool _isStealing;
        private bool _isSpraying;

        public void FinishPaint()
        {
            _isSpraying = false;
            _vandalismSpot.Spray();
        }

        private bool CancellationToken => _done || _runOut || _sprayInterrupted || _sneakOut || _stealInterrupted ||
                                          !agent.isActiveAndEnabled;

        private IEnumerator GoToTargetWithCallback(Vector3 position, Action start = null, Action callback = null,
            float delay = 0, Action onFinally = null)
        {
            agent.SetDestination(position);

            yield return null;

            while (!CancellationToken && agent.remainingDistance > 0.1f)
            {
                yield return null;
            }

            if (!CancellationToken)
                start?.Invoke();

            if (!CancellationToken)
                yield return new WaitForSeconds(delay);

            if (!CancellationToken)
                callback?.Invoke();

            onFinally?.Invoke();
        }

        private bool _sprayInterrupted;
        private bool _stealInterrupted;

        public void InterruptSpraying()
        {
            _sprayInterrupted = true;
            var paint = GetComponent<SprayPaintSounds>();
            paint.StopSpraySound();
            paint.HidePaintCan();
            paint.StopRattleSound();
            _isSpraying = false;
            if (_vandalismSpot && !_vandalismSpot.IsVisible)
                _vandalismSpot.IsLocked = false;
        }

        private int _stolenMoney;

        public void InterruptStealing()
        {
            _stealInterrupted = true;
            _isStealing = false;
            burlapSack.SetActive(false);
            _aiController.SetThiefLocked(false);
            anim.SetBool("isSneaking", false);
        }

        private void OnValidate()
        {
            if (rb)
                rb = GetComponent<Rigidbody>();
        }

        public void GetHit()
        {
            Unsit();
            if (agent.isOnNavMesh)
                agent.ResetPath();
        }


        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Weapon"))
            {
                rb.isKinematic = false;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_isThief)
            {
                Handles.Label(transform.position + Vector3.up * 2.5f,
                    "Id: " + Id + ", Is Motherfucker: " + (_isMotherfucker ? "Yes" : "No"));
                Handles.Label(transform.position + Vector3.up * 2f,
                    "Wants: " + (_isMotherfucker ? "trouble" : _contractType));
                Handles.Label(transform.position + Vector3.up * 1.5f,
                    "Stresslevel: " + (_isMotherfucker ? "unlimited" : StressMeter));

                Handles.Label(transform.position + Vector3.up * 3f,
                    "Target: " + (agent.isActiveAndEnabled ? agent.destination : "agent inanctive") + "\n" +
                    $"Done: {_done}, RunOut: {_runOut}, SInt: {_sprayInterrupted}\nSOut: {_sneakOut}, STInter: {_stealInterrupted}\nAssLock: {_lockedAssistant} ");
            }
            else
            {
                Handles.Label(transform.position + Vector3.up * 2.5f,
                    "Id: " + Id + ", Is Thief: Yes");
                Handles.Label(transform.position + Vector3.up * 2f,
                    "Wants: money");
                Handles.Label(transform.position + Vector3.up * 1.5f,
                    "Stresslevel: chilled");
            }
        }
#endif

        public bool Validate(Contract contract)
        {
            GameManager.Singleton.upgrades.tutorialDone = true;
            Unsit();
            if (contract.Result == _contractType || contract.GetIsPowerContract())
            {
                myJingle.Play();
                bubble.gameObject.SetActive(false);
                UnlockAssistant();
                return true;
            }

            return false;
        }

        public void WalkOut()
        {
            _sneakOut = false;
            _runOut = false;
            _done = true;
            StopConversing();
            if (!agent.enabled)
            {
                agent.enabled = true;
            }

            if (agent.isOnNavMesh)
            {
                agent.speed = _initialAgentSpeed;
                agent.SetDestination(_aiController.GetRandomDespawnPoint().transform.position);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void UnlockAssistant()
        {
            if (_lockedAssistant)
            {
                _lockedAssistant = false;
                _aiController.AssistantLocked = false;
            }

            convoWithAgent = false;
        }

        public void RunOut()
        {
            UnlockAssistant();
            StopConversing();
            if (!agent.enabled)
            {
                agent.enabled = true;
            }
            if (_runOut)
            {
                return;
            }
            
            _runOut = true;
            _sneakOut = false;
            anim.SetBool("isRunning", true);
            anim.Play("RUN");
            //GameManager.Singleton.RemoveCustomer(this);
            agent.speed = _initialAgentSpeed * 2f;
            agent.SetDestination(_aiController.GetRandomDespawnPoint().transform.position);
        }

        public void IsHit()
        {
            if (_stolenMoney > 0)
            {
                anim.SetBool("isSneaking", false);
                GameManager.Singleton.ReturnStolenMoney(_stolenMoney);
                _stolenMoney = 0;
            }
        }

        public void SneakOut()
        {
            if (!agent.enabled)
            {
                agent.enabled = true;
            }

            if (_sneakOut)
            {
                return;
            }

            _sneakOut = true;
            anim.SetBool("isSneaking", true);
            anim.Play("SNEAK");
            //GameManager.Singleton.RemoveCustomer(this);

            agent.speed = _initialAgentSpeed * 0.25f;
            agent.SetDestination(_aiController.GetRandomDespawnPoint().transform.position);
        }

        public bool IsSpraying => _isSpraying;
        public bool IsStealing => _isStealing;
        public bool IsSneakingOut => _sneakOut;

        private int CalculateReward(int dayNpcReward, int variance, float multiplicator, bool isDismissal)
        {
            const float dismissalMultiplicator = 0.5f;

            // Had to do this shit, cuz int can't handle floating points
            float converted = dayNpcReward + variance;

            var reward = converted * multiplicator * (isDismissal ? dismissalMultiplicator : 1.0f);

            return (int) Math.Ceiling(reward);
        }

        public void Pay(bool isDismissal = false)
        {
            var laMultiplicator = GameManager.Singleton.LoanAgreementMultiplicator;
            
            var value = GameManager.Singleton.day switch
            {
                1 => CalculateReward(day1NPCReward, Random.Range(-1000, 1000),
                    GameManager.Singleton.IsLoanAgreementRunning ? laMultiplicator : 1f, isDismissal),
                2 => CalculateReward(day2NPCReward, Random.Range(-1000, 1000),
                    GameManager.Singleton.IsLoanAgreementRunning ? laMultiplicator : 1f, isDismissal),
                3 => CalculateReward(day3NPCReward, Random.Range(-1000, 1000),
                    GameManager.Singleton.IsLoanAgreementRunning ? laMultiplicator : 1f, isDismissal),
                4 => CalculateReward(day4NPCReward, Random.Range(-1000, 1000),
                    GameManager.Singleton.IsLoanAgreementRunning ? laMultiplicator : 1f, isDismissal),
                5 => CalculateReward(day5NPCReward, Random.Range(-1000, 1000),
                    GameManager.Singleton.IsLoanAgreementRunning ? laMultiplicator : 1f, isDismissal),
                6 => CalculateReward(day6NPCReward, Random.Range(-1000, 1000),
                    GameManager.Singleton.IsLoanAgreementRunning ? laMultiplicator : 1f, isDismissal),
                7 => CalculateReward(day7NPCReward, Random.Range(-1000, 1000),
                    GameManager.Singleton.IsLoanAgreementRunning ? laMultiplicator : 1f, isDismissal),
                8 => CalculateReward(day8NPCReward, Random.Range(-1000, 1000),
                    GameManager.Singleton.IsLoanAgreementRunning ? laMultiplicator : 1f, isDismissal),
                9 => CalculateReward(day9NPCReward, Random.Range(-1000, 1000),
                    GameManager.Singleton.IsLoanAgreementRunning ? laMultiplicator : 1f, isDismissal),
                10 => CalculateReward(day10NPCReward, Random.Range(-1000, 1000),
                    GameManager.Singleton.IsLoanAgreementRunning ? laMultiplicator : 1f, isDismissal),
                _ => 0
            };

            GameManager.Singleton.ChangeMoneyAmount(value);
        }

        public HuellController TargetedFrom { get; set; }

        public bool IsHuellImmune { get; set; }
        public bool IsWalkingOut => _done;

        public void StopHuell()
        {
            TargetedFrom?.StopCurrentTarget();
        }

        public void RemoveHuellReferences()
        {
            IsHuellImmune = true;
            TargetedFrom = null;
        }
    }
}
