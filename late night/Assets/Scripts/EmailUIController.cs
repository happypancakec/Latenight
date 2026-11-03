using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EmailUIController : MonoBehaviour
{
    [System.Serializable]
    public class EmailData
    {
        [Header("Identity")]
        public string id;

        [Header("Email")]
        public string sender;
        public string subject;

        [TextArea(4, 12)]
        public string message;

        [Min(1)]
        public int availableFromDay = 1;

        [Header("Optional Task")]
        public bool addsTask;
        public string taskId;

        [TextArea(2, 4)]
        public string taskDescription;
    }

    [Header("Inbox")]
    [SerializeField] private Transform emailContent;
    [SerializeField] private EmailListItemUI emailItemPrefab;

    [Header("Reading Panel")]
    [SerializeField] private TMP_Text fromText;
    [SerializeField] private TMP_Text subjectText;
    [SerializeField] private TMP_Text bodyText;

    [Header("New Email Badge")]
    [SerializeField] private GameObject notificationBadge;
    [SerializeField] private TMP_Text notificationCountText;

    [Header("Game References")]
    [SerializeField] private DayNightCycle dayNightCycle;

    [Header("Emails")]
    [SerializeField]
    private List<EmailData> emails =
        new List<EmailData>();

    private void OnEnable()
    {
        FindGameReferences();
        BuildInbox();
        ClearReadingPanel();
        RefreshNotificationBadge();
    }

    private void FindGameReferences()
    {
        if (dayNightCycle == null)
        {
            dayNightCycle =
                FindAnyObjectByType<DayNightCycle>();
        }
    }

    private void BuildInbox()
    {
        if (emailContent == null ||
            emailItemPrefab == null)
        {
            Debug.LogError(
                "EmailUIController is missing its " +
                "Email Content or Email Item Prefab."
            );

            return;
        }

        foreach (Transform child in emailContent)
        {
            Destroy(child.gameObject);
        }

        int currentDay = GetCurrentDay();

        foreach (EmailData email in emails)
        {
            if (email.availableFromDay > currentDay)
            {
                continue;
            }

            EmailListItemUI item = Instantiate(
                emailItemPrefab,
                emailContent
            );

            item.Setup(
                email,
                DisplayEmail
            );
        }
    }

    private void DisplayEmail(EmailData email)
    {
        if (fromText != null)
        {
            fromText.text =
                "From: " + email.sender;
        }

        if (subjectText != null)
        {
            subjectText.text = email.subject;
        }

        if (bodyText != null)
        {
            bodyText.text = email.message;
        }

        MarkEmailAsRead(email);
        AddEmailTask(email);
        RefreshNotificationBadge();
    }

    private void MarkEmailAsRead(EmailData email)
    {
        if (string.IsNullOrWhiteSpace(email.id))
        {
            Debug.LogWarning(
                "An email has no unique ID: " +
                email.subject
            );

            return;
        }

        PlayerPrefs.SetInt(
            GetReadKey(email.id),
            1
        );

        PlayerPrefs.Save();
    }

    private void AddEmailTask(EmailData email)
    {
        if (!email.addsTask)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email.taskId))
        {
            Debug.LogError(
                "Email is set to add a task but " +
                "has no Task ID: " + email.subject
            );

            return;
        }

        TaskManager manager = TaskManager.Instance;

        if (manager == null)
        {
            manager =
                FindAnyObjectByType<TaskManager>();
        }

        if (manager == null)
        {
            Debug.LogError(
                "The email could not find TaskManager."
            );

            return;
        }

        manager.AddTask(
            email.taskId,
            email.taskDescription
        );
    }

    public void RefreshNotificationBadge()
    {
        FindGameReferences();

        int unreadCount = 0;
        int currentDay = GetCurrentDay();

        foreach (EmailData email in emails)
        {
            if (email.availableFromDay > currentDay)
            {
                continue;
            }

            if (!IsEmailRead(email.id))
            {
                unreadCount++;
            }
        }

        if (notificationBadge != null)
        {
            notificationBadge.SetActive(
                unreadCount > 0
            );
        }

        if (notificationCountText != null)
        {
            notificationCountText.text =
                unreadCount.ToString();
        }
    }

    private bool IsEmailRead(string emailId)
    {
        if (string.IsNullOrWhiteSpace(emailId))
        {
            return false;
        }

        return PlayerPrefs.GetInt(
            GetReadKey(emailId),
            0
        ) == 1;
    }

    private string GetReadKey(string emailId)
    {
        return "EmailRead_" + emailId;
    }

    private int GetCurrentDay()
    {
        FindGameReferences();

        return dayNightCycle != null
            ? dayNightCycle.CurrentDay
            : 1;
    }

    private void ClearReadingPanel()
    {
        if (fromText != null)
        {
            fromText.text = "";
        }

        if (subjectText != null)
        {
            subjectText.text = "Select an email";
        }

        if (bodyText != null)
        {
            bodyText.text =
                "Choose a message from your inbox " +
                "to read it.";
        }
    }

    [ContextMenu("Clear Saved Email Status")]
    public void ClearSavedEmailStatus()
    {
        foreach (EmailData email in emails)
        {
            if (string.IsNullOrWhiteSpace(email.id))
            {
                continue;
            }

            PlayerPrefs.DeleteKey(
                GetReadKey(email.id)
            );
        }

        PlayerPrefs.Save();

        if (gameObject.activeInHierarchy)
        {
            BuildInbox();
            ClearReadingPanel();
        }

        RefreshNotificationBadge();

        Debug.Log(
            "All saved email read statuses were cleared."
        );
    }
}