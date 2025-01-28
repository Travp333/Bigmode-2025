using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Vector3 = UnityEngine.Vector3;

namespace Scripting.Desk
{
    public class DeskArms : MonoBehaviour
    {
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

        [Header("Animators")]
        [SerializeField]
        Animator leftHandAnim, rightHandAnim;

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

        void Update()
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

        public void ResetContractAnimation()
        {
            leftHandAnim.Play("Dropping Paper");
        }
    }
}
