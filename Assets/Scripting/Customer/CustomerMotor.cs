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
        AudioSource talkNoise;
        [SerializeField]
        GameObject burlapSack;

        [SerializeField]
        float assistantConvoTime = 5f;

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

        private readonly Vector3 _aiFixPosition = new(1.4f, 1.0f, -32f);

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

        public static int NextId = 0;

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

        public bool IsHuellTarget { get; set; }

        [SerializeField]
        int vandalSpawnOdds = 6;

        [SerializeField]
        int thiefSpawnOdds = 2;

        private CapsuleCollider _capsuleCollider;
        [SerializeField]
        public GameObject TutorialBubble;
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
            if(Id == 0){
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
            secondsUntilFreakOut = 60;
            secondsUntilChangeActivity = 10;
            //needed to find direction to face
            _player = FindFirstObjectByType<Movement>();
            //needed to affect animations
            anim = GetComponent<Animator>();
            _paymentAmount = 20000 + Random.Range(-5000, 5000);
            _penalty = 7000 * Random.Range(-500, 500);
            
            if(0 != Id){
                _isMotherfucker =  _aiController.HasVandalismSpots & (Random.Range(0, vandalSpawnOdds) == Random.Range(0, vandalSpawnOdds));
            }

            if (!_isMotherfucker && 0 != Id)
            {
                _isThief = _aiController.HasThiefSpot && (Random.Range(0, thiefSpawnOdds) == Random.Range(0, thiefSpawnOdds));
            }

            _aiController = FindFirstObjectByType<AiController>();
            _changeTaskCooldown = 0.1f;

            _standardDocuments = TrainingsData.StandardContractTypes;
            _index = Random.Range(0, _standardDocuments.Count);

            _contractType = _standardDocuments[_index];
        }


        //start conversation
        public void StartConversing()
        {
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
            agent.isStopped = false;
            
        }

        private static bool _tutorialDone;

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
                agent.isStopped = true;

                if (!convoWithAgent)
                {
                    transform.rotation = Quaternion.LookRotation(-_player.transform.forward, _player.transform.up);
                }
                if(!talkNoise.isPlaying){
                    talkNoise.Play();
                }
                
                anim.SetBool("conversing", true);
            }
            else
            {
                anim.SetBool("conversing", false);
                if(talkNoise.isPlaying){
                    talkNoise.Stop();
                }
            }
        }

        private bool _lockedAssistant;
        
        private void Update()
        {
            if (!agent.hasPath)
            {
                if (GameManager.Singleton.IsNightTime)
                {
                    if (_isThief)
                    {
                        HuellController.Singleton.Reset(this);
                    }

                    Destroy(gameObject);
                }
            }

            // if (agent.enabled && !agent.isOnNavMesh)
            // {
            //     transform.position = Vector3.Lerp(transform.position, _aiFixPosition, Time.deltaTime * 2.0f);
            // }

            if (!_aiController || _done || _runOut || _sneakOut)
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

            if (!_isMotherfucker && !_isThief)
            {
                if (agent.enabled && agent.remainingDistance < .1f && queuedSit && !anim.GetBool("Sitting"))
                {
                    StartCoroutine(Sitdown());
                }

                StressMeter += Time.deltaTime / (secondsUntilFreakOut * (_currentSpot ? 2f : 1f));
                if (_isBubbleVisible)
                {
                    newColor = bubSprite.color;
                    newColor.a = Mathf.Lerp(1, 0, StressMeter);
                    bubSprite.color = newColor;
                }

                //handles rotation
                HandleConversing();

                if (StressMeter >= 1.0f)
                {
                    _done = true;
                    // are they stealing money when they leave?
                    //RemoveMoney();

                    WalkOut();
                    return;
                }

                _changeTaskCooldown -= Time.deltaTime;

                if (_changeTaskCooldown <= 0)
                {
                    _changeTaskCooldown = secondsUntilChangeActivity + Random.Range(-3f, 3f);

                    if (_aiController.AssistantActive && !_aiController.AssistantLocked && !_isBubbleVisible)
                    {
                        _aiController.AssistantLocked = true;
                        _lockedAssistant = true;

                        StartCoroutine(GoToTargetWithCallback(_aiController.AssistantSpot.transform.position, () =>
                            {
                                // TODO: Play Talk Animation with Assistant
                                StartConversing();
                                convoWithAgent = true;
                                DecideToPlayVariant();
                            },
                            () =>
                            {
                                StopConversing();
                                convoWithAgent = false;
                                // TODO: Stop Talk Animation with Assistant
                                Assistant.Singleton.PayBubbleGum();
                                ShowBubble();
                            }, assistantConvoTime, () =>
                            {
                                _lockedAssistant = false; _aiController.AssistantLocked = false;} ));

                        return;
                    }

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
            else if (_isMotherfucker)
            {
                if (!_vandalismSpot)
                {
                    _vandalismSpot = _aiController.GetRandomVandalismSpot();

                    if (_vandalismSpot)
                    {
                        _vandalismSpot.IsLocked = true;
                        StartCoroutine(GoToTargetWithCallback(_vandalismSpot.transform.position,
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
                                _stolenMoney = Math.Min(GameManager.Singleton.GetMoney(), 20);
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
            // transform.position = position;
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

        private IEnumerator GoToTargetWithCallback(Vector3 position, Action start = null, Action callback = null,
            float delay = 0, Action onFinally = null)
        {
            if (agent.enabled)
            {
                agent.SetDestination(position);

                yield return null;

                while (agent.remainingDistance > 0.1f && !_done && !_runOut && !_sneakOut && !_sprayInterrupted &&
                       !_stealInterrupted)
                {
                    yield return null;
                }
            }

            if (!_done && !_runOut && !_sprayInterrupted && !_sneakOut && !_stealInterrupted)
                start?.Invoke();

            if (!_done && !_runOut && !_sprayInterrupted && !_sneakOut && !_stealInterrupted)
                yield return new WaitForSeconds(delay);

            if (!_done && !_runOut && !_sprayInterrupted && !_sneakOut && !_stealInterrupted)
                callback?.Invoke();

            onFinally?.Invoke();
        }

        private void WalkIn()
        {
            if (!_aiController) return;

            // _changeTaskCooldown = 0.1f;

            // var entrance = _aiController.EntrancePoint.transform.position;
            //
            // agent.SetDestination(entrance);
        }

        private bool _sprayInterrupted;
        private bool _stealInterrupted;

        public void InterruptSpraying()
        {
            _sprayInterrupted = true;
            var paint = this.GetComponent<SprayPaintSounds>();
            paint.StopSpraySound();
            paint.HidePaintCan();
            paint.StopRattleSound();
            _isSpraying = false;

            if (!_vandalismSpot.IsVisible)
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
            StartCoroutine(DoGetHit());
        }

        private IEnumerator DoGetHit()
        {
            //Debug.Log("Stopping Agent!~");
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            //agent.enabled = false;
            yield return new WaitForSeconds(.5f);
            //agent.enabled = true;
            agent.isStopped = false;
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

        private void RemoveMoney()
        {
            //GameManager.Singleton.upgrades.money -= _penalty;
            GameManager.Singleton.ChangeMoneyAmount(-_penalty);
            GameManager.Singleton.OnMoneyUpdated?.Invoke(GameManager.Singleton.upgrades.money, -_penalty);
            // HAS TO BE IMPLEMENTED
        }

        public bool Validate(Contract contract)
        {
            GameManager.Singleton.upgrades.tutorialDone = true;
            Unsit();
            return contract.Result == _contractType || contract.GetIsPowerContract();
        }

        public void WalkOut()
        {
            _done = true; 
            StopConversing();
            if (!agent.enabled)
            {
                agent.enabled = true;
            }
            
            if (agent.isOnNavMesh)
                agent.SetDestination(_aiController.GetRandomDespawnPoint().transform.position);
            else
            {
                if (_isThief)
                {
                    HuellController.Singleton.Reset(this);
                }

                Destroy(gameObject);
            }
        }

        public void RunOut()
        {
            
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
            GameManager.Singleton.RemoveCustomer(this);

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
            GameManager.Singleton.RemoveCustomer(this);

            agent.speed = _initialAgentSpeed * 0.25f;
            agent.SetDestination(_aiController.GetRandomDespawnPoint().transform.position);
        }

        public bool IsSpraying => _isSpraying;
        public bool IsStealing => _isStealing;
        public bool IsSneakingOut => _sneakOut;

        public void Pay()
        {
            var value = _paymentAmount * (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);

            //GameManager.Singleton.upgrades.money += value;
            GameManager.Singleton.ChangeMoneyAmount(value);
            GameManager.Singleton.OnMoneyUpdated?.Invoke(GameManager.Singleton.upgrades.money, value);
        }
        
    }
}
