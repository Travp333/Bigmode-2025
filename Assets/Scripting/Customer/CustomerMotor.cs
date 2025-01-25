using System.Collections;
using AI;
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

        public float StressMeter { get; private set; }

        private AiSpot _currentSpot;

        private void Awake()
        {
            _aiController = FindFirstObjectByType<AiController>();

            _changeTaskCooldown = secondsUntilChangeActivity;
        }

        private void Start()
        {
            WalkIn();
        }

        private void Update()
        {
            if (!_aiController) return;

            if (_goingToDesk && _target.HasValue)
            {
                var distance = Vector3.Distance(transform.position, _target.Value);
                if (distance < 0.1f)
                {
                    capsuleCollider.enabled = false;
                    agent.enabled = false;
                    StartCoroutine(DoSitDeskAnimation());
                }

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

        private Vector3? _target;

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

        private bool _goingToDesk;

        public void GoToDesk(Transform tf)
        {
            _goingToDesk = true;
            _target = tf.position;
            agent.destination = tf.position;
        }

        private const float AnimationDuration = 1.0f;

        private IEnumerator DoSitDeskAnimation()
        {
            var sittingSpot = _aiController.GetDeskChairSpot();

            var elapsed = 0f;

            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / AnimationDuration;

                transform.position = Vector3.Slerp(transform.position, sittingSpot.transform.position, t);
                transform.rotation = Quaternion.Slerp(transform.rotation, sittingSpot.transform.rotation, t);
                yield return null;
            }

            transform.position = sittingSpot.transform.position;
            transform.rotation = sittingSpot.transform.rotation;
        }

        private IEnumerator DoLeaveDeskAnimation()
        {
            var leaveSpot = _aiController.GetDeskLeaveSpot();

            var elapsed = 0f;

            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / AnimationDuration;

                transform.position = Vector3.Slerp(transform.position, leaveSpot.transform.position, t);
                transform.rotation = Quaternion.Slerp(transform.rotation, leaveSpot.transform.rotation, t);
                yield return null;
            }

            transform.position = leaveSpot.transform.position;
            transform.rotation = leaveSpot.transform.rotation;

            DeskLeft();
        }

        public void LeaveDesk()
        {
            StartCoroutine(DoLeaveDeskAnimation());
        }

        private void DeskLeft()
        {
            capsuleCollider.enabled = true;
            agent.enabled = true;
            agent.SetDestination(_aiController.GetRandomDespawnPoint().transform.position);
        }
    }
}
