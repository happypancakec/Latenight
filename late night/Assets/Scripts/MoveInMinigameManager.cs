using System;
using System.Text;
using TMPro;
using UnityEngine;

public class MoveInMinigameManager : MonoBehaviour
{
    [Header("Carrying")]
    [SerializeField] private Transform carryPoint;

    [Tooltip(
        "Prevents a box from being placed immediately " +
        "after it is picked up."
    )]
    [SerializeField, Min(0f)]
    private float placementInputDelay = 0.35f;

    [Header("Progress")]
    [SerializeField, Min(1)]
    private int totalBoxes = 4;

    [SerializeField] private TMP_Text progressText;

    [Header("Task")]
    [SerializeField]
    private string taskId =
        "move_into_trailer";

    [Header("Reward")]
    [SerializeField, Min(0)]
    private int moneyReward = 25;

    [Header("Completion")]
    [Tooltip(
        "Assign the same RepairMinigameUI used by " +
        "the fence repair task."
    )]
    [SerializeField]
    private RepairMinigameUI completionUI;

    private MoveInBox carriedBox;

    private TaskManager taskManager;
    private SupplyManager supplyManager;

    private int deliveredBoxes;
    private float deliveryAllowedAt;
    private bool taskCompleted;

    public string CarriedBoxId
    {
        get
        {
            if (carriedBox == null)
            {
                return "";
            }

            return NormalizeId(
                carriedBox.BoxId
            );
        }
    }

    public int DeliveredBoxes =>
        deliveredBoxes;

    public bool IsCarryingBox =>
        carriedBox != null;

    private void Awake()
    {
        FindManagers();
    }

    private void Start()
    {
        RefreshProgressText();
    }

    private void FindManagers()
    {
        taskManager = TaskManager.Instance;

        if (taskManager == null)
        {
            taskManager =
                FindAnyObjectByType<TaskManager>();
        }

        supplyManager = SupplyManager.Instance;

        if (supplyManager == null)
        {
            supplyManager =
                FindAnyObjectByType<SupplyManager>();
        }
    }

    public bool PickUpBox(MoveInBox box)
    {
        if (box == null)
        {
            return false;
        }

        if (taskCompleted)
        {
            return false;
        }

        if (carriedBox != null)
        {
            Debug.Log(
                "You are already carrying a box."
            );

            return false;
        }

        FindManagers();

        if (
            taskManager != null &&
            (
                !taskManager.HasTask(taskId) ||
                taskManager.IsTaskComplete(taskId)
            )
        )
        {
            Debug.Log(
                "The move-in task is not active."
            );

            return false;
        }

        if (carryPoint == null)
        {
            Debug.LogError(
                "MoveInMinigameManager does not have " +
                "a BoxCarryPoint assigned."
            );

            return false;
        }

        carriedBox = box;

        deliveryAllowedAt =
            Time.time + placementInputDelay;

        carriedBox.BeginCarrying(
            carryPoint
        );

        Debug.Log(
            "Picked up box. ID: " +
            CarriedBoxId
        );

        return true;
    }

    public bool IsCarryingMatchingBox(
        string requiredBoxId
    )
    {
        if (carriedBox == null)
        {
            return false;
        }

        string carriedId =
            NormalizeId(
                carriedBox.BoxId
            );

        string requiredId =
            NormalizeId(
                requiredBoxId
            );

        return string.Equals(
            carriedId,
            requiredId,
            StringComparison.OrdinalIgnoreCase
        );
    }

    public bool CanDeliverBox(
        string requiredBoxId
    )
    {
        if (taskCompleted)
        {
            return false;
        }

        if (carriedBox == null)
        {
            return false;
        }

        if (Time.time < deliveryAllowedAt)
        {
            return false;
        }

        return IsCarryingMatchingBox(
            requiredBoxId
        );
    }

    public bool DeliverCarriedBox(
        string requiredBoxId
    )
    {
        if (!CanDeliverBox(requiredBoxId))
        {
            Debug.LogWarning(
                "This placement point cannot accept " +
                "the currently carried box."
            );

            return false;
        }

        MoveInBox deliveredBox =
            carriedBox;

        carriedBox = null;

        deliveredBox.DeliverBox();

        deliveredBoxes++;

        deliveredBoxes = Mathf.Clamp(
            deliveredBoxes,
            0,
            totalBoxes
        );

        Debug.Log(
            $"Delivered {deliveredBox.BoxId}. " +
            $"Progress: {deliveredBoxes}/" +
            $"{totalBoxes}."
        );

        if (deliveredBoxes >= totalBoxes)
        {
            CompleteMoveInTask();
        }
        else
        {
            RefreshProgressText();
        }

        return true;
    }

    private void RefreshProgressText()
    {
        if (progressText == null)
        {
            return;
        }

        progressText.gameObject.SetActive(true);

        progressText.text =
            $"{deliveredBoxes} / " +
            $"{totalBoxes} boxes moved";
    }

    private void CompleteMoveInTask()
    {
        if (taskCompleted)
        {
            return;
        }

        taskCompleted = true;

        FindManagers();

        /*
         * Hide the box progress text. The reused repair
         * completion panel will display the final message.
         */
        if (progressText != null)
        {
            progressText.text = "";
            progressText.gameObject.SetActive(false);
        }

        /*
         * Give the player their reward before displaying
         * the celebration.
         */
        if (supplyManager != null)
        {
            supplyManager.AddMoney(
                moneyReward
            );
        }
        else
        {
            Debug.LogWarning(
                "MoveInMinigameManager could not find " +
                "SupplyManager, so the reward was not added."
            );
        }

        /*
         * Reuse the completion celebration from the
         * fence repair minigame.
         */
        if (completionUI != null)
        {
            completionUI.ShowCompletionCelebration(
                moneyReward
            );
        }
        else
        {
            Debug.LogWarning(
                "Completion UI has not been assigned " +
                "on MoveInMinigameManager."
            );
        }

        /*
         * Complete the task after opening the celebration.
         * This prevents DayOneContentController from hiding
         * the Day 1 objects before the celebration is called.
         */
        if (
            taskManager != null &&
            !taskManager.IsTaskComplete(taskId)
        )
        {
            taskManager.CompleteTask(
                taskId
            );
        }
        else if (taskManager == null)
        {
            Debug.LogWarning(
                "MoveInMinigameManager could not find " +
                "TaskManager."
            );
        }

        Debug.Log(
            "Move-in task completed. " +
            $"Rewarded ${moneyReward}."
        );
    }

    private string NormalizeId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return "";
        }

        StringBuilder normalized =
            new StringBuilder();

        foreach (char character in id)
        {
            if (char.IsLetterOrDigit(character))
            {
                normalized.Append(
                    char.ToLowerInvariant(character)
                );
            }
        }

        return normalized.ToString();
    }
}