using Scripting;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (GameManager.Singleton.IsNightTime)
        {
            anim.SetBool("ON", false);
        }
    }

    public void Pressed()
    {
        if (GameManager.Singleton.IsNightTime)
        {
            if(anim.GetBool("ON") == false){
                anim.Play("turnon");
                anim.SetBool("ON", true);
                GameManager.Singleton.StartDay();
            }
        }
    }

    public void ShowFlavorText()
    {

    }

    public string FetchFlavorText()
    {
        return "it's a lightswitch";
    }
}