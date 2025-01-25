using System.Collections;
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

        [Header("Components")]
        [SerializeField] private Rigidbody rb;

        [SerializeField] private Camera cam;
        [SerializeField] private CapsuleCollider capsuleCollider;

        [Header("Sitting")]
        [SerializeField] private float cameraRotationSitting = 15f;

        [SerializeField] private GameObject seatEnterPosition;
        [SerializeField] private GameObject seatExitPosition;

        [Header("Debug")]
        [SerializeField] private bool debugMode;

        [SerializeField] private Button button;
        
        private Vector3 _moveInput;
        private PlayerInput _playerInput;
        private float _rotationX;
        private bool _actionPressed;
        private bool _isInChairTrigger;

        public void ExitChair()
        {
            StartCoroutine(DoExitChairAnimation());
        }

        private void OnValidate()
        {
            if (!rb)
                rb = GetComponent<Rigidbody>();
            if (!cam)
                cam = GetComponentInChildren<Camera>();
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

        private void Update()
        {
            if (BlockAction) return;

            var moveInput = _playerInput.Game.Move.ReadValue<Vector2>();
            // buggy by unity, so I have to use the old method:
            // var lookDelta = _playerInput.Game.Rotate.ReadValue<Vector2>();
            var lookDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            
#if UNITY_EDITOR
            lookDelta *= 3f;
#else
            lookDelta *= 0.3f;
#endif

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
            }

            _actionPressed = false;
        }

        public bool BlockAction { get; private set; }

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
        }
    }
}
