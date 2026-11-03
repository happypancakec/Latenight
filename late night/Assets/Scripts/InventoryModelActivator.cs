using System;
using UnityEngine;

public class InventoryModelActivator : MonoBehaviour
{
    [Serializable]
    public class InventoryDisplay
    {
        [Tooltip(
            "Must match the ID used by SupplyManager."
        )]
        public string supplyId;

        [Min(1)]
        public int minimumAmount = 1;

        [Tooltip(
            "These objects become visible when the " +
            "player owns the required amount."
        )]
        public GameObject[] objectsToActivate;
    }

    [Header("Inventory Displays")]
    [SerializeField]
    private InventoryDisplay[] displays;

    private SupplyManager supplyManager;

    private void Start()
    {
        FindSupplyManager();

        if (supplyManager != null)
        {
            supplyManager.InventoryChanged -=
                RefreshModels;

            supplyManager.InventoryChanged +=
                RefreshModels;
        }

        RefreshModels();
    }

    private void OnEnable()
    {
        FindSupplyManager();

        if (supplyManager != null)
        {
            supplyManager.InventoryChanged -=
                RefreshModels;

            supplyManager.InventoryChanged +=
                RefreshModels;
        }

        RefreshModels();
    }

    private void OnDisable()
    {
        if (supplyManager != null)
        {
            supplyManager.InventoryChanged -=
                RefreshModels;
        }
    }

    private void FindSupplyManager()
    {
        if (supplyManager != null)
        {
            return;
        }

        supplyManager = SupplyManager.Instance;

        if (supplyManager == null)
        {
            supplyManager =
                FindAnyObjectByType<SupplyManager>();
        }
    }

    public void RefreshModels()
    {
        if (supplyManager == null)
        {
            FindSupplyManager();
        }

        if (supplyManager == null ||
            displays == null)
        {
            return;
        }

        foreach (
            InventoryDisplay display
            in displays
        )
        {
            if (display == null ||
                string.IsNullOrWhiteSpace(
                    display.supplyId
                ))
            {
                continue;
            }

            int ownedAmount =
                supplyManager.GetAmount(
                    display.supplyId
                );

            bool shouldBeVisible =
                ownedAmount >=
                Mathf.Max(
                    1,
                    display.minimumAmount
                );

            if (display.objectsToActivate == null)
            {
                continue;
            }

            foreach (
                GameObject model
                in display.objectsToActivate
            )
            {
                if (model != null)
                {
                    model.SetActive(
                        shouldBeVisible
                    );
                }
            }
        }
    }
}