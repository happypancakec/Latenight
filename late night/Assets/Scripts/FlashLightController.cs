using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Held Flashlight")]
    [SerializeField] private GameObject heldFlashlight;
    [SerializeField] private Light flashlightLight;

    [Header("Battery")]
    [SerializeField] private float maximumBatteryLife = 60f;
    [SerializeField] private float currentBatteryLife;
    [SerializeField] private int batteryCount;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F;
    [SerializeField] private KeyCode reloadKey = KeyCode.R;

    private bool hasFlashlight;
    private bool flashlightIsOn;

    // These properties allow the UI and pickup scripts to read
    // the flashlight's current state.
    public bool HasFlashlight => hasFlashlight;
    public bool FlashlightIsOn => flashlightIsOn;
    public float CurrentBatteryLife => currentBatteryLife;
    public float MaximumBatteryLife => maximumBatteryLife;
    public int BatteryCount => batteryCount;

    private void Start()
    {
        if (heldFlashlight != null)
        {
            heldFlashlight.SetActive(false);
        }

        if (flashlightLight != null)
        {
            flashlightLight.enabled = false;
        }

        flashlightIsOn = false;
    }

    private void Update()
    {
        if (!hasFlashlight)
        {
            return;
        }

        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }

        if (Input.GetKeyDown(reloadKey))
        {
            InsertBattery();
        }

        DrainBattery();
    }

    public void PickUpFlashlight(float startingBatteryLife)
    {
        if (hasFlashlight)
        {
            return;
        }

        hasFlashlight = true;

        currentBatteryLife = Mathf.Clamp(
            startingBatteryLife,
            0f,
            maximumBatteryLife
        );

        if (heldFlashlight != null)
        {
            heldFlashlight.SetActive(true);
        }

        SetFlashlight(false);

        Debug.Log(
            "Flashlight collected. Press F to use it."
        );
    }

    public void CollectBattery(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        batteryCount += amount;

        Debug.Log(
            "Battery collected. Batteries: " +
            batteryCount
        );
    }

    private void ToggleFlashlight()
    {
        if (flashlightIsOn)
        {
            SetFlashlight(false);
            return;
        }

        if (currentBatteryLife <= 0f)
        {
            Debug.Log(
                "The flashlight is empty. Press R to insert a battery."
            );

            return;
        }

        SetFlashlight(true);
    }

    private void DrainBattery()
    {
        if (!flashlightIsOn)
        {
            return;
        }

        currentBatteryLife -= Time.deltaTime;

        currentBatteryLife = Mathf.Clamp(
            currentBatteryLife,
            0f,
            maximumBatteryLife
        );

        if (currentBatteryLife <= 0f)
        {
            SetFlashlight(false);

            Debug.Log(
                "The flashlight battery died."
            );
        }
    }

    private void InsertBattery()
    {
        if (!hasFlashlight)
        {
            return;
        }

        if (batteryCount <= 0)
        {
            Debug.Log(
                "You don't have any spare batteries."
            );

            return;
        }

        if (currentBatteryLife >= maximumBatteryLife)
        {
            Debug.Log(
                "The flashlight battery is already full."
            );

            return;
        }

        batteryCount--;

        currentBatteryLife = maximumBatteryLife;

        Debug.Log(
            "Battery inserted. Batteries remaining: " +
            batteryCount
        );
    }

    private void SetFlashlight(bool turnOn)
    {
        flashlightIsOn =
            turnOn && currentBatteryLife > 0f;

        if (flashlightLight != null)
        {
            flashlightLight.enabled = flashlightIsOn;
        }
    }
}