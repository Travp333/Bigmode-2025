using System.Collections;
using Scripting.Customer;
using Scripting.Objects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Scripting.Player
{
    [SelectionBase]
    public class Movement : MonoBehaviour
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

        private Vector3 _moveInput;
        private PlayerInput _playerInput;
        private float _rotationX;
        private bool _actionPressed;
        private bool _attackPressed;
        private bool _isInChairTrigger;
        private BaseballBat _baseballBat;
        private bool _canPickupBaseballBat;
        private bool _canInteractWithClient;
        private bool _seated;
        [SerializeField]
        float interactRayLength;
        float counter = 0f;
        float counterMax = 5f;
        [SerializeField]
        float clientInteractDistance = 5f;
        bool countDownGate;
        [SerializeField]
        Image radialIndicatorUI;
        GameObject clientInteractor;
        
        

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
            _playerInput = new PlayerInput();

            Cursor.lockState = CursorLockMode.Locked;

            _playerInput.Game.Action.performed += ActionPerformed;
            _playerInput.Game.Attack.performed += AttackPerformed;
            _playerInput.Game.Action.canceled += ActionReleased;
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
        
        private void ActionReleased(InputAction.CallbackContext context)
        {
            _actionPressed = false;
        }
        

        private void AttackPerformed(InputAction.CallbackContext context)
        {
            _attackPressed = true;
        }

        private void Update()
        {
            if (_seated)
            {
                // var targetBounds = new Vector3(cam.pixelWidth, cam.pixelHeight);
                // //Debug.Log("Target Bounds" + targetBounds);
                // Mouse.current.WarpCursorPosition(new Vector2(
                //     Mathf.Clamp(Input.mousePosition.x, seatedMouseBounds.x * targetBounds.x,
                //         (1 - seatedMouseBounds.x) * targetBounds.x),
                //     Mathf.Clamp(Input.mousePosition.y, seatedMouseBounds.y * targetBounds.y,
                //         (1 - seatedMouseBounds.y) * targetBounds.y)));
                // var targetPosition = Input.mousePosition;
                // //Debug.Log("Target Position" + targetPosition);
                // cam.transform.SetLocalPositionAndRotation(cam.transform.localPosition,
                //     Quaternion.Euler(
                //         targetPosition.y / targetBounds.y * seatedCameraBounds.y * -1.0f + seatedCameraBounds.y / 2, 0,
                //         0));
                // transform.rotation = Quaternion.Euler(0,
                //     targetPosition.x / targetBounds.x * seatedCameraBounds.x - seatedCameraBounds.x / 2, 0);
                //
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    ExitChair();
                }
            }

            if (BlockAction)
                return;

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
                _isInChairTrigger = false;
                if (_baseballBat)
                {
                    _baseballBat.Drop();
                    _baseballBat = null;
                    _actionPressed = false;
                }
            }

            if (_baseballBat && _attackPressed)
            {
                StartCoroutine(DoAttack());
                // StartCoroutine(DoAttackAnimation());
            }

            Debug.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * interactRayLength, Color.red);
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, interactRayLength))
            {
                if(hit.transform.GetComponent<BaseballBat>() != null){
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
                if(hit.transform.GetComponent<CustomerMotor>()!=null){
                    _canInteractWithClient = true;
                    if(_actionPressed){
                        counter = 0f;
                        countDownGate = true;
                        clientInteractor = hit.transform.gameObject;
                    }
                }
                else{
                    _canInteractWithClient = false;
                }

            }
            else{
                _canInteractWithClient = false;
                _canPickupBaseballBat = false;
            }
            if(countDownGate){
                if(Vector3.Distance(this.transform.position, clientInteractor.transform.position) < clientInteractDistance){
                    if(counter < counterMax){
                        counter += Time.deltaTime;
                        radialIndicatorUI.enabled = true;
                        radialIndicatorUI.fillAmount = counter / counterMax;
                    }
                    else{
                        radialIndicatorUI.enabled = false;
                        countDownGate = false;
                        counter = 0f;
                        Debug.Log("INTERACTION COMPLETE!");
                    }
                }
                else{
                    radialIndicatorUI.enabled = false;
                    countDownGate = false;
                    counter = 0f;
                    Debug.Log("INTERACTION INTERRUPTED!");
                }
            }


            _actionPressed = false;
            _attackPressed = false;
        }

        // TODO: CHANGE
        private IEnumerator DoAttack()
        {
            var elapsed = 0f;

            var originalTransform = weaponPosition.transform;

            while (elapsed < AnimationDuration / 4)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / AnimationDuration * 2;

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
            Cursor.lockState = CursorLockMode.Confined;
            _seated = true;
            // GetComponent<Contract>().SetActive(true);
        }

        private void ExitChairStart()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _seated = false;
            // GetComponent<Contract>().SetActive(false);
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
                    Quaternion.Euler(Mathf.Lerp(cam.transform.rotation.eulerAngles.x, cameraRotationSitting, t), cam.transform.rotation.eulerAngles.y,
                        cam.transform.rotation.eulerAngles.z);
                yield return null;
            }

            transform.position = seatEnterPosition.transform.position;
            transform.rotation = seatEnterPosition.transform.rotation;
            //cam.transform.rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x, cam.transform.rotation.eulerAngles.y, cam.transform.rotation.eulerAngles.z);

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

            if (_seated)
            {
                GUI.Label(new Rect(5, 30, 200, 50), "Press 'Tab' to stand up.");
            }

            if (_canPickupBaseballBat)
            {
                GUI.Label(new Rect(5, 5, 200, 50), "Press 'E' to pick up baseball bat.");
            }
            if (_canInteractWithClient)
            {
                GUI.Label(new Rect(5, 5, 200, 50), "Press 'E' to listen to client");
            }
        }
    }
}
