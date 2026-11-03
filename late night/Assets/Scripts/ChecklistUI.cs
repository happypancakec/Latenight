using System.Text;
using TMPro;
using UnityEngine;

public class ChecklistUI : MonoBehaviour
{
    [SerializeField] private TMP_Text checklistText;
    [SerializeField] private GameObject checklistPanel;

    private TaskManager taskManager;

    private void Start()
    {
        taskManager = TaskManager.Instance;

        if (taskManager == null)
        {
            taskManager =
                FindAnyObjectByType<TaskManager>();
        }

        if (taskManager == null)
        {
            Debug.LogError(
                "ChecklistUI could not find TaskManager."
            );
            return;
        }

        taskManager.TasksChanged += RefreshChecklist;
        RefreshChecklist();
    }

    private void OnDestroy()
    {
        if (taskManager != null)
        {
            taskManager.TasksChanged -= RefreshChecklist;
        }
    }

    private void RefreshChecklist()
    {
        if (taskManager == null || checklistText == null)
        {
            return;
        }

        StringBuilder text = new StringBuilder();

        foreach (TaskManager.TaskEntry task
                 in taskManager.Tasks)
        {
            text.Append(task.completed ? "[X] " : "[ ] ");
            text.AppendLine(task.description);
        }

        bool hasTasks = taskManager.Tasks.Count > 0;

        if (checklistPanel != null)
        {
            checklistPanel.SetActive(hasTasks);
        }

        checklistText.text = text.ToString();
    }
}