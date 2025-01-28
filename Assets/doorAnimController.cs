using UnityEngine;

public class doorAnimController : MonoBehaviour
{
    public bool isOpen;
    Animator anim;
    [SerializeField]
    float doorOpenTimer = 2f;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void OpenDoorFromFront(){
        anim.Play("OPEN");
        anim.SetBool("open", true);
        isOpen = true;
        Invoke("CloseDoor", doorOpenTimer);
    }
    public void OpenDoorFromBack(){
        anim.Play("OpenAlt");
        anim.SetBool("open", true);
        isOpen = true;
        Invoke("CloseDoor", doorOpenTimer);
    }
    public void SlamOpenDoor(){
        anim.Play("Fly Open");
        anim.SetBool("open", true);
        isOpen = true;
        Invoke("CloseDoor", doorOpenTimer);
    }
    public void CloseDoor(){
        anim.SetBool("open", false);
        isOpen = false;
    }


}
