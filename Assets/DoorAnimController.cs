using UnityEngine;

public class DoorAnimController : MonoBehaviour
{
    public bool isOpen;
    private Animator _anim;
    [SerializeField] private float doorOpenTimer = 2f;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void OpenDoorFromFront()
    {
        _anim.Play("OPEN");
        _anim.SetBool("open", true);
        isOpen = true;
        Invoke(nameof(CloseDoor), doorOpenTimer);
    }

    public void OpenDoorFromBack()
    {
        _anim.Play("OpenAlt");
        _anim.SetBool("open", true);
        isOpen = true;
        Invoke(nameof(CloseDoor), doorOpenTimer);
    }

    public void SlamOpenDoor()
    {
        _anim.Play("Fly Open");
        _anim.SetBool("open", true);
        isOpen = true;
        Invoke(nameof(CloseDoor), doorOpenTimer);
    }

    public void CloseDoor()
    {
        _anim.SetBool("open", false);
        isOpen = false;
    }
}
