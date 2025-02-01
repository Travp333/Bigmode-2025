using UnityEngine;

public class MainMenuFadeIn : MonoBehaviour
{
    [SerializeField] AudioSource targetSound;
    [SerializeField] float fadeInTime = 2.0f;
    float progress = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        targetSound.volume = Mathf.Clamp01(progress / fadeInTime);
        progress += Time.deltaTime;
    }
}
