using Scripting.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// /////////////////////////////////////////DEPRECATED/////////////////////////////////////////
/// </summary>

public class StoreManager : MonoBehaviour
{
    [SerializeField] private GameObject upgradeButtonPrefab;
    private List<Upgrades.UpgradeTypes> limitedTimeUpgrades = new List<Upgrades.UpgradeTypes>();
    private List<Upgrades.UpgradeTypes> standardUpgrades = new List<Upgrades.UpgradeTypes>();

    private void Awake()
    {
        standardUpgrades.Add(Upgrades.UpgradeTypes.Chairs);
        standardUpgrades.Add(Upgrades.UpgradeTypes.Paintings);
        standardUpgrades.Add(Upgrades.UpgradeTypes.BaseballBat);
        standardUpgrades.Add(Upgrades.UpgradeTypes.Cigar);
        standardUpgrades.Add(Upgrades.UpgradeTypes.Phone);
        //standardUpgrades.Add(Upgrades.UpgradeTypes.bodyguard);
        standardUpgrades.Add(Upgrades.UpgradeTypes.Assistant);

        int i = 0;
        foreach (var newStoreOption in standardUpgrades)
        {
            var temp = GameObject.Instantiate(upgradeButtonPrefab, transform);
            temp.transform.localPosition = new Vector3(0.5f, 0.35f - (0.075f * i), 0.35f);
            temp.GetComponent<UpgradeButton>().myUpgradeType = standardUpgrades[i];
            i++;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        //on certain conditions add limited time upgrades to store
    }
}
