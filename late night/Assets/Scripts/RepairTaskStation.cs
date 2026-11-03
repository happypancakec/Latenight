using TMPro;
using UnityEngine;

public class RepairTaskStation : MonoBehaviour
{
    [Header("Task")]
    [SerializeField]
    private string taskId =
        "fix_broken_fence";

    [Header("Required Supplies")]
    [SerializeField]
    private string requiredSupplyId =
        "wood";

    [SerializeField]
    private string supplyDisplayName =
        "wood";

    [SerializeField] private int requiredAmount = 3;

    [Header("Reward")]
    [SerializeField] private int moneyReward = 35;

    [Header("Interaction")]
    [SerializeField]
    private KeyCode interactionKey =
        KeyCode.E;

    [SerializeField] private TMP_Text promptText;
    [SerializeField] private RepairMinigameUI minigame;

    private bool playerNearby;
    private bool repairInProgress;

    private TaskManager taskManager;
    private SupplyManager supplyManager;

    private void Start()
    {
        FindManagers();
        ShowPrompt(false);
    }

    private void Update()
    {
        if (!playerNearby || repairInProgress)
        {
            return;
        }

        if (Input.GetKeyDown(interactionKey))
        {
            TryBeginRepair();
        }
    }

    private void FindManagers()
    {
        taskManager = TaskManager.Instance;
        supplyManager = SupplyManager.Instance;

        if (taskManager == null)
        {
            taskManager =
                FindAnyObjectByType<TaskManager>();
        }

        if (supplyManager == null)
        {
            supplyManager =
                FindAnyObjectByType<SupplyManager>();
        }
    }

    private void TryBeginRepair()
    {
        FindManagers();

        if (taskManager == null ||
            supplyManager == null)
        {
            Debug.LogError(
                "RepairTaskStation could not find " +
                "TaskManager or SupplyManager."
            );

            return;
        }

        if (!taskManager.HasTask(taskId))
        {
            ShowMessage(
                "You don't know what needs fixing yet."
            );

            return;
        }

        if (taskManager.IsTaskComplete(taskId))
        {
            ShowMessage(
                "The fence has already been repaired."
            );

            return;
        }

        int currentAmount =
            supplyManager.GetAmount(requiredSupplyId);

        if (currentAmount < requiredAmount)
        {
            ShowMessage(
                $"You need {requiredAmount} " +
                $"{supplyDisplayName}. " +
                $"You currently have {currentAmount}."
            );

            return;
        }

        if (minigame == null)
        {
            Debug.LogError(
                "Repair Minigame has not been assigned."
            );

            return;
        }

        repairInProgress = true;
        ShowPrompt(false);

        minigame.BeginMinigame(
            CompleteRepair,
            CancelRepair
        );
    }

    private void CompleteRepair()
    {
        if (taskManager == null ||
            supplyManager == null)
        {
            FindManagers();
        }

        bool suppliesUsed =
            supplyManager.TryUseSupply(
                requiredSupplyId,
                requiredAmount
            );

        if (!suppliesUsed)
        {
            repairInProgress = false;

            ShowMessage(
                "The required supplies are unavailable."
            );

            return;
        }

        taskManager.CompleteTask(taskId);
        supplyManager.AddMoney(moneyReward);

        repairInProgress = false;
        ShowPrompt(false);

        if (minigame != null)
        {
            minigame.ShowCompletionCelebration(
                moneyReward
            );
        }

        Debug.Log(
            $"Completed task {taskId}. " +
            $"Used {requiredAmount} {requiredSupplyId}. " +
            $"Earned ${moneyReward}."
        );
    }

    private void CancelRepair()
    {
        repairInProgress = false;

        if (playerNearby)
        {
            RefreshPrompt();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(
            "Repair trigger entered by: " +
            other.name
        );

        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerNearby = true;
        RefreshPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerNearby = false;
        ShowPrompt(false);
    }

    private void RefreshPrompt()
    {
        FindManagers();

        if (taskManager == null)
        {
            return;
        }

        bool taskIsActive =
            taskManager.HasTask(taskId) &&
            !taskManager.IsTaskComplete(taskId);

        ShowPrompt(taskIsActive);
    }

    private void ShowPrompt(bool show)
    {
        if (promptText == null)
        {
            return;
        }

        promptText.gameObject.SetActive(show);

        if (show)
        {
            promptText.text =
                $"Press E to repair the fence " +
                $"({requiredAmount} " +
                $"{supplyDisplayName} required)";
        }
    }

    private void ShowMessage(string message)
    {
        if (promptText == null)
        {
            return;
        }

        promptText.gameObject.SetActive(true);
        promptText.text = message;
    }
}