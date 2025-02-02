using Scripting.Player;
using UnityEngine;

namespace Scripting.Desk
{
    public class Cigar : MonoBehaviour
    {
        [SerializeField]
        private GameObject arms;

        [SerializeField]
        private GameObject leftArm;

        private Animator _leftArmAnim;
        [SerializeField] private Movement player;
        [SerializeField] private GameObject attachPoint;
        [SerializeField] private AudioSource myCigarSound;

        private Transform _originalParent;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private bool _isSmoking;
        private DeskArms _deskArms;

        private void Awake()
        {
            _leftArmAnim = leftArm.GetComponent<Animator>();
            _originalParent = transform.parent;
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            _deskArms = arms.GetComponent<DeskArms>();
        }

        private void Update()
        {
            if (_isSmoking && GameManager.Singleton.IsNightTime)
            {
                RunStopSmoking();
            }

            if (_isSmoking && Input.GetMouseButtonDown(0))
            {
                RunStopSmoking();
            }

            if (!_isSmoking && player.CanAct())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out var hit))
                    {
                        if (hit.collider.gameObject == gameObject)
                        {
                            _leftArmAnim.Play("Grabbing Cigar");
                            Invoke(nameof(StartSmoking), .33f);
                            _deskArms.BlockLeftHand();
                        }
                    }
                }
            }
        }

        private void RunStopSmoking()
        {
            _deskArms.UnblockLeftHand();
            _leftArmAnim.Play("Dropping Cigar");
            Invoke(nameof(StopSmoking), .33f);
        }

        private bool _smokingTutorial; //...lol hey kids

        public void StartSmoking()
        {
            if (!_smokingTutorial)
            {
                _smokingTutorial = true;
                TutorialManager.Singleton.HideOrderNumber(6);
            }

            myCigarSound.Play();
            player.NotifyIsSmoking();
            _isSmoking = true;
            transform.parent = attachPoint.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            //leftArmAnim.Play("Smoking Cigar");
            //ANIMATIONS START
        }

        public void StopSmoking()
        {
            myCigarSound.Stop();
            player.NotifyStoppedSmoking();
            _isSmoking = false;
            transform.parent = _originalParent;
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
        }
    }
}
