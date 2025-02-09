using UnityEngine;

namespace Scripting
{
    public class BatHitboxCollision : MonoBehaviour
    {
        [SerializeField]
        public BatHitboxController cont;
        [SerializeField] private AudioSource myBatHitSound;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Structure"))
            {
                return;
            }
            else
            {
                myBatHitSound.Play();
            }
        }
    }
}
