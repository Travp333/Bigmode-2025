using Scripting;
using UnityEngine;

public class lightSwitch : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void pressed()
    {
        if (GameManager.Singleton._dayOver)
        {
            GameManager.Singleton.NextDay();
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