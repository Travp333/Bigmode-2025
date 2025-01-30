using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Vector3 = UnityEngine.Vector3;

namespace Scripting.Desk
{
    public class DeskArms : MonoBehaviour
    {
        [SerializeField] private GameObject paperBallSpawnPos;
        [SerializeField] private GameObject paperBallPrefab;
        [SerializeField] private float paperBallSpeed;

        [Header("leftArm")]
        [SerializeField] private GameObject leftArmIk;

        [SerializeField] private GameObject leftArm;
        [SerializeField] private Rig leftArmRig;

        [Header("rightArm")]
        [SerializeField] private GameObject rightArmIk;

        [SerializeField] private GameObject rightArm;
        [SerializeField] private Rig rightArmRig;

        [Header("Physics")]
        [SerializeField] private LayerMask layerMask;

        [SerializeField] private LayerMask layerMaskContract;

        [Header("Animators")]
        [SerializeField]
        private Animator leftHandAnim, rightHandAnim;


        [SerializeField] private GameObject contractAttachmentPoint;

        public bool HasContract => Contract;

        public Contract Contract => GetComponentInChildren<Contract>();

        public void PutDownContract()
        {
            var contract = Contract;
            if (contract)
            {
                contract.SetActive(false);
            }
        }

        public Transform GetContractObject()
        {
            if(Contract){
                return Contract.transform.parent;
            }
            else{
                return null;
            }
        }

        public void SetContractObject(Transform contract)
        {
            contract.transform.parent = contractAttachmentPoint.transform;
            contract.transform.localPosition = Vector3.zero;
            contract.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            leftHandAnim = leftArm.GetComponent<Animator>();
            rightHandAnim = rightArm.GetComponent<Animator>();
        }

        public void BlockLeftHand()
        {
            _isBlockedLeft = true;

            if (_leftHandActive)
                StartCoroutine(SetRigTo(0, leftArmRig, leftArm, true));
        }

        public void UnblockLeftHand()
        {
            _isBlockedLeft = false;
            leftHandAnim.SetBool("LeftPoint", true);
        }

        public void BlockRightHand()
        {
            _isBlockedRight = true;

            if (_rightHandActive)
                StartCoroutine(SetRigTo(0, rightArmRig, rightArm, false));
        }

        public void UnblockRightHand()
        {
            _isBlockedRight = false;
            rightHandAnim.SetBool("RightPoint", true);
        }

        public void ActivateLeftHand()
        {
            _leftHandActive = true;
        }

        public void DeactivateLeftHand()
        {
            _leftHandActive = false;
        }

        private bool _isBlockedLeft = true;
        private bool _isBlockedRight = true;

        private bool _leftHandActive = true;
        private bool _rightHandActive = true;

        private void Update()
        {
            var pos = Input.mousePosition;

            if (!_isBlockedLeft && !_isBlockedRight)
            {
                if (pos.x + 50f < Screen.width / 2f && _rightHandActive || _rightHandActive == _leftHandActive)
                {
                    _leftHandActive = true;
                    _rightHandActive = false;

                    leftHandAnim.SetBool("LeftPoint", true);
                    rightHandAnim.SetBool("RightPoint", false);
                    StartCoroutine(SetRigTo(1, leftArmRig, leftArm, true));
                    StartCoroutine(SetRigTo(0, rightArmRig, rightArm, false));
                }

                if (pos.x - 50f > Screen.width / 2f && _leftHandActive)
                {
                    _leftHandActive = false;
                    _rightHandActive = true;

                    leftHandAnim.SetBool("LeftPoint", false);
                    rightHandAnim.SetBool("RightPoint", true);
                    StartCoroutine(SetRigTo(0, leftArmRig, leftArm, true));
                    StartCoroutine(SetRigTo(1, rightArmRig, rightArm, false));
                }
            }

            var cam = Camera.main!;
            var ray = cam.ScreenPointToRay(pos);

            if (Physics.Raycast(ray, out var hitContract, Mathf.Infinity, layerMaskContract))
            {
                var vec = hitContract.point;
                var directionreturn = Vector3.forward * 0.24f + Vector3.left * 0.15f + Vector3.down * 0.41f;

                rightArmIk.transform.position = vec + directionreturn;
                rightArmIk.transform.LookAt(vec);

                var rotationVec = rightArmIk.transform.rotation.eulerAngles;

                // x: 8, y: -91.2, 343.1

                rotationVec.z = 343.1f;
                rotationVec.y = 91.2f;
                rotationVec.x = 8f;

                rightArmIk.transform.rotation = Quaternion.Euler(rotationVec);
                return;
            }

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask))
            {
                var vec = hit.point;
                vec.y = 1.80f;

                var directionreturn = ray.direction * -1;

                directionreturn.y = 0;

                leftArmIk.transform.position = vec + directionreturn * 0.75f;
                leftArmIk.transform.LookAt(vec);

                var rotationVec = leftArmIk.transform.rotation.eulerAngles;
                rotationVec.z = -270;
                rotationVec.y += 80;

                leftArmIk.transform.rotation = Quaternion.Euler(rotationVec);

                rightArmIk.transform.position = vec + directionreturn * 0.75f;
                rightArmIk.transform.LookAt(vec);

                rotationVec = rightArmIk.transform.rotation.eulerAngles;
                rotationVec.z = 270;
                rotationVec.y -= 80;

                rightArmIk.transform.rotation = Quaternion.Euler(rotationVec);
            }
        }

        private IEnumerator SetRigTo(float value, Rig rig, GameObject arm, bool isLeft)
        {
            rig.weight = value;

            var target = new Vector3(0.4f * (isLeft ? 1 : -1), 0.0f, -1.16f) * value;

            var elapsed = 0.0f;

            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;

                rig.weight = Mathf.Lerp(rig.weight, value, elapsed * 2.0f);
                arm.transform.localPosition = Vector3.Lerp(arm.transform.localPosition, target, elapsed * 2.0f);
                yield return null;
            }
        }

        private void SpawnBall()
        {
            var ball = Instantiate(paperBallPrefab, paperBallSpawnPos.transform.position, Quaternion.identity);
            ball.GetComponent<Rigidbody>().AddForce(this.transform.forward * paperBallSpeed);
            ball.GetComponent<Rigidbody>().AddTorque(this.transform.right * paperBallSpeed);
        }

        public void ResetContractAnimation()
        {
            leftHandAnim.Play("Dropping Paper");
            Invoke(nameof(SpawnBall), .65f);
        }

        public void ResetHands()
        {
            UnblockLeftHand();
            UnblockRightHand();
        }
        
        public void ShowContractUp()
        {
            leftHandAnim.Play("Holding Paper Idle");
            rightHandAnim.SetBool("RightPoint", true);
            UnblockRightHand();
            BlockLeftHand();
        }
    }
}
