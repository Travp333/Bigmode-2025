using Scripting;
using UnityEngine;

public class ShowOnlyOnce : MonoBehaviour
{
    void Awake()
    {
        if (!GameManager.Singleton.upgrades.powerTutorialDone)
        {
            gameObject.SetActive(true);
            
            GameManager.Singleton.upgrades.powerTutorialDone = true;
        }
        else
        {
            Hide();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
