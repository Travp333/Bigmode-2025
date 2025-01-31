using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scripting;
using Scripting.Customer;
using Unity.Mathematics.Geometry;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class HuellController : MonoBehaviour
{
    [SerializeField] private GameObject footHitbox;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    private bool _hitboxVisible;

    private void OnValidate()
    {
        if (!agent)
            agent = GetComponent<NavMeshAgent>();
        if (!animator)
            animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void HideHitbox()
    {
        if (_hitboxVisible)
        {
            footHitbox.SetActive(false);
            _hitboxVisible = false;
        }
    }

    public void ShowHitbox()
    {
        if (!_hitboxVisible)
        {
            footHitbox.SetActive(true);
            _hitboxVisible = true;
        }
    }

    private List<CustomerMotor> _customers;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    void Awake()
    {
        _customers = GameManager.Singleton.GetCustomerList();
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    private float _timerReconsider;

    // Update is called once per frame
    private void Update()
    {
        if (GameManager.Singleton.IsNightTime)
        {
            _lockedTarget = null;
            _huellmode = Huellmode.OriginalPos;

            _timerReconsider = 15f;

            agent.SetDestination(_originalPosition);
        }
        else
        {
            if (!_lockedTarget)
            {
                _timerReconsider -= Time.deltaTime;
                if (_timerReconsider <= 0)
                {
                    _timerReconsider = 15f;
                    SetMode(Random.Range(0, 3) == 0 ? Huellmode.Patrol : Huellmode.OriginalPos);
                }
            }
        }

        if (_lockedTarget)
        {
            if (!_lockedTarget.IsSpraying)
            {
                _lockedTarget = null;
                SetMode(Random.Range(0, 3) == 0 ? Huellmode.Patrol : Huellmode.OriginalPos);
                return;
            }
            
            var vec =  _lockedTarget.transform.position - transform.position;
            // IF DISTANCE IS CLOSE ENOUGH - KICK IN BUTT
            if (vec.magnitude < 2.0f) // TODO: CHANGE DISTANCE
            {
                if (Vector3.Angle(transform.forward, vec.normalized) < 10f)
                {
                    animator.SetBool("isWalking", false);
                    StartCoroutine(KickAndGoBack());
                }
                else
                {
                    var targetRotation = Quaternion.LookRotation(_lockedTarget.transform.position - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }

            // CHECK DISTANCE TO TARGET
            return;
        }

        _lockedTarget = _customers.Where(n => n.IsSpraying || n.IsStealing).Where(n =>
            {
                var vec = n.transform.position - transform.position;

                var angle = Vector3.Angle(transform.forward, vec.normalized);

                return angle < 60f || vec.magnitude < 3.0f;
            })
            .OrderBy(n => Vector3.Distance(n.transform.position, transform.position)).FirstOrDefault();

        Debug.DrawRay(transform.position, transform.forward * 3f, Color.red);
        
        if (_lockedTarget)
        {
            agent.isStopped = false;
            animator.SetBool("isWalking", true);
            agent.SetDestination(_lockedTarget.transform.position);
        }
        else
        {
            if (_huellmode == Huellmode.OriginalPos && agent.remainingDistance <= 0.1f)
            {
                agent.isStopped = true;
                animator.SetBool("isWalking", false);
                transform.rotation = Quaternion.Lerp(transform.rotation, _originalRotation, Time.deltaTime * 2f);
            }

            if (_huellmode == Huellmode.Patrol && agent.remainingDistance <= 0.1f)
            {
                agent.isStopped = true;
                animator.SetBool("isWalking", false);
                var centerVec = new Vector3(1.4f, transform.position.y, -30.765f);

                var targetRotation = Quaternion.LookRotation(centerVec - transform.position);

                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.up * 2.5f,
            $"HasTarget: {_lockedTarget}, Mode: {_huellmode}, PositionTarget: {agent.destination}");
    }
#endif

    private void SetMode(Huellmode mode)
    {
        _huellmode = mode;

        _lockedTarget = null;

        if (_huellmode == Huellmode.OriginalPos)
        {
            agent.SetDestination(_originalPosition);
        }

        if (_huellmode == Huellmode.Patrol)
        {
            _waitingSpot = AiController.Singleton.GetRandomWaitingSpot();
            agent.SetDestination(_waitingSpot.Value);
        }
    }

    private Vector3? _waitingSpot;

    private enum Huellmode
    {
        Patrol,
        OriginalPos,
    }

    private Huellmode _huellmode = Huellmode.OriginalPos;

    private IEnumerator KickAndGoBack()
    {
        //TODO: DO KICK
        animator.Play("Attack");
        _lockedTarget = null;

        animator.SetBool("isWalking", true);

        yield return new WaitForSeconds(0.5f);

        SetMode(Random.Range(0, 3) == 0 ? Huellmode.Patrol : Huellmode.OriginalPos);
    }

    private CustomerMotor _lockedTarget;
}
