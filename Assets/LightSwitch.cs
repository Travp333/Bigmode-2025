using Scripting;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void Pressed()
    {
        if (GameManager.Singleton.IsNightTime)
        {
            GameManager.Singleton.StartDay();
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