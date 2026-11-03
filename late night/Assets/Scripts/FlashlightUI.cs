using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlashlightUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FlashlightController flashlightController;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider chargeSlider;
    [SerializeField] private Image chargeFillImage;
    [SerializeField] private Image statusIcon;
    [SerializeField] private TMP_Text batteryText;
    [SerializeField] private TMP_Text statusText;

    [Header("Charge Colors")]
    [SerializeField]
    private Color highChargeColor =
        new Color(0.2f, 1f, 0.35f);

    [SerializeField]
    private Color mediumChargeColor =
        new Color(1f, 0.8f, 0.15f);

    [SerializeField]
    private Color lowChargeColor =
        new Color(1f, 0.2f, 0.15f);

    [Header("Status Colors")]
    [SerializeField]
    private Color flashlightOnColor =
        new Color(1f, 0.9f, 0.35f);

    [SerializeField]
    private Color flashlightOffColor =
        new Color(0.5f, 0.5f, 0.5f);

    [SerializeField]
    private Color flashlightEmptyColor =
        new Color(1f, 0.15f, 0.15f);

    private void Start()
    {
        if (chargeSlider != null)
        {
            chargeSlider.minValue = 0f;
            chargeSlider.maxValue = 1f;
        }

        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (flashlightController == null)
        {
            return;
        }

        bool hasFlashlight =
            flashlightController.HasFlashlight;

        SetUIVisible(hasFlashlight);

        if (!hasFlashlight)
        {
            return;
        }

        float maximumCharge =
            flashlightController.MaximumBatteryLife;

        float currentCharge =
            flashlightController.CurrentBatteryLife;

        float chargePercentage = 0f;

        if (maximumCharge > 0f)
        {
            chargePercentage =
                currentCharge / maximumCharge;
        }

        chargePercentage = Mathf.Clamp01(
            chargePercentage
        );

        if (chargeSlider != null)
        {
            chargeSlider.value = chargePercentage;
        }

        UpdateChargeColor(chargePercentage);
        UpdateBatteryCount();
        UpdateFlashlightStatus(currentCharge);
    }

    private void SetUIVisible(bool visible)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void UpdateChargeColor(float percentage)
    {
        if (chargeFillImage == null)
        {
            return;
        }

        if (percentage > 0.6f)
        {
            chargeFillImage.color = highChargeColor;
        }
        else if (percentage > 0.3f)
        {
            chargeFillImage.color = mediumChargeColor;
        }
        else
        {
            chargeFillImage.color = lowChargeColor;
        }
    }

    private void UpdateBatteryCount()
    {
        if (batteryText == null)
        {
            return;
        }

        batteryText.text =
            "Batteries: " +
            flashlightController.BatteryCount;
    }

    private void UpdateFlashlightStatus(float currentCharge)
    {
        string message;
        Color statusColor;

        if (currentCharge <= 0f)
        {
            message = "EMPTY";
            statusColor = flashlightEmptyColor;
        }
        else if (flashlightController.FlashlightIsOn)
        {
            message = "ON";
            statusColor = flashlightOnColor;
        }
        else
        {
            message = "OFF";
            statusColor = flashlightOffColor;
        }

        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = statusColor;
        }

        if (statusIcon != null)
        {
            statusIcon.color = statusColor;
        }
    }
}
