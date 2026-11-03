using UnityEngine;

public class TaskWorldMarker : MonoBehaviour
{
    [SerializeField]
    private string taskId =
        "fix_broken_fence";

    [SerializeField] private GameObject markerVisual;

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
                "TaskWorldMarker could not find TaskManager."
            );

            if (markerVisual != null)
            {
                markerVisual.SetActive(false);
            }

            return;
        }

        taskManager.TasksChanged += RefreshMarker;
        RefreshMarker();
    }

    private void OnDestroy()
    {
        if (taskManager != null)
        {
            taskManager.TasksChanged -= RefreshMarker;
        }
    }

    private void RefreshMarker()
    {
        if (taskManager == null || markerVisual == null)
        {
            return;
        }

        bool taskIsActive =
            taskManager.HasTask(taskId) &&
            !taskManager.IsTaskComplete(taskId);

        markerVisual.SetActive(taskIsActive);
    }
}