using Scripting;
using Scripting.ScriptableObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SpecialStoreManager : MonoBehaviour
{
    [SerializeField] private UpgradeButton upgradeButtons; // = new List<UpgradeButton>();
    [SerializeField] private List<Material> materials = new();
    private static SpecialStoreManager _singleton;

    public static SpecialStoreManager Singleton
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
    private void Start()
    {
        upgradeButtons.special = true;
        newDay();
        upgradeButtons.correctMaterial();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void newDay()
    {
        //8-13 inclusive
        UnityEngine.Random.InitState((int) DateTime.Now.Ticks);
        int temp = UnityEngine.Random.Range(8, 14); //(int minInclusive, int maxExclusive)        
        upgradeButtons.myUpgradeType = (Upgrades.UpgradeTypes) temp;
        upgradeButtons.unpress();
    }

    public Material fetchUpgradeMaterial(int x)
    {
        return materials[(int) (x - 8)];
    }

    public Material fetchUpgradeMaterial(Upgrades.UpgradeTypes x)
    {
        return fetchUpgradeMaterial((int) (x));
    }
}
