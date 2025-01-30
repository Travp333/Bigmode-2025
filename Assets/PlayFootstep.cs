using UnityEngine;

public class PlayFootstep : MonoBehaviour
{
    [SerializeField] AudioSource footstep;
    [SerializeField] bool randomized;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void DoIt()
    {
        footstep.Play();
    }
}
