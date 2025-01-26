using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripting.Desk
{
    public class ContractStack : MonoBehaviour
    {
        [SerializeField] private GameObject contract;
        [SerializeField] private Transform downPoint;
        [SerializeField] private Transform upPoint;
        [SerializeField] private GameObject attachPoint;

        private GameObject _currentContract;

        private PlayerInput _input;

        private void Awake()
        {
            _input = new PlayerInput();

            _input.Game.Action.performed += OnAction;
        }

        private bool _isUp;

        private void OnAction(InputAction.CallbackContext ctx)
        {
            if (_isUp)
            {
                StartCoroutine(DoDownAnimation());
                _isUp = false;
            }
            else
            {
                StartCoroutine(DoUpAnimation());
                _isUp = true;
            }
        }

        private void OnEnable()
        {
            _input?.Enable();
        }

        private void OnDisable()
        {
            _input?.Disable();
        }

        private void OnMouseDown()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _currentContract = Instantiate(contract, downPoint.position, downPoint.rotation, attachPoint.transform);

                StartCoroutine(DoUpAnimation());
            }

            if (Input.GetMouseButtonDown(1))
            {
                Destroy(_currentContract);
                _currentContract = null;
            }
        }

        private const float AnimationDuration = 1.0f;

        private IEnumerator DoUpAnimation()
        {
            var elapsed = 0f;

            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / AnimationDuration;

                transform.position = Vector3.Slerp(transform.position, upPoint.transform.position, t);
                transform.rotation = Quaternion.Slerp(transform.rotation, upPoint.transform.rotation, t);

                yield return null;
            }

            transform.position = upPoint.transform.position;
            transform.rotation = upPoint.transform.rotation;
        }

        private IEnumerator DoDownAnimation()
        {
            var elapsed = 0f;

            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / AnimationDuration;

                transform.position = Vector3.Slerp(transform.position, downPoint.transform.position, t);
                transform.rotation = Quaternion.Slerp(transform.rotation, downPoint.transform.rotation, t);

                yield return null;
            }

            transform.position = downPoint.transform.position;
            transform.rotation = downPoint.transform.rotation;
        }
    }
}
