using System.Collections;
using Scripting.Customer;
using Scripting.Desk;
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

        [Header("Hands")]
        [SerializeField] private GameObject leftHand;
        [SerializeField] private GameObject rightHand;
        [SerializeField] private DeskArms bothArmsScript;

        [Header("Interaction")]
        [SerializeField] private float clientInteractDistance = 5f;

        [SerializeField] private float interactRayLength;
        [SerializeField] private Image radialIndicatorUI;

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
        private bool _phoneRinging;
        private bool _isSmoking;

        private float _counter = 0f;
        private float _counterMax = 5f;
        private bool _countDownGate;
        private GameObject _clientInteractor;

        public float StressLevel { get; private set; }
        public bool BlockAction { get; private set; }

        [SerializeField] private LayerMask mask;

        public void ExitChair()
        {
            StartCoroutine(DoExitChairAnimation());
            DeactivateContractControls();
            var contract = GetComponentInChildren<Contract>();
            if (contract)
            {
                contract.SetActive(false);
            }
        }

        void ShowHands()
        {
            leftHand.SetActive(true);
            rightHand.SetActive(true);
        }

        void HideHands()
        {
            leftHand.SetActive(false);
            rightHand.SetActive(false);
        }

        private void ActivateContractControls()
        {
            var contract = GetComponentInChildren<Contract>();
            if (contract)
            {
                contract.SetActive(true);
            }
        }

        private void DeactivateContractControls()
        {
            var contract = GetComponentInChildren<Contract>();
            if (contract)
            {
                contract.SetActive(false);
            }
        }

        public bool CanAct()
        {
            if (!_seated) return false;

            var contract = GetComponentInChildren<Contract>();
            if (contract && contract.IsUp)
                return false;

            if (_isSmoking)
                return false;

            return true;
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

        private void Awake()
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

        public void SubmitContract()
        {
            GetComponentInChildren<Contract>()?.Submit();
        }

        public void ResetContract()
        {
            GetComponentInChildren<Contract>()?.Reset();
        }

        private void Update()
        {
            var stressChange = 0.025f;

            if (_seated)
            {
                stressChange /= 2f;
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    ExitChair();
                }
            }

            if (_phoneRinging)
            {
                stressChange *= 1.5f;
            }

            if (_isSmoking)
            {
                stressChange -= 0.05f;
            }

            StressLevel += Time.deltaTime * stressChange;

            // if (StressLevel >= 1.0f)
            // {
            //     BlockAction = true;
            //     GameManager.Singleton.StressmeterTooHigh();
            // }

            if (BlockAction)
                return;

            var moveInput = _playerInput.Game.Move.ReadValue<Vector2>();
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
                ActivateContractControls();
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

            Debug.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * interactRayLength,
                Color.red);

            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, interactRayLength, mask))
            {
                if (hit.transform.GetComponent<BaseballBat>() != null)
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

                if (hit.transform.TryGetComponent<CustomerMotor>(out var customer))
                {
                    _canInteractWithClient = true;
                    if (_actionPressed)
                    {
                        var contract = GetComponentInChildren<Contract>();
                        if (contract)
                        {
                            var attachment = contract.transform.parent;
                            //Change this to NPC head bone
                            attachment.parent = customer.transform;
                            attachment.position = hit.point;
                            attachment.localScale = Vector3.one / 2.0f;

                            if (contract.Converted && customer.Validate())
                            {
                                GameManager.Singleton.FinalizeCustomer(customer);

                                ChangeStressLevel(-0.25f);
                                customer.WalkOut();

                                //Todo: Play Smack sound
                            }
                            else
                            {
                                //Todo: Play NO Sound
                            }
                        }
                        else
                        {
                            _counter = 0f;
                            _countDownGate = true;
                            _clientInteractor = hit.transform.gameObject;
                        }
                    }
                }
                else
                {
                    _canInteractWithClient = false;
                }
            }
            else
            {
                _canInteractWithClient = false;
                _canPickupBaseballBat = false;
            }

            if (_countDownGate)
            {
                if (Vector3.Distance(transform.position, _clientInteractor.transform.position) <
                    clientInteractDistance)
                {
                    if (_counter < _counterMax)
                    {
                        _clientInteractor.GetComponent<CustomerMotor>().StartConversing();
                        _counter += Time.deltaTime;
                        radialIndicatorUI.enabled = true;
                        radialIndicatorUI.fillAmount = _counter / _counterMax;
                    }
                    else
                    {
                        _clientInteractor.GetComponent<CustomerMotor>().StopConversing();
                        radialIndicatorUI.enabled = false;
                        _countDownGate = false;
                        _counter = 0f;
                        Debug.Log("INTERACTION COMPLETE!");
                    }
                }
                else
                {
                    _clientInteractor.GetComponent<CustomerMotor>().StopConversing();
                    radialIndicatorUI.enabled = false;
                    _countDownGate = false;
                    _counter = 0f;
                    Debug.Log("INTERACTION INTERRUPTED!");
                }
            }


            _actionPressed = false;
            _attackPressed = false;
        }


        private void ChangeStressLevel(float value)
        {
            StressLevel += value;
            if (StressLevel < 0f)
            {
                StressLevel = 0f;
            }

            if (StressLevel > 1f)
            {
                StressLevel = 1f;
            }
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
            ShowHands();
            Cursor.lockState = CursorLockMode.Confined;
            _seated = true;
            bothArmsScript.Unblock();
            // GetComponent<Contract>().SetActive(true);
        }

        private void ExitChairStart()
        {
            HideHands();
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
                    Quaternion.Euler(Mathf.Lerp(cam.transform.rotation.eulerAngles.x, cameraRotationSitting, t),
                        cam.transform.rotation.eulerAngles.y,
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

            GUI.Label(new Rect(5, Screen.height - 25, 200, 25), StressLevel.ToString());
        }

        public void NotifyPhoneRinging()
        {
            _phoneRinging = true;
        }

        public void NotifyPhoneStopped()
        {
            _phoneRinging = false;
        }

        public void BlockContractDrawing(bool value)
        {
            var contract = GetComponentInChildren<Contract>();
            if (!contract) return;
            if (value)
            {
                contract.Block();
            }
            else
            {
                contract.Unblock();
            }
        }

        public void NotifyIsSmoking()
        {
            _isSmoking = true;

            BlockContractDrawing(true);
        }

        public void NotifyStoppedSmoking()
        {
            _isSmoking = false;

            BlockContractDrawing(false);
        }
    }
}
