using UnityEngine;

public class DayOneContentController : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private TaskManager taskManager;

    [Header("Permanent Content")]
    [SerializeField] private GameObject car;

    [Header("Day One Content")]
    [SerializeField] private GameObject dayOneMoveInContent;

    [SerializeField] private GameObject[] moveInBoxes;

    [Header("Move-In Task")]
    [SerializeField]
    private string taskId =
        "move_into_trailer";

    [SerializeField]
    private string taskDescription =
        "Move your boxes into the trailer.";

    private void Start()
    {
        FindReferences();
        RefreshDayOneContent();
    }

    private void OnEnable()
    {
        FindReferences();
        RefreshDayOneContent();
    }

    private void FindReferences()
    {
        if (dayNightCycle == null)
        {
            dayNightCycle =
                FindAnyObjectByType<DayNightCycle>();
        }

        if (taskManager == null)
        {
            taskManager = TaskManager.Instance;
        }

        if (taskManager == null)
        {
            taskManager =
                FindAnyObjectByType<TaskManager>();
        }
    }

    public void RefreshDayOneContent()
    {
        if (car != null)
        {
            car.SetActive(true);
        }

        if (dayNightCycle == null)
        {
            Debug.LogError(
                "DayOneContentController cannot find " +
                "DayNightCycle."
            );

            return;
        }

        bool isDayOne =
            dayNightCycle.CurrentDay == 1;

        bool taskCompleted =
            taskManager != null &&
            taskManager.IsTaskComplete(taskId);

        bool shouldShowBoxes =
            isDayOne && !taskCompleted;

        if (dayOneMoveInContent != null)
        {
            dayOneMoveInContent.SetActive(
                shouldShowBoxes
            );
        }

        if (moveInBoxes != null)
        {
            foreach (GameObject box in moveInBoxes)
            {
                if (box != null)
                {
                    box.SetActive(shouldShowBoxes);
                }
            }
        }

        if (isDayOne &&
            !taskCompleted &&
            taskManager != null &&
            !taskManager.HasTask(taskId))
        {
            taskManager.AddTask(
                taskId,
                taskDescription
            );
        }

        Debug.Log(
            "Move-in content: " +
            shouldShowBoxes +
            " | Current day: " +
            dayNightCycle.CurrentDay +
            " | Task complete: " +
            taskCompleted
        );
    }
}