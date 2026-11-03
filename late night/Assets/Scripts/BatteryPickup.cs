using UnityEngine;

public class BatteryPickup : MonoBehaviour, PickupInteractable
{
    [SerializeField] private FlashlightController flashlightController;
    [SerializeField] private int batteryAmount = 1;

    private bool hasBeenCollected;

    public void Interact()
    {
        if (hasBeenCollected)
        {
            return;
        }

        if (flashlightController == null)
        {
            flashlightController =
                FindAnyObjectByType<FlashlightController>();
        }

        if (flashlightController == null)
        {
            Debug.LogError("No FlashlightController was found.");
            return;
        }

        hasBeenCollected = true;

        flashlightController.CollectBattery(batteryAmount);

        gameObject.SetActive(false);
    }
}
