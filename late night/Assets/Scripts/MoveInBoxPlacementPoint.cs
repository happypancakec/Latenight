using TMPro;
using UnityEngine;

public class MoveInBoxPlacementPoint : MonoBehaviour
{
    [Header("Required Box")]
    [SerializeField]
    private string requiredBoxId =
        "bedroom";

    [Header("Task")]
    [SerializeField]
    private string taskId =
        "move_into_trailer";

    [Header("Interaction")]
    [SerializeField]
    private KeyCode interactionKey =
        KeyCode.E;

    [SerializeField]
    private TMP_Text promptText;

    [Header("References")]
    [SerializeField]
    private MoveInMinigameManager minigameManager;

    [SerializeField]
    private GameObject markerVisual;

    [SerializeField]
    private ParticleSystem markerParticles;

    [SerializeField]
    private Collider placementTrigger;

    private bool playerNearby;
    private bool pointUsed;
    private bool markerVisible;

    private TaskManager taskManager;

    private void Start()
    {
        FindReferences();
        ShowPrompt(false);

        // Force the marker to be hidden initially.
        markerVisible = true;
        SetMarkerVisible(false);
    }

    private void Update()
    {
        if (minigameManager == null ||
            taskManager == null)
        {
            FindReferences();
        }

        bool canAcceptBox =
            CanAcceptCurrentBox();

        SetMarkerVisible(canAcceptBox);

        if (playerNearby)
        {
            ShowPrompt(canAcceptBox);
        }

        if (!playerNearby ||
            !canAcceptBox)
        {
            return;
        }

        if (Input.GetKeyDown(
            interactionKey
        ))
        {
            PlaceBox();
        }
    }

    private void FindReferences()
    {
        if (taskManager == null)
        {
            taskManager =
                TaskManager.Instance;
        }

        if (taskManager == null)
        {
            taskManager =
                FindAnyObjectByType<
                    TaskManager
                >();
        }

        if (minigameManager == null)
        {
            minigameManager =
                FindAnyObjectByType<
                    MoveInMinigameManager
                >();
        }

        if (placementTrigger == null)
        {
            placementTrigger =
                GetComponent<Collider>();
        }

        if (markerParticles == null &&
            markerVisual != null)
        {
            markerParticles =
                markerVisual
                    .GetComponentInChildren<
                        ParticleSystem
                    >(true);
        }
    }

    private bool CanAcceptCurrentBox()
    {
        if (pointUsed ||
            minigameManager == null)
        {
            return false;
        }

        if (!TaskIsActive())
        {
            return false;
        }

        return minigameManager
            .IsCarryingMatchingBox(
                requiredBoxId
            );
    }

    private bool TaskIsActive()
    {
        if (taskManager == null)
        {
            taskManager =
                TaskManager.Instance;

            if (taskManager == null)
            {
                taskManager =
                    FindAnyObjectByType<
                        TaskManager
                    >();
            }
        }

        return
            taskManager != null &&
            taskManager.HasTask(taskId) &&
            !taskManager.IsTaskComplete(
                taskId
            );
    }

    private void PlaceBox()
    {
        if (minigameManager == null)
        {
            Debug.LogError(
                gameObject.name +
                " cannot find MoveInMinigameManager."
            );

            return;
        }

        if (!minigameManager.CanDeliverBox(
            requiredBoxId
        ))
        {
            Debug.LogWarning(
                gameObject.name +
                " cannot deliver the current box."
            );

            return;
        }

        bool delivered =
            minigameManager.DeliverCarriedBox(
                requiredBoxId
            );

        if (!delivered)
        {
            Debug.LogWarning(
                gameObject.name +
                " failed to deliver the box."
            );

            return;
        }

        pointUsed = true;
        playerNearby = false;

        ShowPrompt(false);
        SetMarkerVisible(false);

        if (placementTrigger != null)
        {
            placementTrigger.enabled =
                false;
        }

        Debug.Log(
            gameObject.name +
            " successfully received the " +
            requiredBoxId +
            " box."
        );
    }

    private void OnTriggerEnter(
        Collider other
    )
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        FindReferences();
        playerNearby = true;

        string carriedId =
            minigameManager != null
                ? minigameManager.CarriedBoxId
                : "NO MANAGER";

        bool taskActive =
            TaskIsActive();

        bool canAccept =
            CanAcceptCurrentBox();

        Debug.Log(
            "PLACEMENT DEBUG | Point: " +
            gameObject.name +
            " | Required ID: " +
            requiredBoxId +
            " | Carried ID: " +
            carriedId +
            " | Point used: " +
            pointUsed +
            " | Task active: " +
            taskActive +
            " | Can accept: " +
            canAccept
        );

        ShowPrompt(canAccept);
    }

    private void OnTriggerExit(
        Collider other
    )
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerNearby = false;
        ShowPrompt(false);
    }

    private void ShowPrompt(bool show)
    {
        if (promptText == null)
        {
            return;
        }

        promptText.gameObject.SetActive(
            show
        );

        if (show)
        {
            promptText.text =
                "Press E to place the " +
                requiredBoxId +
                " box";
        }
    }

    private void SetMarkerVisible(
        bool visible
    )
    {
        if (markerVisible == visible)
        {
            return;
        }

        markerVisible = visible;

        if (visible)
        {
            if (markerVisual != null)
            {
                markerVisual.SetActive(
                    true
                );
            }

            if (markerParticles != null)
            {
                markerParticles.gameObject
                    .SetActive(true);

                markerParticles.Clear(true);
                markerParticles.Play(true);
            }
        }
        else
        {
            if (markerParticles != null)
            {
                markerParticles.Stop(
                    true,
                    ParticleSystemStopBehavior
                        .StopEmittingAndClear
                );
            }

            if (markerVisual != null)
            {
                markerVisual.SetActive(
                    false
                );
            }
        }
    }
}