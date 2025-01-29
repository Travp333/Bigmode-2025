using Scripting;
using Scripting.ScriptableObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

public class specialStoreManager : MonoBehaviour
{
    [SerializeField]
    List<UpgradeButton> upgradeButtons = new List<UpgradeButton>();
    private static specialStoreManager _singleton;

    public static specialStoreManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

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
            upgradeButton.unpress();
        }
    }
}
