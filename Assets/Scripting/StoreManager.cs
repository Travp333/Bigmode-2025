using NUnit.Framework;
using Scripting.ScriptableObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    [SerializeField]
    GameObject upgradeButtonPrefab;
    List<Upgrades.UpgradeTypes> limitedTimeUpgrades = new List<Upgrades.UpgradeTypes>();
    List<Upgrades.UpgradeTypes> standardUpgrades = new List<Upgrades.UpgradeTypes>();

    private void Awake()
    {
        standardUpgrades.Add(Upgrades.UpgradeTypes.chairs);
        standardUpgrades.Add(Upgrades.UpgradeTypes.paintings);
        standardUpgrades.Add(Upgrades.UpgradeTypes.baseballBat);
        standardUpgrades.Add(Upgrades.UpgradeTypes.cigar);
        standardUpgrades.Add(Upgrades.UpgradeTypes.phone);
        //standardUpgrades.Add(Upgrades.UpgradeTypes.bodyguard);
        standardUpgrades.Add(Upgrades.UpgradeTypes.assistant);

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
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //on certain conditions add limited time upgrades to store
    }
}
