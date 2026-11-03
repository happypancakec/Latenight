using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class ComputerSupplyStoreUI : MonoBehaviour
{
    [Header("Store UI")]
    [SerializeField] private TMP_Text fundsText;
    [SerializeField] private TMP_Text inventoryText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject notificationPanel;

    [Header("Notification")]
    [SerializeField, Min(0.5f)]
    private float notificationDuration = 3f;

    [Header("Products")]
    [SerializeField] private StoreProductUI[] productRows;

    [Header("Store Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip quantityButtonSound;
    [SerializeField] private AudioClip buyButtonSound;
    [SerializeField] private AudioClip purchaseSuccessfulSound;
    [SerializeField] private AudioClip insufficientFundsSound;
    [SerializeField] private AudioClip soldOutSound;

    [SerializeField, Range(0f, 1f)]
    private float soundVolume = 0.8f;

    private SupplyManager supplyManager;
    private Coroutine notificationRoutine;

    private void Awake()
    {
        FindProductRows();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        FindSupplyManager();
        FindProductRows();

        if (supplyManager != null)
        {
            supplyManager.InventoryChanged -=
                RefreshDisplay;

            supplyManager.InventoryChanged +=
                RefreshDisplay;
        }

        ClearStatus();
        RefreshDisplay();
    }

    private void OnDisable()
    {
        if (supplyManager != null)
        {
            supplyManager.InventoryChanged -=
                RefreshDisplay;
        }

        if (notificationRoutine != null)
        {
            StopCoroutine(notificationRoutine);
            notificationRoutine = null;
        }
    }

    private void FindSupplyManager()
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
                "The computer store could not find " +
                "SupplyManager in TrailerParkGame."
            );
        }
    }

    private void FindProductRows()
    {
        productRows =
            GetComponentsInChildren<StoreProductUI>(
                true
            );

        foreach (StoreProductUI product in productRows)
        {
            if (product != null)
            {
                product.SetStore(this);
            }
        }
    }

    public void TryPurchase(StoreProductUI product)
    {
        if (product == null)
        {
            return;
        }

        if (supplyManager == null)
        {
            FindSupplyManager();
        }

        if (supplyManager == null)
        {
            ShowNotification(
                "The store cannot access your inventory.",
                false
            );

            PlaySound(insufficientFundsSound);
            return;
        }

        int currentlyOwned =
            supplyManager.GetAmount(
                product.ProductId
            );

        if (product.MaximumOwned > 0 &&
            currentlyOwned >= product.MaximumOwned)
        {
            ShowNotification(
                product.DisplayName +
                " is sold out.",
                false
            );

            PlaySound(soldOutSound);
            RefreshDisplay();
            return;
        }

        if (product.MaximumOwned > 0 &&
            currentlyOwned +
            product.PurchaseAmount >
            product.MaximumOwned)
        {
            ShowNotification(
                "You cannot own that many " +
                product.DisplayName + ".",
                false
            );

            PlaySound(soldOutSound);
            RefreshDisplay();
            return;
        }

        if (supplyManager.Money <
            product.TotalPrice)
        {
            int missingMoney =
                product.TotalPrice -
                supplyManager.Money;

            ShowNotification(
                "Not enough money! You need $" +
                missingMoney + " more.",
                false
            );

            PlaySound(insufficientFundsSound);
            return;
        }

        bool purchased =
            supplyManager.TryBuySupply(
                product.ProductId,
                product.PurchaseAmount,
                product.TotalPrice
            );

        if (purchased)
        {
            ShowNotification(
                "Purchased " +
                product.PurchaseAmount + " " +
                product.DisplayName +
                " for $" +
                product.TotalPrice + "!",
                true
            );

            PlaySound(purchaseSuccessfulSound);
            product.ResetQuantity();
        }
        else
        {
            ShowNotification(
                "The purchase could not be completed.",
                false
            );

            PlaySound(insufficientFundsSound);
        }

        RefreshDisplay();
    }

    public void TryBuyProduct(StoreProductUI product)
    {
        TryPurchase(product);
    }

    public void PlayQuantityButtonSound()
    {
        PlaySound(quantityButtonSound);
    }

    public void PlayBuyButtonSound()
    {
        PlaySound(buyButtonSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null ||
            clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            clip,
            soundVolume
        );
    }

    public void RefreshDisplay()
    {
        if (supplyManager == null)
        {
            FindSupplyManager();
        }

        if (supplyManager == null)
        {
            return;
        }

        if (fundsText != null)
        {
            fundsText.text =
                "Available Funds: $" +
                supplyManager.Money;
        }

        if (inventoryText != null)
        {
            StringBuilder inventory =
                new StringBuilder();

            inventory.AppendLine(
                "CURRENT INVENTORY"
            );

            foreach (
                SupplyManager.SupplyEntry item
                in supplyManager.Supplies
            )
            {
                inventory.AppendLine(
                    item.displayName +
                    ": " +
                    item.amount
                );
            }

            inventoryText.text =
                inventory.ToString();
        }

        FindProductRows();

        foreach (StoreProductUI product in productRows)
        {
            if (product == null)
            {
                continue;
            }

            product.Refresh(
                supplyManager.Money,
                supplyManager.GetAmount(
                    product.ProductId
                )
            );
        }
    }

    private void ShowNotification(
        string message,
        bool successful
    )
    {
        if (statusText != null)
        {
            statusText.text = message;

            statusText.color = successful
                ? new Color32(40, 160, 80, 255)
                : new Color32(220, 45, 45, 255);
        }

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
            notificationPanel.transform.SetAsLastSibling();
        }

        if (notificationRoutine != null)
        {
            StopCoroutine(notificationRoutine);
        }

        notificationRoutine =
            StartCoroutine(
                HideNotificationAfterDelay()
            );
    }

    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSecondsRealtime(
            notificationDuration
        );

        ClearStatus();
        notificationRoutine = null;
    }

    private void ClearStatus()
    {
        if (statusText != null)
        {
            statusText.text = "";
        }

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
}