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
        [SerializeField]
        float documentStressDecrease = .1f;

        [SerializeField]
        MusicSwitcher switchMus;

        [SerializeField]
        AudioSource RageModeActivate, RageModePassive;

        [SerializeField]
        float stressRate = .125f;

        private static readonly int HoldingDocument = Animator.StringToHash("HoldingDocument");

        [SerializeField]
        Image crosshair;

        [SerializeField]
        GameObject RageModeOverlay;

        [SerializeField]
        Image rageModeUIFrontBase, rageModeUIFrontOVERFULL, rageModeUIFillBase, rageModeUIFillOVERFULL;

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
        [SerializeField] private Image graffitiRemoveProgress;

        [Header("DeskButtons")]
        [SerializeField] private GameObject submit;

        [SerializeField] private GameObject reset;
        [SerializeField] private LayerMask buttonLayerMask;

        [Header("ContractPoint")]
        [SerializeField] private GameObject contractAttachmentPoint;

        [Header("other")]
        [SerializeField] private LayerMask mask;

        [SerializeField] private LayerMask graffitiMask;
        [SerializeField] private GameObject attachmentPointContract;
        [SerializeField] private AudioSource myBatSwingSound;
        [SerializeField] private AudioSource myStaplerSound;

        [Header("Graffiti")]
        [SerializeField] private GameObject sitDown;

        [SerializeField] private GameObject getUp;
        [SerializeField] private GameObject talk;
        [SerializeField] private GameObject graffiti;
        [SerializeField] private GameObject baseballbat;

        [Header("Misc")]
        [SerializeField] private Phone phone;

        private Vector3 _moveInput;
        private PlayerInput _playerInput;
        private float _rotationX;
        private bool _actionPressed;
        private bool _attackPressed;
        private bool _isInChairTrigger;
        private BaseballBat _baseballBat;
        private bool _canPickupBaseballBat;
        private bool _canInteractWithClient;
        private bool _canInteractWithGraffiti;
        private bool _seated;
        private bool _phoneRinging;
        public bool onPhone;
        private bool _isSmoking;
        private VandalismSpot _currentGraffiti;

        private float _counter = 0f;
        [SerializeField] float clientConversationLength = 1.5f;

        private float _graffitiRemoveCounter = 1.5f;

        private bool _countDownGate;
        private GameObject _clientInteractor;

        public float StressLevel { get; private set; }
        public bool BlockAction { get; private set; }
        public bool HasContract => _currentContract;

        [SerializeField]
        private GameObject playerHands;

        [SerializeField]
        private Animator handAnim;

        public bool rageMode;

        [SerializeField]
        GameObject staplerMesh;

        [SerializeField] private float rageModeTimer = 5f;

        bool chargeBlock;

        private bool _mailboxTutorial;
        private bool _customerTutorial;

        public void ExitChair()
        {
            if (!_mailboxTutorial)
            {
                TutorialManager.Singleton.HideOrderNumber(7);
                _mailboxTutorial = true;
            }

            if (!_customerTutorial)
            {
                TutorialManager.Singleton.HideOrderNumber(5);
                TutorialManager.Singleton.HideOrderNumber(9);
                _customerTutorial = true;
            }

            getUp.SetActive(false);
            StartCoroutine(DoExitChairAnimation());
            DeactivateContractControls();
        }

        private void ShowHands()
        {
            leftHand.SetActive(true);
            rightHand.SetActive(true);
        }

        private void HideHands()
        {
            leftHand.SetActive(false);
            rightHand.SetActive(false);
        }

        private void ActivateContractControls()
        {
            _currentContract?.SetActive(true);
        }

        private Contract _currentContract;

        public void SetActiveContract(Contract contract) => _currentContract = contract;

        private void DeactivateContractControls()
        {
            _currentContract?.SetActive(false);
        }

        public bool CanAct()
        {
            if (!_seated)
                return false;
            if (_currentContract)
                return false;
            if (_isSmoking)
                return false;
            if (onPhone)
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
            Debug.Log("Try Submitting contract");
            _currentContract?.Submit();
        }

        public void ResetStressLevel()
        {
            StressLevel = 0f;
        }

        public void ResetContract()
        {
            if (!_currentContract) return;
            var deskArms = bothArmsScript.GetComponent<DeskArms>();
            deskArms.UnblockLeftHand();
            deskArms.ResetContractAnimation();
            var x = _currentContract;
            Debug.Log("Resetting contract");
            _currentContract = null;
            x?.Reset();
        }

        private void CheckButtons()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, Mathf.Infinity, buttonLayerMask))
                {
                    if (hit.collider.gameObject == submit)
                    {
                        SubmitContract();
                        if (hit.collider.gameObject.GetComponent<Animator>() != null)
                        {
                            hit.collider.gameObject.GetComponent<Animator>().Play("ButtonAnim");
                        }
                    }

                    if (hit.collider.gameObject == reset)
                    {
                        ResetContract();
                        if (hit.collider.gameObject.GetComponent<Animator>() != null)
                        {
                            hit.collider.gameObject.GetComponent<Animator>().Play("ButtonAnim");
                        }
                    }
                }
            }
        }

        private void EndRageMode()
        {
            handAnim.SetBool("RAGE", false);
            rageMode = false;
            rageModeUIFillBase.gameObject.SetActive(true);
            rageModeUIFrontBase.gameObject.SetActive(true);
            rageModeUIFillOVERFULL.gameObject.SetActive(false);
            rageModeUIFrontOVERFULL.gameObject.SetActive(false);
            chargeBlock = false;
            RageModeOverlay.SetActive(false);
            RageModePassive.Stop();
            if (GameManager.Singleton.IsNightTime)
            {
                switchMus.NightTime();
            }
            else
            {
                switchMus.DayTime();
            }
        }

        private void Update()
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward,
                    out var hitGraffiti, interactRayLength, graffitiMask))
            {
                _currentGraffiti = hitGraffiti.transform.GetComponent<VandalismSpot>();

                if (_currentGraffiti.IsVisible)
                {
                    if (!_removingGraffiti)
                    {
                        if (!_canInteractWithGraffiti)
                            graffiti.SetActive(true);
                        _canInteractWithGraffiti = true;
                        if (_actionPressed)
                        {
                            StartCoroutine(RemoveGraffiti());
                        }
                    }
                }
                else
                {
                    graffitiRemoveProgress.enabled = false;

                    if (_canInteractWithGraffiti)
                        graffiti.SetActive(false);
                    _canInteractWithGraffiti = false;
                }
            }
            else
            {
                _currentGraffiti = null;
                graffitiRemoveProgress.enabled = false;

                if (_canInteractWithGraffiti)
                    graffiti.SetActive(false);
                _canInteractWithGraffiti = false;
            }

            if (GameManager.Singleton.IsNightTime)
            {
                if (_currentContract)
                {
                    handAnim.SetBool("HoldingDocument", false);
                    handAnim.Play("IDLE");
                    Debug.Log("Resetting contract");
                    var x = _currentContract;
                    _currentContract = null;
                    //TODO: PLAY THROW ANIMATION #1
                    x?.Reset();
                }

                if (StressLevel > 0f)
                    ResetStressLevel();
                if (_seated)
                    ExitChair();
                if (rageMode)
                    EndRageMode();

                if (_baseballBat)
                {
                    _baseballBat.Drop();
                    _baseballBat = null;
                    _actionPressed = false;
                }

                _actionPressed = false;
            }

            else
            {
                var stressChange = stressRate;

                if (_seated)
                {
                    crosshair.gameObject.SetActive(false);
                    stressChange /= 2f;
                    if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) ||
                         Input.GetKeyDown(KeyCode.D)) && !_isSmoking && !onPhone)
                    {
                        var contract = bothArmsScript.GetContractObject();

                        if (contract)
                        {
                            contract.transform.parent = attachmentPointContract.transform;
                            contract.transform.localPosition = Vector3.zero;
                            contract.transform.localRotation = Quaternion.identity;
                        }

                        ExitChair();
                    }

                    CheckButtons();
                }

                if (rageMode)
                {
                    stressChange = 0f;
                }

                if (_phoneRinging)
                {
                    stressChange *= 1.5f;
                }

                if (_isSmoking)
                {
                    stressChange -= 0.15f;
                }

                StressLevel += Time.deltaTime * stressChange;

                if (StressLevel < 0.0f)
                {
                    StressLevel = 0f;
                }

                //UI here
                rageModeUIFillBase.fillAmount = StressLevel;

                if (StressLevel >= 1.0f)
                {
                    if (_baseballBat)
                    {
                        _baseballBat.Drop();
                        _baseballBat = null;
                        _actionPressed = false;
                    }

                    if (onPhone)
                    {
                        phone.ConversationEndEarly(true);
                    }

                    if (_currentContract)
                    {
                        handAnim.SetBool("HoldingDocument", false);
                        handAnim.Play("IDLE");
                        Debug.Log("Resetting contract");
                        var x = _currentContract;
                        _currentContract = null;
                        x?.Reset();
                        //TODO: PLAY THROW ANIMATION #2

                        ResetContract();
                    }

                    if (_seated)
                    {
                        // bothArmsScript.PutDownContract();

                        var contract = bothArmsScript.GetContractObject();
                        if (contract)
                        {
                            contract.transform.parent = attachmentPointContract.transform;
                            contract.transform.localPosition = Vector3.zero;
                            contract.transform.localRotation = Quaternion.identity;
                        }

                        ExitChair();
                    }

                    rageModeUIFillBase.gameObject.SetActive(false);
                    rageModeUIFrontBase.gameObject.SetActive(false);
                    rageModeUIFillOVERFULL.gameObject.SetActive(true);
                    rageModeUIFrontOVERFULL.gameObject.SetActive(true);
                    //UI HERE
                    //BlockAction = true;
                    rageMode = true;
                    handAnim.Play("RAGEMODE");
                    handAnim.SetBool("RAGE", true);
                    GameManager.Singleton.StressmeterTooHigh();
                    StressLevel = 0f;
                    Invoke(nameof(EndRageMode), rageModeTimer);
                    chargeBlock = true;
                    RageModeOverlay.SetActive(true);
                    RageModeActivate.Play();
                    RageModePassive.Play();
                    switchMus.PlayRageMode();
                }
            }


            if (BlockAction)
                return;
            if (chargeBlock)
            {
                rb.AddForce(this.transform.forward * (1000f * Time.deltaTime));
            }
            else
            {
                var moveInput = _playerInput.Game.Move.ReadValue<Vector2>();
                var move = transform.right * moveInput.x + transform.forward * moveInput.y;

                var vector = move * moveSpeed;
                vector.y = rb.linearVelocity.y;

                rb.linearVelocity = vector;

                if (rb.linearVelocity.magnitude > 0.5f)
                {
                    handAnim.SetBool("Walking", true);
                }
                else
                {
                    handAnim.SetBool("Walking", false);
                }
            }

            var lookDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSpeed;
            var mouseX = lookDelta.x;
            var mouseY = lookDelta.y;


            _rotationX -= mouseY;
            _rotationX = Mathf.Clamp(_rotationX, -80f, 80f);

            cam.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.Rotate(Vector3.up * mouseX);

            if (_isInChairTrigger && !GameManager.Singleton.IsNightTime && !rageMode)
            {
                var contract = GetComponentInChildren<Contract>();
                if (contract)
                {
                    //Debug.Log("Re entering seat while holding this document" + contract);
                    bothArmsScript.SetContractObject(contract.transform.parent);
                    handAnim.Play("IDLE");
                    handAnim.SetBool("HoldingDocument", false);
                }

                StartCoroutine(DoSitChairAnimation());
                if (_currentContract)
                {
                    _currentContract.GetComponent<BoxCollider>().enabled = true;
                    ActivateContractControls();
                }

                sitDown.SetActive(false);
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
                myBatSwingSound.Play();
                handAnim.Play("Swing Bat Miss");
                //StartCoroutine(DoAttack());
                // StartCoroutine(DoAttackAnimation());
            }

            Debug.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * interactRayLength,
                Color.red);

            if (!GameManager.Singleton.IsNightTime && Physics.Raycast(cam.transform.position, cam.transform.forward,
                    out var hit, interactRayLength, mask))
            {
                if (hit.transform.GetComponent<BaseballBat>())
                {
                    var bat = hit.transform.GetComponent<BaseballBat>();
                    if (bat)
                    {
                        if (!_canPickupBaseballBat && !GameManager.Singleton.IsNightTime)
                            baseballbat.SetActive(true);
                        _canPickupBaseballBat = true;
                        if (_actionPressed)
                        {
                            _baseballBat = bat;
                            _baseballBat.PickUp();
                        }
                    }
                }
                else
                {
                    if (_canPickupBaseballBat && !GameManager.Singleton.IsNightTime)
                        baseballbat.SetActive(false);
                    _canPickupBaseballBat = false;
                }

                if (hit.transform.gameObject.CompareTag("Trashcan"))
                {
                    if (_currentContract)
                    {
                        talk.SetActive(true);
                        //INTERACT UI HERE
                    }

                    if (_actionPressed)
                    {
                        if (_currentContract)
                        {
                            handAnim.SetBool("HoldingDocument", false);
                            handAnim.Play("IDLE");
                            talk.SetActive(false);
                            // TODO: Play throw Animation?
                            ResetContract();
                        }
                    }
                }

                if (hit.transform.gameObject.CompareTag("Mailbox"))
                {
                    if (_currentContract)
                    {
                        if (_currentContract.name == "ds" || _currentContract.name == "la" ||
                            _currentContract.name == "tec" || _currentContract.name == "eel")
                        {
                            //INTERACT UI HERE
                            talk.SetActive(true);
                        }
                    }

                    if (_actionPressed)
                    {
                        if (_currentContract && _currentContract.GetIsMailBoxContract())
                        {
                            GameManager.Singleton.upgrades.tutorialDone = true;

                            var abc = _currentContract;

                            abc.ExecuteMailboxEffect(this);
                            handAnim.SetBool(HoldingDocument, false);
                            Destroy(_currentContract.gameObject);
                            _currentContract = null;
                            talk.SetActive(false);
                        }
                    }
                }

                if (hit.transform.TryGetComponent<CustomerMotor>(out var customer))
                {
                    //Debug.Log("Looking at client!");
                    if (!_canInteractWithClient &&
                        !GameManager.Singleton.IsNightTime &&
                        customer.IsGoodGuy)
                        talk.SetActive(true);
                    _canInteractWithClient = true;

                    if (_actionPressed)
                    {
                        if (_currentContract && !_currentContract.GetIsMailBoxContract())
                        {
                            if (!_stapledTutorial)
                            {
                                _stapledTutorial = true;
                                TutorialManager.Singleton.HideOrderNumber(5);
                            }

                            if (customer.IsGoodGuy)
                            {
                                customer.transform.rotation =
                                    Quaternion.LookRotation(-this.transform.forward, this.transform.up);
                                customer.anim.Play("GetStapled");
                            }

                            var attachment = _currentContract.transform.parent;
                            attachment.parent = customer.documentAttachPoint.transform;
                            attachment.position = customer.documentAttachPoint.transform.position;
                            attachment.rotation = customer.documentAttachPoint.transform.rotation;
                            attachment.localScale = Vector3.one / 2.0f;
                            //Debug.Log("Interacting with document!");
                            //RemoveContract();
                            staplerMesh.SetActive(true);
                            handAnim.Play("Staple");
                            myStaplerSound.Play();
                            Invoke(nameof(HideStaplerMesh), 1f);
                            handAnim.SetBool(HoldingDocument, false);
                            var abc = _currentContract;
                            _currentContract = null;
                            if (abc.Converted)
                            {
                                if (customer.Validate(abc))
                                {
                                    abc.ExecuteEffect(customer, this);
                                }
                            }
                        }
                        else
                        {
                            //Debug.Log("Interacting with no document!");
                            _counter = 0f;
                            _countDownGate = true;
                            _clientInteractor = hit.transform.gameObject;
                        }
                    }
                }
                else
                {
                    if (_canInteractWithClient)
                        talk.SetActive(false);

                    _canInteractWithClient = false;
                }
            }
            else
            {
                if (_canInteractWithClient)
                    talk.SetActive(false);
                _canInteractWithClient = false;

                if (_canPickupBaseballBat && !GameManager.Singleton.IsNightTime)
                    baseballbat.SetActive(false);
                _canPickupBaseballBat = false;
            }

            if (_countDownGate)
            {
                if (_clientInteractor != null && _clientInteractor.GetComponent<CustomerMotor>().IsGoodGuy)
                {
                    if (Vector3.Distance(transform.position, _clientInteractor.transform.position) <
                        clientInteractDistance)
                    {
                        if (_counter < clientConversationLength)
                        {
                            var motor = _clientInteractor.GetComponent<CustomerMotor>();
                            motor.StartConversing();
                            motor.UnlockAssistant();

                            _counter += Time.deltaTime;
                            radialIndicatorUI.enabled = true;
                            radialIndicatorUI.fillAmount = _counter / clientConversationLength;
                        }
                        else
                        {
                            var customer = _clientInteractor.GetComponent<CustomerMotor>();
                            customer.StopConversing();
                            customer.ShowBubble();
                            radialIndicatorUI.enabled = false;
                            _countDownGate = false;
                            _counter = 0f;
                            Debug.Log("INTERACTION COMPLETE!");
                            customer.TutorialBubble.SetActive(false);
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
            }

            _actionPressed = false;
            _attackPressed = false;
        }

        private bool _removingGraffiti;

        private IEnumerator RemoveGraffiti()
        {
            var elapsed = 0.0f;

            graffitiRemoveProgress.enabled = true;
            _removingGraffiti = true;

            while (elapsed < _graffitiRemoveCounter && _currentGraffiti)
            {
                elapsed += Time.deltaTime;

                graffitiRemoveProgress.fillAmount = elapsed / _graffitiRemoveCounter;
                yield return true;
            }

            graffitiRemoveProgress.enabled = false;
            _removingGraffiti = false;

            _currentGraffiti?.Remove();
        }

        void HideStaplerMesh()
        {
            staplerMesh.SetActive(false);
        }

        public void ChangeStressLevel()
        {
            StressLevel += documentStressDecrease;
            if (StressLevel < 0f)
            {
                StressLevel = 0f;
            }

            if (StressLevel > 1f)
            {
                StressLevel = 1f;
            }
        }

        private void SitOnChairStart()
        {
            BlockAction = true;
            rb.isKinematic = true;
            capsuleCollider.enabled = false;
            handAnim.Play("IDLE");
            handAnim.SetBool("Walking", false);
        }

        private bool _tutorialSeatedDone;
        private bool _stapledTutorial;

        private void SitOnChairDone()
        {
            ShowHands();
            Cursor.lockState = CursorLockMode.Confined;

            if (!_tutorialSeatedDone)
            {
                TutorialManager.Singleton.ShowOrderNumber(2, true);
                _tutorialSeatedDone = true;
            }

            _seated = true;
            getUp.SetActive(true);
            bothArmsScript.UnblockRightHand();
            if (!_currentContract)
            {
                bothArmsScript.ResetHands();
            }
            else
            {
                bothArmsScript.BlockLeftHand();
                bothArmsScript.ShowContractUp();
            }
        }

        private void ExitChairStart()
        {
            HideHands();
            Cursor.lockState = CursorLockMode.Locked;
            _seated = false;
            getUp.SetActive(false);
            crosshair.gameObject.SetActive(true);
            // GetComponent<Contract>().SetActive(false);
        }

        private void ExitChairDone()
        {
            BlockAction = false;
            rb.isKinematic = false;
            capsuleCollider.enabled = true;

            //var contTest = bothArmsScript.HasContract;
            if (_currentContract)
            {
                handAnim.SetBool("HoldingDocument", true);
                _currentContract.GetComponent<BoxCollider>().enabled = false;
            }
            else
            {
                handAnim.SetBool("HoldingDocument", false);
            }
        }

        private const float AnimationDuration = .3f;

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
            cam.transform.rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x,
                cam.transform.rotation.eulerAngles.y, cam.transform.rotation.eulerAngles.z);

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
                if (!GameManager.Singleton.IsNightTime)
                {
                    _isInChairTrigger = true;
                    sitDown.SetActive(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("ChairTrigger"))
            {
                _isInChairTrigger = false;
                sitDown.SetActive(false);
            }
        }

        public void NotifyOnPhone()
        {
            onPhone = true;
        }

        public void NotifyNotOnPhone()
        {
            onPhone = false;
        }

        public void NotifyPhoneRinging()
        {
            _phoneRinging = true;
        }

        public void NotifyPhoneStopped()
        {
            _phoneRinging = false;
        }

        public void NotifyIsSmoking()
        {
            _isSmoking = true;
        }

        public void NotifyStoppedSmoking()
        {
            _isSmoking = false;
        }

        public void SetStressLevel(float value)
        {
            StressLevel = value;
        }

        public float MouseSpeed
        {
            get { return mouseSpeed; }
            set { mouseSpeed = value; }
        }
    }
}
