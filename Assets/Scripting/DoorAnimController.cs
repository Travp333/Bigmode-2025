using UnityEngine;

namespace Scripting
{
    public class DoorAnimController : MonoBehaviour
    {
        public bool isOpen;
        private Animator _anim;
        [SerializeField] private float doorOpenTimer = 2f;
        AudioSource doorNoise;

        private void Awake()
        {
            doorNoise = GetComponent<AudioSource>();
            _anim = GetComponent<Animator>();
        }

        public void OpenDoorFromFront()
        {
            _anim.Play("OPEN");
            _anim.SetBool("open", true);
            isOpen = true;
            Invoke(nameof(CloseDoor), doorOpenTimer);
            doorNoise.Play();
        }

        public void OpenDoorFromBack()
        {
            _anim.Play("OpenAlt");
            _anim.SetBool("open", true);
            isOpen = true;
            Invoke(nameof(CloseDoor), doorOpenTimer);
            doorNoise.Play();
        }

        public void SlamOpenDoor()
        {
            _anim.Play("Fly Open");
            _anim.SetBool("open", true);
            isOpen = true;
            Invoke(nameof(CloseDoor), doorOpenTimer);
            doorNoise.Play();
        }

        public void CloseDoor()
        {
            _anim.SetBool("open", false);
            isOpen = false;
        }
    }
}
