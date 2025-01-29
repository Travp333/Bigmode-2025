using Scripting.ScriptableObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

public class specialStoreManager : MonoBehaviour
{
    [SerializeField]
    List<UpgradeButton> upgradeButtons = new List<UpgradeButton>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        newDay();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void newDay()
    {
        //8-13 inclusive
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        int temp = UnityEngine.Random.Range(8, 14); //(int minInclusive, int maxExclusive)
        foreach (UpgradeButton upgradeButton in upgradeButtons)
        {
            upgradeButton.myUpgradeType = (Upgrades.UpgradeTypes)temp;
        }
    }
}
