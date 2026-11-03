using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField] private float maximumStamina = 100f;
    [SerializeField] private float currentStamina = 100f;

    [Header("Stamina Usage")]
    [SerializeField] private float sprintDrainPerSecond = 18f;
    [SerializeField] private float regenerationPerSecond = 14f;
    [SerializeField] private float regenerationDelay = 1f;

    [Header("Exhaustion")]
    [Tooltip("Stamina required before sprinting again after exhaustion.")]
    [SerializeField] private float recoveryThreshold = 20f;

    private bool sprinting;
    private bool exhausted;
    private float lastSprintTime;

    public float CurrentStamina => currentStamina;
    public float MaximumStamina => maximumStamina;
    public bool IsExhausted => exhausted;

    public bool CanSprint =>
        !exhausted && currentStamina > 0f;

    private void Awake()
    {
        currentStamina = Mathf.Clamp(
            currentStamina,
            0f,
            maximumStamina
        );
    }

    private void Update()
    {
        if (sprinting && !exhausted)
        {
            DrainStamina();
        }
        else
        {
            RegenerateStamina();
        }

        UpdateExhaustion();
    }

    public void SetSprinting(bool value)
    {
        sprinting = value && CanSprint;

        if (sprinting)
        {
            lastSprintTime = Time.time;
        }
    }

    private void DrainStamina()
    {
        currentStamina -=
            sprintDrainPerSecond * Time.deltaTime;

        currentStamina = Mathf.Clamp(
            currentStamina,
            0f,
            maximumStamina
        );

        lastSprintTime = Time.time;

        if (currentStamina <= 0f)
        {
            exhausted = true;
            sprinting = false;
        }
    }

    private void RegenerateStamina()
    {
        if (Time.time < lastSprintTime + regenerationDelay)
        {
            return;
        }

        currentStamina +=
            regenerationPerSecond * Time.deltaTime;

        currentStamina = Mathf.Clamp(
            currentStamina,
            0f,
            maximumStamina
        );
    }

    private void UpdateExhaustion()
    {
        if (
            exhausted &&
            currentStamina >= recoveryThreshold
        )
        {
            exhausted = false;
        }
    }

    public void RestoreAllStamina()
    {
        currentStamina = maximumStamina;
        exhausted = false;
        sprinting = false;
    }

    public void SetStamina(float amount)
    {
        currentStamina = Mathf.Clamp(
            amount,
            0f,
            maximumStamina
        );

        exhausted = currentStamina <= 0f;
    }
}