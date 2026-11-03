using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class StaminaBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStamina playerStamina;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Visibility")]
    [SerializeField] private float fadeSpeed = 5f;

    [Tooltip("How long the bar remains visible after reaching full stamina.")]
    [SerializeField] private float hideDelay = 0.75f;

    [Header("Colors")]
    [SerializeField] private Color highColor = Color.green;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;

    private float fullStaminaTimer;
    private bool hasUsedStamina;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = 1f;
        }

        // Start hidden because stamina begins full.
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (
            playerStamina == null ||
            staminaSlider == null
        )
        {
            return;
        }

        float maximumStamina =
            Mathf.Max(1f, playerStamina.MaximumStamina);

        float staminaPercentage = Mathf.Clamp01(
            playerStamina.CurrentStamina / maximumStamina
        );

        staminaSlider.value = staminaPercentage;

        UpdateColor(staminaPercentage);
        UpdateVisibility(staminaPercentage);
    }

    private void UpdateColor(float percentage)
    {
        if (fillImage == null)
        {
            return;
        }

        if (percentage > 0.6f)
        {
            fillImage.color = highColor;
        }
        else if (percentage > 0.3f)
        {
            fillImage.color = mediumColor;
        }
        else
        {
            fillImage.color = lowColor;
        }
    }

    private void UpdateVisibility(float percentage)
    {
        if (canvasGroup == null)
        {
            return;
        }

        bool staminaIsBelowFull = percentage < 0.999f;

        if (staminaIsBelowFull)
        {
            hasUsedStamina = true;
            fullStaminaTimer = 0f;
        }
        else if (hasUsedStamina)
        {
            fullStaminaTimer += Time.unscaledDeltaTime;
        }

        bool shouldBeVisible =
            staminaIsBelowFull ||
            (
                hasUsedStamina &&
                fullStaminaTimer < hideDelay
            );

        float targetAlpha = shouldBeVisible ? 1f : 0f;

        canvasGroup.alpha = Mathf.MoveTowards(
            canvasGroup.alpha,
            targetAlpha,
            fadeSpeed * Time.unscaledDeltaTime
        );

        if (
            !staminaIsBelowFull &&
            fullStaminaTimer >= hideDelay &&
            canvasGroup.alpha <= 0f
        )
        {
            hasUsedStamina = false;
        }
    }
}