using System;
using System.Collections.Generic;
using UnityEngine;

public class SupplyManager : MonoBehaviour
{
    [Serializable]
    public class SupplyEntry
    {
        public string id;
        public string displayName;
        public int amount;
    }

    public static SupplyManager Instance { get; private set; }

    [Header("Starting Values")]
    [SerializeField] private int startingMoney = 50;

    [Header("Current Inventory")]
    [SerializeField] private int money;

    [SerializeField]
    private List<SupplyEntry> supplies =
        new List<SupplyEntry>();

    public int Money => money;

    public IReadOnlyList<SupplyEntry> Supplies =>
        supplies;

    public event Action InventoryChanged;

    private const string MoneyKey =
        "PlayerMoney";

    private const string SupplyPrefix =
        "Supply_";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadData();
    }

    private void LoadData()
    {
        money = PlayerPrefs.GetInt(
            MoneyKey,
            startingMoney
        );

        EnsureSupplyExists(
            "wood",
            "Wood"
        );

        EnsureSupplyExists(
            "nails",
            "Nails"
        );

        EnsureSupplyExists(
            "paint",
            "Paint"
        );

        EnsureSupplyExists(
            "battery",
            "Flashlight Batteries"
        );

        EnsureSupplyExists(
            "rifle_ammo",
            "Rifle Ammunition"
        );

        EnsureSupplyExists(
            "hunting_rifle",
            "Hunting Rifle"
        );

        foreach (SupplyEntry supply in supplies)
        {
            supply.amount = PlayerPrefs.GetInt(
                SupplyPrefix + supply.id,
                supply.amount
            );
        }

        InventoryChanged?.Invoke();
    }

    private void EnsureSupplyExists(
        string id,
        string displayName
    )
    {
        SupplyEntry existingSupply =
            FindSupply(id);

        if (existingSupply != null)
        {
            existingSupply.displayName =
                displayName;

            return;
        }

        supplies.Add(
            new SupplyEntry
            {
                id = id,
                displayName = displayName,
                amount = 0
            }
        );
    }

    public int GetAmount(string supplyId)
    {
        SupplyEntry supply =
            FindSupply(supplyId);

        return supply != null
            ? supply.amount
            : 0;
    }

    public bool HasSupplies(
        string supplyId,
        int amount
    )
    {
        return GetAmount(supplyId) >= amount;
    }

    public void AddSupply(
        string supplyId,
        int amount
    )
    {
        if (amount <= 0)
        {
            return;
        }

        SupplyEntry supply =
            FindSupply(supplyId);

        if (supply == null)
        {
            supply = new SupplyEntry
            {
                id = supplyId,
                displayName = supplyId,
                amount = 0
            };

            supplies.Add(supply);
        }

        supply.amount += amount;

        SaveData();
        InventoryChanged?.Invoke();

        Debug.Log(
            $"Added {amount} {supply.displayName}. " +
            $"New total: {supply.amount}"
        );
    }

    public bool TryUseSupply(
        string supplyId,
        int amount
    )
    {
        if (amount <= 0)
        {
            return false;
        }

        SupplyEntry supply =
            FindSupply(supplyId);

        if (supply == null ||
            supply.amount < amount)
        {
            return false;
        }

        supply.amount -= amount;

        SaveData();
        InventoryChanged?.Invoke();

        Debug.Log(
            $"Used {amount} {supply.displayName}. " +
            $"Remaining: {supply.amount}"
        );

        return true;
    }

    public bool TryBuySupply(
        string supplyId,
        int amount,
        int totalPrice
    )
    {
        if (amount <= 0 || totalPrice < 0)
        {
            return false;
        }

        if (money < totalPrice)
        {
            return false;
        }

        money -= totalPrice;

        SupplyEntry supply =
            FindSupply(supplyId);

        if (supply == null)
        {
            Debug.LogError(
                "Unknown store item ID: " +
                supplyId
            );

            return false;
        }

        supply.amount += amount;

        SaveData();
        InventoryChanged?.Invoke();

        Debug.Log(
            $"Purchased {amount} " +
            $"{supply.displayName} for " +
            $"${totalPrice}."
        );

        return true;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        money += amount;

        SaveData();
        InventoryChanged?.Invoke();

        Debug.Log(
            $"Earned ${amount}. " +
            $"Current money: ${money}"
        );
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount < 0 || money < amount)
        {
            return false;
        }

        money -= amount;

        SaveData();
        InventoryChanged?.Invoke();

        return true;
    }

    private SupplyEntry FindSupply(string id)
    {
        return supplies.Find(
            supply => supply.id == id
        );
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(
            MoneyKey,
            money
        );

        foreach (SupplyEntry supply in supplies)
        {
            PlayerPrefs.SetInt(
                SupplyPrefix + supply.id,
                supply.amount
            );
        }

        PlayerPrefs.Save();
    }

    [ContextMenu("Reset Inventory and Money")]
    public void ResetInventoryAndMoney()
    {
        PlayerPrefs.DeleteKey(MoneyKey);

        foreach (SupplyEntry supply in supplies)
        {
            PlayerPrefs.DeleteKey(
                SupplyPrefix + supply.id
            );

            supply.amount = 0;
        }

        money = startingMoney;

        PlayerPrefs.Save();
        InventoryChanged?.Invoke();

        Debug.Log(
            "Inventory reset. Money restored to $" +
            startingMoney + "."
        );
    }
}