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
        Animator leftHandAnim, rightHandAnim;
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            leftHandAnim = leftArm.GetComponent<Animator>();
            rightHandAnim = rightArm.GetComponent<Animator>();
        }
        public void Block()
        {
            _isBlocked = true;
            
            StartCoroutine(SetRigTo(0, leftArmRig, leftArm, true));
            StartCoroutine(SetRigTo(0, rightArmRig, rightArm, false));
        }

        public void Unblock() => _isBlocked = false;

        private bool _isBlocked = true;
        private bool _leftHandActive = true;

        void Update()
        {
            if (_isBlocked){ 
                leftHandAnim.SetBool("IsPointing", false);
                rightHandAnim.SetBool("IsPointing", false);
                leftHandAnim.SetBool("LeftPoint", false);
                rightHandAnim.SetBool("RightPoint", false);
                return;
            }


            var pos = Input.mousePosition;

            if (pos.x + 50f < Screen.width / 2f && !_leftHandActive)
            {
                _leftHandActive = true;
                leftHandAnim.SetBool("IsPointing", true);
                leftHandAnim.SetBool("LeftPoint", true);
                rightHandAnim.SetBool("IsPointing", false);
                rightHandAnim.SetBool("RightPoint", false);
                StartCoroutine(SetRigTo(1, leftArmRig, leftArm, true));
                StartCoroutine(SetRigTo(0, rightArmRig, rightArm, false));
            }

            if (pos.x - 50f > Screen.width / 2f && _leftHandActive)
            {
                _leftHandActive = false;
                leftHandAnim.SetBool("IsPointing", false);
                leftHandAnim.SetBool("LeftPoint", false);
                rightHandAnim.SetBool("IsPointing", true);
                rightHandAnim.SetBool("RightPoint", true);
                StartCoroutine(SetRigTo(0, leftArmRig, leftArm, true));
                StartCoroutine(SetRigTo(1, rightArmRig, rightArm, false));
            }
            
            var cam = Camera.main!;
            var ray = cam.ScreenPointToRay(pos);

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask))
            {
                var vec = hit.point;
                vec.y = 1.80f;

                var other = (cam.transform.position - hit.point).normalized * 0.05f;
                other.y = 0f;
                vec += other;

                leftArmIk.transform.position = vec;
                // leftArmIk.transform.forward = leftForwardVectorCopy.forward;
                rightArmIk.transform.position = vec;
                // leftArmIk.transform.forward = rightForwardVectorCopy.forward;
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
    }
}
