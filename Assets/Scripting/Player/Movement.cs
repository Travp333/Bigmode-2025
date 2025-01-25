using System.Collections;
using Objects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Scripting.Player
{
    [SelectionBase]
    public class PlayerBehaviour : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private float moveSpeed = 7.5f;

        [SerializeField] private float mouseSpeed = 3.0f;

        [Header("Components")]
        [SerializeField] private Rigidbody rb;

        [SerializeField] private CapsuleCollider capsuleCollider;

        [Header("Cameras")]
        [SerializeField] private Camera cam;

        [Header("Sitting")]
        [SerializeField] private float cameraRotationSitting = 15f;

        [SerializeField] private GameObject seatEnterPosition;
        [SerializeField] private GameObject seatExitPosition;

        [Header("Markers")]
        [SerializeField] private GameObject weaponPosition;

        [SerializeField] private GameObject attackAnimationTarget;

        [Header("Animation")]
        [SerializeField] private AnimationCurve attackCurve;

        [Header("Debug")]
        [SerializeField] private bool debugMode;

        [SerializeField] private Button button;

        private Vector3 _moveInput;
        private PlayerInput _playerInput;
        private float _rotationX;
        private bool _actionPressed;
        private bool _attackPressed;
        private bool _isInChairTrigger;
        private BaseballBat _baseballBat;
        private bool _canPickupBaseballBat;

        public bool BlockAction { get; private set; }

        public void ExitChair()
        {
            StartCoroutine(DoExitChairAnimation());
        }

        private void OnValidate()
        {
            if (!rb)
                rb = GetComponent<Rigidbody>();
            if (!capsuleCollider)
                capsuleCollider = GetComponent<CapsuleCollider>();

            if (moveSpeed < 0f)
            {
                moveSpeed = 0f;
                Debug.LogWarning("Max move speed cannot be negative. Resetting to 0.");
            }
        }

        void Awake()
        {
#if !UNITY_EDITOR
            debugMode = false;
#endif
            _playerInput = new PlayerInput();

            Cursor.lockState = CursorLockMode.Locked;

            _playerInput.Game.Action.performed += ActionPerformed;
            _playerInput.Game.Attack.performed += AttackPerformed;
        }

        private void OnEnable()
        {
            _playerInput?.Enable();
        }

        private void OnDisable()
        {
            _playerInput?.Disable();
        }

        private void ActionPerformed(InputAction.CallbackContext context)
        {
            _actionPressed = true;
        }

        private void AttackPerformed(InputAction.CallbackContext context)
        {
            _attackPressed = true;
        }

        private void Update()
        {
            if (BlockAction) return;

            var moveInput = _playerInput.Game.Move.ReadValue<Vector2>();
            // buggy by unity, so I have to use the old method:
            // var lookDelta = _playerInput.Game.Rotate.ReadValue<Vector2>();
            var lookDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSpeed;

            var mouseX = lookDelta.x;
            var mouseY = lookDelta.y;

            var move = transform.right * moveInput.x + transform.forward * moveInput.y;
            rb.linearVelocity = move * moveSpeed;

            _rotationX -= mouseY;
            _rotationX = Mathf.Clamp(_rotationX, -80f, 80f);

            cam.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.Rotate(Vector3.up * mouseX);

            if (_isInChairTrigger && _actionPressed)
            {
                StartCoroutine(DoSitChairAnimation());
                if (_baseballBat)
                {
                    _baseballBat.Drop();
                    _baseballBat = null;
                    _actionPressed = false;
                }
            }

            if (_baseballBat && _attackPressed)
            {
                StartCoroutine(DoAttackAnimation());
            }

            Debug.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * 1.5f, Color.red);
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, 1.5f))
            {
                var bat = hit.transform.GetComponent<BaseballBat>();
                if (bat)
                {
                    _canPickupBaseballBat = true;
                    if (_actionPressed)
                    {
                        _baseballBat = bat;
                        _baseballBat.PickUp(weaponPosition.transform);
                    }
                }
            }
            else
            {
                _canPickupBaseballBat = false;
            }

            _actionPressed = false;
            _attackPressed = false;
        }

        // TODO: CHANGE
        private IEnumerator DoAttackAnimation()
        {
            var elapsed = 0f;

            var originalTransform = weaponPosition.transform;

            while (elapsed < (AnimationDuration / 4))
            {
                elapsed += Time.deltaTime;
                var t = (elapsed / AnimationDuration) * 2;

                _baseballBat.transform.position = Vector3.Slerp(_baseballBat.transform.position,
                    attackAnimationTarget.transform.position, t);
                _baseballBat.transform.rotation = Quaternion.Slerp(_baseballBat.transform.rotation,
                    attackAnimationTarget.transform.rotation, t);
                yield return null;
            }

            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / AnimationDuration;

                _baseballBat.transform.position = Vector3.Slerp(_baseballBat.transform.position,
                    originalTransform.transform.position, t);
                _baseballBat.transform.rotation = Quaternion.Slerp(_baseballBat.transform.rotation,
                    originalTransform.transform.rotation, t);
                yield return null;
            }
        }

        private void SitOnChairStart()
        {
            BlockAction = true;
            rb.isKinematic = true;
            capsuleCollider.enabled = false;
        }

        private void SitOnChairDone()
        {
            // Just in case
            if (debugMode)
            {
                button.gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        private void ExitChairStart()
        {
            // Just in case
            if (debugMode)
            {
                Cursor.lockState = CursorLockMode.Locked;
                button.gameObject.SetActive(false);
            }
        }

        private void ExitChairDone()
        {
            BlockAction = false;
            rb.isKinematic = false;
            capsuleCollider.enabled = true;
        }

        private const float AnimationDuration = 1f;

        private IEnumerator DoSitChairAnimation()
        {
            SitOnChairStart();

            var elapsed = 0f;

            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / AnimationDuration;

                transform.position = Vector3.Slerp(transform.position, seatEnterPosition.transform.position, t);
                transform.rotation = Quaternion.Slerp(transform.rotation, seatEnterPosition.transform.rotation, t);
                cam.transform.rotation =
                    Quaternion.Euler(Mathf.Lerp(cam.transform.rotation.eulerAngles.x, cameraRotationSitting, t), 0f,
                        0f);
                yield return null;
            }

            transform.position = seatEnterPosition.transform.position;
            transform.rotation = seatEnterPosition.transform.rotation;
            cam.transform.rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x, 0f, 0f);

            SitOnChairDone();
        }

        private IEnumerator DoExitChairAnimation()
        {
            ExitChairStart();

            var elapsed = 0f;

            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / AnimationDuration;

                transform.position = Vector3.Slerp(transform.position, seatExitPosition.transform.position, t);
                transform.rotation = Quaternion.Slerp(transform.rotation, seatExitPosition.transform.rotation, t);

                yield return null;
            }

            transform.position = seatExitPosition.transform.position;
            transform.rotation = seatExitPosition.transform.rotation;

            ExitChairDone();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("ChairTrigger"))
            {
                _isInChairTrigger = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("ChairTrigger"))
            {
                _isInChairTrigger = false;
            }
        }

        private void OnGUI()
        {
            if (_isInChairTrigger)
            {
                GUI.Label(new Rect(5, 5, 200, 50), "Press 'E' to sit down.");
            }

            if (_canPickupBaseballBat)
            {
                GUI.Label(new Rect(5, 5, 200, 50), "Press 'E' to pick up baseball bat.");
            }
        }
    }
}
