using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreProductUI : MonoBehaviour
{
    [Header("Product")]
    [SerializeField] private string productId;
    [SerializeField] private string displayName;

    [SerializeField, Min(1)]
    private int amountPerPurchase = 1;

    [SerializeField, Min(0)]
    private int price = 1;

    [Tooltip("Set to 0 for no ownership limit.")]
    [SerializeField, Min(0)]
    private int maximumOwned;

    [SerializeField, Min(1)]
    private int maximumSelectedQuantity = 10;

    [SerializeField] private Sprite productSprite;

    [Header("UI")]
    [SerializeField] private Image productImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text ownedText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text totalText;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button buyButton;

    [Header("Sold Out")]
    [SerializeField] private GameObject soldOutOverlay;
    [SerializeField] private TMP_Text soldOutText;

    [Header("Store")]
    [SerializeField] private ComputerSupplyStoreUI storeUI;

    private int selectedQuantity = 1;
    private int currentlyOwned;
    private int availableMoney;

    public string ProductId => productId;
    public string DisplayName => displayName;
    public int AmountPerPurchase => amountPerPurchase;
    public int Price => price;
    public int MaximumOwned => maximumOwned;
    public int SelectedQuantity => selectedQuantity;

    public int PurchaseAmount =>
        amountPerPurchase * selectedQuantity;

    public int TotalPrice =>
        price * selectedQuantity;

    public bool IsSoldOut =>
        maximumOwned > 0 &&
        currentlyOwned >= maximumOwned;

    private void Awake()
    {
        FindStore();
        ConnectQuantityButtons();
        RefreshDisplay();
    }

    private void OnEnable()
    {
        FindStore();
        RefreshDisplay();
    }

    private void OnDestroy()
    {
        DisconnectQuantityButtons();
    }

    private void FindStore()
    {
        if (storeUI != null)
        {
            return;
        }

        storeUI =
            GetComponentInParent<ComputerSupplyStoreUI>();

        if (storeUI == null)
        {
            storeUI =
                FindAnyObjectByType<ComputerSupplyStoreUI>();
        }
    }

    private void ConnectQuantityButtons()
    {
        if (minusButton != null)
        {
            minusButton.onClick.RemoveListener(
                DecreaseQuantity
            );

            minusButton.onClick.AddListener(
                DecreaseQuantity
            );
        }

        if (plusButton != null)
        {
            plusButton.onClick.RemoveListener(
                IncreaseQuantity
            );

            plusButton.onClick.AddListener(
                IncreaseQuantity
            );
        }
    }

    private void DisconnectQuantityButtons()
    {
        if (minusButton != null)
        {
            minusButton.onClick.RemoveListener(
                DecreaseQuantity
            );
        }

        if (plusButton != null)
        {
            plusButton.onClick.RemoveListener(
                IncreaseQuantity
            );
        }
    }

    public void SetStore(
        ComputerSupplyStoreUI newStoreUI
    )
    {
        storeUI = newStoreUI;
    }

    public void Refresh(
        int currentMoney,
        int ownedAmount
    )
    {
        availableMoney = Mathf.Max(0, currentMoney);
        currentlyOwned = Mathf.Max(0, ownedAmount);

        ClampSelectedQuantity();
        RefreshDisplay();
    }

    public void ResetQuantity()
    {
        selectedQuantity = 1;
        ClampSelectedQuantity();
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (nameText != null)
        {
            nameText.text = displayName;
        }

        if (priceText != null)
        {
            priceText.text =
                amountPerPurchase +
                " for $" +
                price;
        }

        if (ownedText != null)
        {
            ownedText.text =
                "Owned: " +
                currentlyOwned;
        }

        if (quantityText != null)
        {
            quantityText.text =
                selectedQuantity.ToString();
        }

        if (totalText != null)
        {
            totalText.text =
                "Total: $" +
                TotalPrice;
        }

        if (productImage != null)
        {
            productImage.sprite = productSprite;
            productImage.enabled =
                productSprite != null;

            productImage.preserveAspect = true;
        }

        if (soldOutText != null)
        {
            soldOutText.text = "SOLD OUT";
        }

        if (soldOutOverlay != null)
        {
            soldOutOverlay.SetActive(IsSoldOut);

            if (IsSoldOut)
            {
                soldOutOverlay.transform.SetAsLastSibling();
            }
        }

        RefreshButtonStates();
    }

    private void IncreaseQuantity()
    {
        FindStore();

        if (storeUI != null)
        {
            storeUI.PlayQuantityButtonSound();
        }

        if (!CanSelectQuantity(
            selectedQuantity + 1))
        {
            return;
        }

        selectedQuantity++;
        RefreshDisplay();
    }

    private void DecreaseQuantity()
    {
        FindStore();

        if (storeUI != null)
        {
            storeUI.PlayQuantityButtonSound();
        }

        if (selectedQuantity <= 1)
        {
            return;
        }

        selectedQuantity--;
        RefreshDisplay();
    }

    public void BuySelectedQuantity()
    {
        FindStore();

        if (storeUI == null)
        {
            Debug.LogError(
                "StoreProductUI could not find " +
                "ComputerSupplyStoreUI."
            );

            return;
        }

        storeUI.PlayBuyButtonSound();
        storeUI.TryPurchase(this);
    }

    private bool CanSelectQuantity(int quantity)
    {
        if (quantity < 1 ||
            quantity > maximumSelectedQuantity)
        {
            return false;
        }

        if (maximumOwned <= 0)
        {
            return true;
        }

        return currentlyOwned +
            amountPerPurchase * quantity <=
            maximumOwned;
    }

    private void ClampSelectedQuantity()
    {
        selectedQuantity = Mathf.Clamp(
            selectedQuantity,
            1,
            Mathf.Max(
                1,
                maximumSelectedQuantity
            )
        );

        while (
            selectedQuantity > 1 &&
            !CanSelectQuantity(selectedQuantity)
        )
        {
            selectedQuantity--;
        }
    }

    private void RefreshButtonStates()
    {
        if (minusButton != null)
        {
            minusButton.interactable =
                !IsSoldOut &&
                selectedQuantity > 1;
        }

        if (plusButton != null)
        {
            plusButton.interactable =
                !IsSoldOut &&
                CanSelectQuantity(
                    selectedQuantity + 1
                );
        }

        if (buyButton != null)
        {
            buyButton.interactable =
                !IsSoldOut &&
                !string.IsNullOrWhiteSpace(productId);
        }
    }
}