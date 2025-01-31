using UnityEngine;

namespace Scripting.Objects
{
    public class BaseballBat : MonoBehaviour
    {
        [SerializeField] private Animator handAnim;
        [SerializeField] private GameObject handBat;
        [SerializeField] private CapsuleCollider capsuleCollider;
        [SerializeField] private MeshRenderer meMesh;
        [SerializeField] private AudioSource myPickupBatSound;

        private void OnValidate()
        {
            if (!capsuleCollider) capsuleCollider = GetComponent<CapsuleCollider>();
        }

        public void PickUp()
        {
            myPickupBatSound.Play();
            handAnim.Play("PickupBat");
            handAnim.SetBool("HoldingBat", true);
            
            handBat.SetActive(true);
            this.GetComponent<CapsuleCollider>().enabled = false;
            meMesh.enabled = false;
           // transform.parent = weaponPosition;
          //  transform.localPosition = Vector3.zero;
           // transform.localRotation = Quaternion.identity;
        }

        public void Drop()
        {
            this.GetComponent<CapsuleCollider>().enabled = true;
            meMesh.enabled = true;
            handAnim.SetBool("HoldingBat", false);
            handAnim.Play("IDLE");
            handBat.SetActive(false);
          //  transform.parent = null;
          //  transform.position = new Vector3(5.796f, 0.489f, -5.296f);
          //  transform.rotation = Quaternion.Euler(0.0f, 0.0f, -16.817f);
        }
    }
}
