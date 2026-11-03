using TMPro;
using UnityEngine;

public class SupplyPickup : MonoBehaviour
{
    [Header("Supply")]
    [SerializeField] private string supplyId = "wood";

    [SerializeField, Min(1)]
    private int amount = 1;

    [Header("Day Availability")]
    [Tooltip(
        "The first day when this pickup can be collected."
    )]
    [SerializeField, Min(1)]
    private int availableFromDay = 2;

    [SerializeField]
    private DayNightCycle dayNightCycle;

    [Header("Interaction")]
    [SerializeField]
    private KeyCode pickupKey =
        KeyCode.E;

    [SerializeField, Min(0.5f)]
    private float pickupDistance = 3f;

    [SerializeField] private TMP_Text promptText;

    [Header("Optional Effects")]
    [SerializeField] private GameObject pickupParticles;
    [SerializeField] private GameObject glowLight;

    private Transform player;

    private bool collected;
    private bool promptIsShowing;
    private bool availabilityInitialized;
    private bool wasAvailable;

    private void Start()
    {
        FindReferences();

        promptIsShowing = false;
        availabilityInitialized = false;

        RefreshAvailability();
    }

    private void Update()
    {
        if (collected)
        {
            return;
        }

        if (dayNightCycle == null)
        {
            dayNightCycle =
                FindAnyObjectByType<DayNightCycle>();
        }

        bool isAvailable =
            IsAvailableToday();

        if (
            !availabilityInitialized ||
            isAvailable != wasAvailable
        )
        {
            availabilityInitialized = true;
            wasAvailable = isAvailable;

            RefreshAvailability();
        }

        if (!isAvailable)
        {
            /*
             * Do not repeatedly disable the shared prompt.
             * A box may currently be using it.
             */
            if (promptIsShowing)
            {
                HideOwnPrompt();
            }

            return;
        }

        if (player == null)
        {
            FindPlayer();

            if (player == null)
            {
                return;
            }
        }

        float distanceSquared =
            (
                transform.position -
                player.position
            ).sqrMagnitude;

        float pickupDistanceSquared =
            pickupDistance *
            pickupDistance;

        bool playerIsClose =
            distanceSquared <=
            pickupDistanceSquared;

        if (playerIsClose)
        {
            ShowOwnPrompt();

            if (Input.GetKeyDown(pickupKey))
            {
                Collect();
            }
        }
        else if (promptIsShowing)
        {
            HideOwnPrompt();
        }
    }

    private void FindReferences()
    {
        if (dayNightCycle == null)
        {
            dayNightCycle =
                FindAnyObjectByType<DayNightCycle>();
        }

        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (playerObject != null)
        {
            player =
                playerObject.transform;

            return;
        }

        FirstPersonController controller =
            FindAnyObjectByType
                <FirstPersonController>();

        if (controller != null)
        {
            player =
                controller.transform;
        }
    }

    private bool IsAvailableToday()
    {
        if (dayNightCycle == null)
        {
            return false;
        }

        return
            dayNightCycle.CurrentDay >=
            availableFromDay;
    }

    private void RefreshAvailability()
    {
        bool isAvailable =
            IsAvailableToday();

        if (pickupParticles != null)
        {
            pickupParticles.SetActive(
                isAvailable && !collected
            );
        }

        if (glowLight != null)
        {
            glowLight.SetActive(
                isAvailable && !collected
            );
        }

        if (!isAvailable && promptIsShowing)
        {
            HideOwnPrompt();
        }
    }

    private void Collect()
    {
        if (
            collected ||
            !IsAvailableToday()
        )
        {
            return;
        }

        SupplyManager manager =
            SupplyManager.Instance;

        if (manager == null)
        {
            manager =
                FindAnyObjectByType<SupplyManager>();
        }

        if (manager == null)
        {
            Debug.LogError(
                "SupplyPickup could not find " +
                "SupplyManager."
            );

            return;
        }

        collected = true;

        string normalizedSupplyId =
            string.IsNullOrWhiteSpace(supplyId)
                ? "wood"
                : supplyId.Trim();

        manager.AddSupply(
            normalizedSupplyId,
            amount
        );

        HideOwnPrompt();

        if (pickupParticles != null)
        {
            pickupParticles.SetActive(false);
        }

        if (glowLight != null)
        {
            glowLight.SetActive(false);
        }

        Debug.Log(
            $"Collected {amount} " +
            $"{normalizedSupplyId}."
        );

        gameObject.SetActive(false);
    }

    private void ShowOwnPrompt()
    {
        if (promptText == null)
        {
            return;
        }

        promptText.gameObject.SetActive(true);

        promptText.text =
            $"Press E to collect " +
            $"{amount} wood";

        promptIsShowing = true;
    }

    private void HideOwnPrompt()
    {
        if (!promptIsShowing)
        {
            return;
        }

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }

        promptIsShowing = false;
    }

    private void OnDisable()
    {
        HideOwnPrompt();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            pickupDistance
        );
    }
}