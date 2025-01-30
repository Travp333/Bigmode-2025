using UnityEngine;

public class PlayFootstep : MonoBehaviour
{
    [SerializeField] AudioSource footstep;
    [SerializeField] bool randomized;
    [SerializeField] float footstepFrequency = 1.0f;
    float nextFootstep = 0.0f;
    LaunchDetection myLaunchDetection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (randomized)
        {
            nextFootstep = footstepFrequency;
            myLaunchDetection = GetComponent<LaunchDetection>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(randomized && myLaunchDetection.IsWalking())
        {
            nextFootstep -= Time.deltaTime;
            if(nextFootstep < 0.0f)
            {
                DoIt();
                nextFootstep += footstepFrequency;
            }
        }
    }

    public void DoIt()
    {
        footstep.Play();
    }
}
