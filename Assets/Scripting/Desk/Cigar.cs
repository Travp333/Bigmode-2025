using Scripting.Player;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace Scripting.Desk
{
    public class Cigar : MonoBehaviour
    {
        [SerializeField]
        GameObject Arms;
        [SerializeField]
        GameObject LeftArm;
        Animator leftArmAnim;
        [SerializeField] private Movement player;
        [SerializeField] private GameObject attachPoint;

        private Transform _originalParent;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        void Awake()
        {
            leftArmAnim = LeftArm.GetComponent<Animator>();
            _originalParent = transform.parent;
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
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
                            ShowArms();
                            leftArmAnim.Play("Grabbing Cigar");
                            Invoke("StartSmoking", .33f);
                            //StartSmoking();
                        }
                    }
                }
            }

            if (_isSmoking && Input.GetMouseButtonDown(1))
            {
                leftArmAnim.Play("Dropping Cigar");
                Invoke("StopSmoking", .33f);
                Invoke("HideArms", 1f);
                //StopSmoking();
            }
        }
        
        private bool _isSmoking;
        void ShowArms(){
            Arms.SetActive(true);
        }
        void HideArms(){
            Arms.SetActive(false);
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
