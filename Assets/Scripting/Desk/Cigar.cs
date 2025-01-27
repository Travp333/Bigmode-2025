using System.Collections;
using Scripting.Player;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

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

        private Transform _originalParent;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private bool _isSmoking;
private DeskArms _deskArms;
        
        
        void Awake()
        {
            _leftArmAnim = leftArm.GetComponent<Animator>();
            _originalParent = transform.parent;
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            _deskArms = arms.GetComponent<DeskArms>();
        }

        void Update()
        {
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
                            _deskArms.Block();
                        }
                    }
                }
            }

            if (_isSmoking && Input.GetMouseButtonDown(1))
            {
                
                _deskArms.Unblock();
                _leftArmAnim.Play("Dropping Cigar");
                Invoke(nameof(StopSmoking), .33f);
            }
        }

        public void StartSmoking()
        {
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
            player.NotifyStoppedSmoking();
            _isSmoking = false;
            transform.parent = _originalParent;
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
        }
    }
}
