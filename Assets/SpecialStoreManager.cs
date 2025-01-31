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
    int tempNum;

    public static SpecialStoreManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(SpecialStoreManager)} instance already exists, destroying duplicate!");
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
        SetRandomUpgrade();
        upgradeButtons.correctMaterial();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void SetRandomUpgrade()
    {
        //8-13 inclusive
        UnityEngine.Random.InitState((int) DateTime.Now.Ticks);
        int temp = UnityEngine.Random.Range(8, 14); //(int minInclusive, int maxExclusive)     
        if(upgradeButtons.myUpgradeType != (Upgrades.UpgradeTypes) temp){
            upgradeButtons.myUpgradeType = (Upgrades.UpgradeTypes) temp;
            upgradeButtons.unpress();
        }   
        else{
            SetRandomUpgrade();
        }

    }

    public Material fetchUpgradeMaterial(int x)
    {
        return materials[x - materials.Count-1];
    }

    public Material fetchUpgradeMaterial(Upgrades.UpgradeTypes x)
    {
        return fetchUpgradeMaterial((int) x);
    }
}
