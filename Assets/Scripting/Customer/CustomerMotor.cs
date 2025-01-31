using System;
using System.Collections;
using Scripting.Player;
using Scripting.Desk;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Scripting.Customer
{
    public class CustomerMotor : MonoBehaviour
    {
        [SerializeField]
        public GameObject documentAttachPoint;

        [SerializeField] private Rigidbody rb;
        [SerializeField] private NavMeshAgent agent;

        [SerializeField] private float secondsUntilFreakOut = 60.0f;
        [SerializeField] private float secondsUntilChangeActivity = 3.0f;

        [SerializeField] private CapsuleCollider capsuleCollider;

        [SerializeField]
        //how likely are you to play the conversation variant animation
        private int converseVariantProbability = 5;

        [SerializeField] private Billboard bubble;

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

        [SerializeField]
        int motherFuckerOdds = 5;

        public bool IsHuellTarget { get; set; }

        private void Awake()
        {
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
            _isMotherfucker = _aiController.HasVandalismSpots && false; //Random.Range(0, 10) == 0;

            if (!_isMotherfucker)
            {
                _isThief = _aiController.HasThiefSpot && false; //Random.Range(0, 1) == 0;
            }

            _aiController = FindFirstObjectByType<AiController>();
            _changeTaskCooldown = secondsUntilChangeActivity;

            _standardDocuments = TrainingsData.StandardContractTypes;
            _index = Random.Range(0, _standardDocuments.Count);

            _contractType = _standardDocuments[_index];
        }

        private void Start()
        {
            WalkIn();
        }

        //start conversation
        public void StartConversing()
        {
            _conversing = true;
            //Debug.Log("Starting Conversation");
            //anim.Play("Conversing");
        }

        //end conversatioon
        public void StopConversing()
        {
            _conversing = false;
            agent.isStopped = false;
        }

        public void ShowBubble()
        {
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
                transform.rotation = Quaternion.LookRotation(-_player.transform.forward, _player.transform.up);
                anim.SetBool("conversing", true);
            }
            else
            {
                anim.SetBool("conversing", false);
            }
        }

        private void Update()
        {
            if (!agent.hasPath)
            {
                if (GameManager.Singleton.IsNightTime)
                {
                    Destroy(gameObject);
                }
            }

            if (!agent.isOnNavMesh)
            {
                transform.position = Vector3.Lerp(transform.position, _aiFixPosition, Time.deltaTime * 2.0f);
            }

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
                StressMeter += Time.deltaTime / (secondsUntilFreakOut * (_currentSpot ? 2f : 1f));
                //handles rotation
                HandleConversing();

                if (StressMeter >= 1.0f)
                {
                    _done = true;

                    RemoveMoney();

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

                        StartCoroutine(GoToTargetWithCallback(_aiController.AssistantSpot.transform.position, () =>
                            {
                                // TODO: Play Talk Animation with Assistant

                                DecideToPlayVariant();
                            },
                            () =>
                            {
                                // TODO: Stop Talk Animation with Assistant
                                Assistant.Singleton.PayBubbleGum();
                                ShowBubble();
                            }, 2.5f, () =>
                            {
                                _aiController.AssistantLocked = false;
                            }));

                        return;
                    }

                    var nextSpot = _aiController.GetFreeSpot();

                    if (_currentSpot)
                    {
                        _currentSpot.Leave();
                    }

                    if (nextSpot)
                    {
                        nextSpot.Arrive();
                        _currentSpot = nextSpot;
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
                                //TODO: anim.Play("Steal");
                                _isStealing = true;
                            },
                            () =>
                            {
                                _stolenMoney = 20; // TODO SET NEW VALUE
                                GameManager.Singleton.MoneyStolen(_stolenMoney);
                                SneakOut();
                                _isStealing = false;
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
            agent.SetDestination(position);

            yield return null;

            while (agent.remainingDistance > 0.1f && !_done && !_runOut && !_sneakOut && !_sprayInterrupted &&
                   !_stealInterrupted)
            {
                yield return null;
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
            var entrance = _aiController.EntrancePoint.transform.position;

            agent.SetDestination(entrance);
        }

        private bool _sprayInterrupted;
        private bool _stealInterrupted;

        public void InterruptSpraying()
        {
            _sprayInterrupted = true;

            _isSpraying = false;

            if (!_vandalismSpot.IsVisible)
                _vandalismSpot.IsLocked = false;
        }

        private int _stolenMoney;

        public void InterruptStealing()
        {
            _stealInterrupted = true;
            _isStealing = false;

            _aiController.SetThiefLocked(false);
        }

        private void OnValidate()
        {
            if (rb)
                rb = GetComponent<Rigidbody>();
            if (capsuleCollider)
                capsuleCollider = GetComponent<CapsuleCollider>();
        }

        public void GetHit()
        {
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
            GameManager.Singleton.upgrades.money -= _penalty;
            GameManager.Singleton.OnMoneyUpdated?.Invoke(GameManager.Singleton.upgrades.money, -_penalty);
            // HAS TO BE IMPLEMENTED
        }

        public bool Validate(Contract contract) =>
            contract.Result == _contractType || contract.GetIsPowerContract();

        public void WalkOut()
        {
            //TODO: Stop Spray Animation
            _done = true;

            if (agent.isOnNavMesh)
                agent.SetDestination(_aiController.GetRandomDespawnPoint().transform.position);
            else
            {
                Destroy(gameObject);
            }
        }

        // TODO: CALL THIS WHEN GET HIT
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

            // TODO: CHANGE ANIMATION

            agent.speed = _initialAgentSpeed * 2f;
            // TODO: Change Agent Speed
            agent.SetDestination(_aiController.GetRandomDespawnPoint().transform.position);
        }

        public void IsHit()
        { 
            if (_stolenMoney > 0)
            {
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
            // TODO: ANIMATIONS
            // anim.SetBool("isSneaking", true);
            // anim.Play("SNEAK");
            GameManager.Singleton.RemoveCustomer(this);

            agent.speed = _initialAgentSpeed * 0.25f;
            // TODO: Change Agent Speed
            agent.SetDestination(_aiController.GetRandomDespawnPoint().transform.position);
        }

        public bool IsSpraying => _isSpraying;
        public bool IsStealing { get; set; }

        public void Pay()
        {
            var value = _paymentAmount * (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);

            GameManager.Singleton.upgrades.money += value;
            GameManager.Singleton.OnMoneyUpdated?.Invoke(GameManager.Singleton.upgrades.money, value);
        }
    }
}
