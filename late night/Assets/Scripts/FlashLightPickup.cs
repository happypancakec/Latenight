using UnityEngine;

public class FlashlightPickup : MonoBehaviour, PickupInteractable
{
    [SerializeField] private FlashlightController flashlightController;

    [Tooltip("How much power is in the flashlight when collected.")]
    [SerializeField] private float startingBatteryLife = 30f;

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
                FindFirstObjectByType<FlashlightController>();
        }

        if (flashlightController == null)
        {
            Debug.LogError("No FlashlightController was found.");
            return;
        }

        hasBeenCollected = true;

        flashlightController.PickUpFlashlight(
            startingBatteryLife
        );

        gameObject.SetActive(false);
    }
}