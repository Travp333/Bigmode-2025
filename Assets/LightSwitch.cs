using Scripting;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    BoxCollider box;
    Animator anim;
    [SerializeField] AudioSource mySwitchSound;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        box = GetComponent<BoxCollider>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (GameManager.Singleton.IsNightTime)
        {
            anim.SetBool("ON", false);
            box.enabled = true;
        }
    }

    public void Pressed()
    {
        if (GameManager.Singleton.IsNightTime)
        {
            mySwitchSound.Play();
            TutorialManager.Singleton.HideOrderNumber(0);
            TutorialManager.Singleton.HideOrderNumber(8);
            if(anim.GetBool("ON") == false){
                anim.Play("turnon");
                anim.SetBool("ON", true);
                GameManager.Singleton.StartDay();
                box.enabled = false;
            }
        }
    }

    public void ShowFlavorText()
    {

    }

    public string FetchFlavorText()
    {
        return "Open up your shop and start the next day!";
    }
}