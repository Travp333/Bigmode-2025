using UnityEngine;

public class SprayPaintSounds : MonoBehaviour
{
    [SerializeField] AudioSource spraySound;
    [SerializeField] AudioSource rattleSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartSpraySound()
    {
        spraySound.Play();
    }

    public void StopSpraySound()
    {
        spraySound.Stop();
    }

    public void StartRattleSound()
    {
        rattleSound.Play();
    }

    public void StopRattleSound()
    {
        rattleSound.Stop();
    }
}
