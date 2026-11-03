using System.Text;
using TMPro;
using UnityEngine;

public class SupplyInventoryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text suppliesText;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private bool startVisible = false;

    private SupplyManager supplyManager;
    private bool isVisible;

    private void Start()
    {
        supplyManager = SupplyManager.Instance;

        if (supplyManager == null)
        {
            supplyManager =
                FindAnyObjectByType<SupplyManager>();
        }

        if (supplyManager == null)
        {
            Debug.LogError(
                "SupplyInventoryUI could not find SupplyManager."
            );

            return;
        }

        supplyManager.InventoryChanged += Refresh;

        isVisible = startVisible;
        SetPanelVisible(isVisible);
        Refresh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            SetPanelVisible(isVisible);

            if (isVisible)
            {
                Refresh();
            }
        }
    }

    private void OnDestroy()
    {
        if (supplyManager != null)
        {
            supplyManager.InventoryChanged -= Refresh;
        }
    }

    private void SetPanelVisible(bool visible)
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(visible);
        }
    }

    private void Refresh()
    {
        if (supplyManager == null)
        {
            return;
        }

        if (moneyText != null)
        {
            moneyText.text =
                $"Money: ${supplyManager.Money}";
        }

        if (suppliesText == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();

        foreach (SupplyManager.SupplyEntry supply
                 in supplyManager.Supplies)
        {
            builder.AppendLine(
                $"{supply.displayName}: {supply.amount}"
            );
        }

        suppliesText.text = builder.ToString();
    }
}