using System.Collections;
using System.Linq;
using AI;
using Scripting.Player;
using Scripting.Desk;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Scripting.Customer
{
    public class CustomerMotor : MonoBehaviour
    {
        [SerializeField]
        public GameObject documentAttachPoint;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private NavMeshAgent agent;

        [SerializeField] private float secondsUntilFreakOut = 20.0f;
        [SerializeField] private float secondsUntilChangeActivity = 5.0f;

        [SerializeField] private CapsuleCollider capsuleCollider;

        private AiController _aiController;
        private float _changeTaskCooldown;
        private bool _done;

        public float StressMeter { get; private set; }

        private AiSpot _currentSpot;
        private Animator anim;
        private bool conversing;
        private Movement player;
        [SerializeField]
        //how likely are you to play the conversation variant animation
        private int converseVariantProbability = 5;

        private string _contractType;

        private float _paymentAmount;
        
        private void Awake()
        {
            //needed to find direction to face
            player = FindFirstObjectByType<Movement>();
            //needed to aaffect animations
            anim = GetComponent<Animator>();

            // TODO: AI
            _paymentAmount = 3.0f;

            _aiController = FindFirstObjectByType<AiController>();
            _changeTaskCooldown = secondsUntilChangeActivity;

            var list = TrainingsData.ContractTypes;
            _contractType = list[Random.Range(0, list.Count)];
        }

        private void Start()
        {
            WalkIn();
        }
        //start conversation
        public void StartConversing(){
            conversing = true;
            //Debug.Log("Starting Conversation");
            //anim.Play("Conversing");
        }
        //end conversatioon
        public void StopConversing(){
            conversing = false;
            agent.isStopped = false;
        }
        //called in animation
        public void DecideToPlayVariant(){
            int rand = Random.Range(0, converseVariantProbability);
            if(rand - 1 >= 0){
                if(rand == converseVariantProbability - 1){
                    int rand2 = Random.Range(0,3);
                    if(rand2 == 0){
                        //Debug.Log("PLAYING VARIANT 1");
                        anim.Play("Conversing Variant 1");
                    }
                    else if(rand2 == 1){
                        //Debug.Log("PLAYING VARIANT 2");
                        anim.Play("Conversing Variant 2");
                    }
                    else if(rand2 == 2){
                        //Debug.Log("PLAYING VARIANT 3");
                        anim.Play("Conversing Variant 3");
                    }
                    else{
                        //Debug.Log("INVALID!!!" + rand2);
                    }
                }
            }

        }

        private void Update()
        {

            if (!_aiController || _done) return;

            StressMeter += Time.deltaTime / (secondsUntilFreakOut * (_currentSpot ? 2f : 1f));
            //handles rotation
            if(conversing){
                agent.isStopped = true;
                this.transform.rotation = Quaternion.LookRotation(-player.transform.forward, player.transform.up);
                anim.SetBool("conversing", true);
            }
            else{
                anim.SetBool("conversing", false);
            }

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
                _changeTaskCooldown = secondsUntilChangeActivity;

                var nextSpot = _aiController.GetFreeSpot();

                if (_currentSpot)
                {
                    _currentSpot.SetIsOccupied(false);
                }

                if (nextSpot)
                {
                    nextSpot.SetIsOccupied(true);
                    _currentSpot = nextSpot;
                    agent.SetDestination(nextSpot.transform.position);
                }
                else
                {
                    var tableSpot = _aiController.GetTableSpot();
                    tableSpot.SetIsOccupied(true);
                    _currentSpot = tableSpot;
                    agent.SetDestination(tableSpot.transform.position);
                }
            }
        }

        private void WalkIn()
        {
            if (!_aiController) return;
            var entrance = _aiController.EntrancePoint.transform.position;

            agent.SetDestination(entrance);
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
            Handles.Label(transform.position + Vector3.up * 2f, "Wants: " + _contractType);
            Handles.Label(transform.position + Vector3.up * 1.5f, "Stresslevel: " + StressMeter);
        }
#endif

        private void RemoveMoney()
        {
            // HAS TO BE IMPLEMENTED
        }

        public bool Validate()
        {
            var contracts = GetComponentsInChildren<Contract>();
            if (contracts.Any(n => n.Result == _contractType || n.GetIsPowerContract()))
            {
                return true;
            }

            return false;
        }

        public void WalkOut()
        {
            _done = true;

            agent.SetDestination(_aiController.GetRandomDespawnPoint().transform.position);
        }

        public void Pay()
        {
            GameManager.Singleton.upgrades.money += _paymentAmount;
        }
    }
}
