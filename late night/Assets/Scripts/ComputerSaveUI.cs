using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ComputerSaveUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text saveDetailsText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button saveButton;

    [Header("Saving Hours")]
    [Range(0f, 24f)]
    [SerializeField] private float daytimeStartsAt = 6f;

    [Range(0f, 24f)]
    [SerializeField] private float nighttimeStartsAt = 20f;

    private FirstPersonController playerController;
    private PlayerStamina playerStamina;
    private DayNightCycle dayNightCycle;

    private void OnEnable()
    {
        FindGameReferences();
        RefreshWindow();
    }

    private void FindGameReferences()
    {
        playerController =
            FindAnyObjectByType<FirstPersonController>();

        playerStamina =
            FindAnyObjectByType<PlayerStamina>();

        dayNightCycle =
            FindAnyObjectByType<DayNightCycle>();
    }

    private void RefreshWindow()
    {
        if (statusText != null)
        {
            statusText.text = "";
        }

        if (dayNightCycle == null)
        {
            if (saveDetailsText != null)
            {
                saveDetailsText.text =
                    "Unable to access game information.";
            }

            if (saveButton != null)
            {
                saveButton.interactable = false;
            }

            return;
        }

        bool daytime = IsDaytime();

        if (saveButton != null)
        {
            saveButton.interactable = daytime;
        }

        if (saveDetailsText != null)
        {
            string timeText =
                FormatTime(dayNightCycle.TimeOfDay);

            saveDetailsText.text =
                "Current Day: Day " +
                dayNightCycle.CurrentDay +
                "\nCurrent Time: " +
                timeText +
                "\nLocation: Trailer Park";
        }

        if (!daytime && statusText != null)
        {
            statusText.text =
                "Manual saving is unavailable at night.\n" +
                "Use the bed to save and begin the next day.";
        }
    }

    public void SaveCurrentGame()
    {
        if (dayNightCycle == null)
        {
            ShowStatus(
                "Save failed: the day/night system was not found."
            );

            return;
        }

        if (!IsDaytime())
        {
            ShowStatus(
                "It's too late to save here. Use the bed."
            );

            return;
        }

        Transform playerTransform =
            playerController != null
                ? playerController.transform
                : null;

        if (playerTransform != null)
        {
            PlayerPrefs.SetFloat(
                "PlayerPositionX",
                playerTransform.position.x
            );

            PlayerPrefs.SetFloat(
                "PlayerPositionY",
                playerTransform.position.y
            );

            PlayerPrefs.SetFloat(
                "PlayerPositionZ",
                playerTransform.position.z
            );

            PlayerPrefs.SetFloat(
                "PlayerRotationY",
                playerTransform.eulerAngles.y
            );
        }

        PlayerPrefs.SetInt(
            "CurrentDay",
            dayNightCycle.CurrentDay
        );

        PlayerPrefs.SetFloat(
            "TimeOfDay",
            dayNightCycle.TimeOfDay
        );

        if (playerStamina != null)
        {
            PlayerPrefs.SetFloat(
                "CurrentStamina",
                playerStamina.CurrentStamina
            );
        }

        PlayerPrefs.SetString(
            "SavedScene",
            SceneManager.GetActiveScene().name
        );

        PlayerPrefs.SetString(
            "LastSaveDisplay",
            "Day " +
            dayNightCycle.CurrentDay +
            " at " +
            FormatTime(dayNightCycle.TimeOfDay)
        );

        PlayerPrefs.SetInt("HasSaveGame", 1);
        PlayerPrefs.Save();

        ShowStatus(
            "Game saved successfully.\n" +
            "Day " +
            dayNightCycle.CurrentDay +
            " — " +
            FormatTime(dayNightCycle.TimeOfDay)
        );
    }

    private bool IsDaytime()
    {
        if (dayNightCycle == null)
        {
            return false;
        }

        float time = dayNightCycle.TimeOfDay;

        return
            time >= daytimeStartsAt &&
            time < nighttimeStartsAt;
    }

    private string FormatTime(float time)
    {
        int hour24 = Mathf.FloorToInt(time);
        int minutes = Mathf.FloorToInt(
            (time - hour24) * 60f
        );

        string period =
            hour24 >= 12 ? "PM" : "AM";

        int hour12 = hour24 % 12;

        if (hour12 == 0)
        {
            hour12 = 12;
        }

        return string.Format(
            "{0}:{1:00} {2}",
            hour12,
            minutes,
            period
        );
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log(message);
    }
}