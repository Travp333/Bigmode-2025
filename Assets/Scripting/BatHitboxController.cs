using UnityEngine;

namespace Scripting
{
    public class BatHitboxController : MonoBehaviour
    {
        [SerializeField] private BoxCollider hitBox;
        //[SerializeField] private AudioSource myBatHitSound;

        public void EnableHitbox(){
            hitBox.enabled = true;
        }
        public void DisableHitbox()
        {
            //myBatHitSound.Play();
            hitBox.enabled = false;
        }
    }
}
