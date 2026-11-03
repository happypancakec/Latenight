using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [Serializable]
    public class TaskEntry
    {
        public string id;
        public string description;
        public bool completed;
    }

    [Serializable]
    private class TaskSaveData
    {
        public List<TaskEntry> tasks =
            new List<TaskEntry>();
    }

    public static TaskManager Instance
    {
        get;
        private set;
    }

    [Header("Current Tasks")]
    [SerializeField]
    private List<TaskEntry> tasks =
        new List<TaskEntry>();

    public IReadOnlyList<TaskEntry> Tasks =>
        tasks;

    public event Action TasksChanged;

    private const string SaveKey =
        "TaskSaveData";

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadTasks();
    }

    public void AddTask(
        string id,
        string description
    )
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogError(
                "A task needs a unique ID."
            );

            return;
        }

        id = id.Trim();

        if (HasTask(id))
        {
            return;
        }

        TaskEntry newTask =
            new TaskEntry
            {
                id = id,
                description = description,
                completed = false
            };

        tasks.Add(newTask);

        SaveTasks();
        TasksChanged?.Invoke();

        Debug.Log(
            "New task added: " +
            description
        );
    }

    public void CompleteTask(string id)
    {
        TaskEntry task =
            FindTask(id);

        if (task == null)
        {
            Debug.LogWarning(
                "Could not complete task because it " +
                "does not exist: " + id
            );

            return;
        }

        if (task.completed)
        {
            return;
        }

        task.completed = true;

        SaveTasks();
        TasksChanged?.Invoke();

        Debug.Log(
            "Task completed: " +
            task.description
        );
    }

    public bool HasTask(string id)
    {
        return FindTask(id) != null;
    }

    public bool IsTaskComplete(string id)
    {
        TaskEntry task =
            FindTask(id);

        return
            task != null &&
            task.completed;
    }

    public bool AreAllTasksComplete()
    {
        if (tasks.Count == 0)
        {
            return false;
        }

        foreach (TaskEntry task in tasks)
        {
            if (!task.completed)
            {
                return false;
            }
        }

        return true;
    }

    public void ClearTasksForNewDay()
    {
        tasks.Clear();

        SaveTasks();
        TasksChanged?.Invoke();

        Debug.Log(
            "Previous day's tasks were cleared."
        );
    }

    private TaskEntry FindTask(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        string normalizedId =
            id.Trim();

        return tasks.Find(
            task =>
                task != null &&
                string.Equals(
                    task.id,
                    normalizedId,
                    StringComparison.OrdinalIgnoreCase
                )
        );
    }

    private void SaveTasks()
    {
        TaskSaveData data =
            new TaskSaveData
            {
                tasks = tasks
            };

        string json =
            JsonUtility.ToJson(data);

        PlayerPrefs.SetString(
            SaveKey,
            json
        );

        PlayerPrefs.Save();
    }

    private void LoadTasks()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            tasks =
                new List<TaskEntry>();

            return;
        }

        string json =
            PlayerPrefs.GetString(
                SaveKey
            );

        TaskSaveData data =
            JsonUtility.FromJson<TaskSaveData>(
                json
            );

        if (
            data != null &&
            data.tasks != null
        )
        {
            tasks = data.tasks;
        }
        else
        {
            tasks =
                new List<TaskEntry>();
        }
    }

    [ContextMenu("Clear Saved Tasks")]
    public void ClearSavedTasks()
    {
        tasks.Clear();

        PlayerPrefs.DeleteKey(
            SaveKey
        );

        PlayerPrefs.Save();

        TasksChanged?.Invoke();

        Debug.Log(
            "All saved tasks were cleared."
        );
    }
}