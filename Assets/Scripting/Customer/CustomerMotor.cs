using System.Collections;
using AI;
using Scripting.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Scripting.Customer
{
    public class CustomerMotor : MonoBehaviour
    {
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
        Animator anim;
        bool conversing;
        Movement player;
        [SerializeField]
        //how likely are you to play the conversation variant animation
        int converseVariantProbability = 5;

        private void Awake()
        {
            //needed to find direction to face
            player = FindFirstObjectByType<Movement>();
            //needed to aaffect animations
            anim = GetComponent<Animator>();

            _aiController = FindFirstObjectByType<AiController>();
            _changeTaskCooldown = secondsUntilChangeActivity;
        }

        private void Start()
        {
            WalkIn();
        }
        //start conversation
        public void StartConversing(){
            conversing = true;
            anim.Play("Conversing");
        }
        //end conversatioon
        public void StopConversing(){
            conversing = false;
        }
        //called in animation
        public void DecideToPlayVariant(){
            int rand = Random.Range(0, converseVariantProbability);
            if(rand - 1 >= 0){
                if(rand == converseVariantProbability - 1){
                    anim.SetBool("playConverseVariant", true);
                    anim.SetInteger("WhichConverseVariant", Random.Range(0,3));
                    Debug.Log("Playing Variant!");
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
                agent.isStopped = false;
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
            yield return new WaitForSeconds(1f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Weapon"))
            {
                rb.isKinematic = false;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.up * 1.5f, "Stresslevel: " + StressMeter);
        }
#endif

        private void RemoveMoney()
        {
            // HAS TO BE IMPLEMENTED
        }

        public void WalkOut()
        {
            _done = true;

            agent.SetDestination(_aiController.GetRandomDespawnPoint().transform.position);
        }
    }
}
