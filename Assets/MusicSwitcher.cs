using System.Collections;
using UnityEngine;

public class MusicSwitcher : MonoBehaviour
{
    public static MusicSwitcher Instance;
    [SerializeField] AudioSource dayMan;
    [SerializeField] AudioSource nightMan;
    [SerializeField] float transitionTime = 2.0f;
    bool switchingToDay = false;
    bool switchingToNight = false;
    float progress = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (switchingToDay)
        {
            if (progress > transitionTime)
            {
                nightMan.Stop();// = 0.0f;
                dayMan.volume = 1.0f;
                switchingToDay = false;
            }
            else
            {
                nightMan.volume = Mathf.Clamp01(Mathf.Lerp(0.0f, 1.0f, (transitionTime - progress) / transitionTime));
                dayMan.volume = Mathf.Clamp01(Mathf.Lerp(0.0f, 1.0f, progress / transitionTime));
            }
        }
        else if (switchingToNight)
        {
            if (progress > transitionTime)
            {
                dayMan.Stop();// = 0.0f;
                nightMan.volume = 1.0f;
                switchingToNight = false;
            }
            else
            {
                dayMan.volume = Mathf.Clamp01(Mathf.Lerp(0.0f, 1.0f, (transitionTime - progress) / transitionTime));
                nightMan.volume = Mathf.Clamp01(Mathf.Lerp(0.0f, 1.0f, progress / transitionTime));
            }
        }
        progress += Time.deltaTime;
    }

    public void DayTime()
    {
        switchingToDay = true;
        progress = 0;
        dayMan.volume = 0.0f;
        dayMan.Play();
    }

    public void NightTime()
    {
        switchingToNight = true;
        progress = 0;
        nightMan.volume = 0.0f;
        nightMan.Play();
    }
}
