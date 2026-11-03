using System.Collections;
using TMPro;
using UnityEngine;

public class BedInteraction : MonoBehaviour, Interactable
{
    [Header("Player References")]
    [SerializeField] private Transform player;
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private PlayerStamina playerStamina;

    [Header("Time")]
    [SerializeField] private DayNightCycle dayNightCycle;

    [Range(0f, 24f)]
    [SerializeField] private float nightStartsAt = 20f;

    [Range(0f, 24f)]
    [SerializeField] private float nightEndsAt = 6f;

    [Range(0f, 24f)]
    [SerializeField] private float wakeUpTime = 8f;

    [Header("Transition UI")]
    [SerializeField] private CanvasGroup blackOverlay;
    [SerializeField] private TMP_Text dayText;

    [Tooltip(
        "Use a separate TMP text object for bed messages. " +
        "Do not use the normal interaction prompt."
    )]
    [SerializeField] private TMP_Text messageText;

    [Header("Transition Timing")]
    [SerializeField] private float fadeToBlackDuration = 1.5f;
    [SerializeField] private float blackScreenPause = 0.75f;
    [SerializeField] private float dayTextDuration = 2.5f;
    [SerializeField] private float fadeFromBlackDuration = 1.5f;

    [Header("Messages")]
    [SerializeField]
    private string tooEarlyMessage =
        "It's too early to sleep.";

    [SerializeField] private float messageDuration = 2.5f;

    private bool transitionRunning;
    private Coroutine messageCoroutine;

    private void Awake()
    {
        PrepareUI();
    }

    private void Start()
    {
        PrepareUI();
    }

    private void PrepareUI()
    {
        HideMessageImmediately();

        if (dayText != null)
        {
            dayText.text = "";
            dayText.gameObject.SetActive(false);
        }

        if (blackOverlay != null)
        {
            ForceOverlayToFillParent();

            blackOverlay.alpha = 0f;
            blackOverlay.interactable = false;
            blackOverlay.blocksRaycasts = false;
            blackOverlay.gameObject.SetActive(false);
        }
    }

    private void ForceOverlayToFillParent()
    {
        if (blackOverlay == null)
        {
            return;
        }

        RectTransform overlayRect =
            blackOverlay.GetComponent<RectTransform>();

        if (overlayRect == null)
        {
            Debug.LogError(
                "BlackOverlay does not have a RectTransform."
            );

            return;
        }

        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.pivot =
            new Vector2(0.5f, 0.5f);

        overlayRect.localScale = Vector3.one;
        overlayRect.localRotation =
            Quaternion.identity;
    }

    public void Interact()
    {
        if (transitionRunning)
        {
            return;
        }

        if (dayNightCycle == null)
        {
            Debug.LogError(
                "BedInteraction has no DayNightCycle assigned."
            );

            return;
        }

        bool canSleep = dayNightCycle.IsNight(
            nightStartsAt,
            nightEndsAt
        );

        if (!canSleep)
        {
            ShowMessage(tooEarlyMessage);
            return;
        }

        HideMessage();
        StartCoroutine(SleepAndSave());
    }

    private IEnumerator SleepAndSave()
    {
        transitionRunning = true;

        DisablePlayerControls();

        if (blackOverlay != null)
        {
            ForceOverlayToFillParent();

            blackOverlay.gameObject.SetActive(true);
            blackOverlay.interactable = false;
            blackOverlay.blocksRaycasts = true;
        }

        if (dayText != null)
        {
            dayText.text = "";
            dayText.gameObject.SetActive(false);
        }

        float startingAlpha =
            blackOverlay != null
                ? blackOverlay.alpha
                : 0f;

        yield return FadeOverlay(
            startingAlpha,
            1f,
            fadeToBlackDuration
        );

        yield return new WaitForSecondsRealtime(
            blackScreenPause
        );

        dayNightCycle.AdvanceToNextDay(
            wakeUpTime
        );

        if (playerStamina != null)
        {
            playerStamina.RestoreAllStamina();
        }

        SaveGame();

        if (dayText != null)
        {
            dayText.text =
                "Day " + dayNightCycle.CurrentDay;

            dayText.gameObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(
            dayTextDuration
        );

        if (dayText != null)
        {
            dayText.text = "";
            dayText.gameObject.SetActive(false);
        }

        yield return FadeOverlay(
            1f,
            0f,
            fadeFromBlackDuration
        );

        if (blackOverlay != null)
        {
            blackOverlay.alpha = 0f;
            blackOverlay.blocksRaycasts = false;
            blackOverlay.interactable = false;
            blackOverlay.gameObject.SetActive(false);
        }

        EnablePlayerControls();

        transitionRunning = false;
    }

    private void DisablePlayerControls()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
        }

        if (playerStamina != null)
        {
            playerStamina.SetSprinting(false);
        }
    }

    private void EnablePlayerControls()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = true;
        }
    }

    private IEnumerator FadeOverlay(
        float startingAlpha,
        float endingAlpha,
        float duration
    )
    {
        if (blackOverlay == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            blackOverlay.alpha = endingAlpha;
            yield break;
        }

        float elapsedTime = 0f;

        blackOverlay.alpha = startingAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / duration
            );

            progress = Mathf.SmoothStep(
                0f,
                1f,
                progress
            );

            blackOverlay.alpha = Mathf.Lerp(
                startingAlpha,
                endingAlpha,
                progress
            );

            yield return null;
        }

        blackOverlay.alpha = endingAlpha;
    }

    private void ShowMessage(string message)
    {
        if (messageText == null)
        {
            Debug.Log(message);
            return;
        }

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(
            ShowMessageRoutine(message)
        );
    }

    private IEnumerator ShowMessageRoutine(
        string message
    )
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(
            messageDuration
        );

        messageText.text = "";
        messageText.gameObject.SetActive(false);

        messageCoroutine = null;
    }

    private void HideMessage()
    {
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }

        HideMessageImmediately();
    }

    private void HideMessageImmediately()
    {
        if (messageText != null)
        {
            messageText.text = "";
            messageText.gameObject.SetActive(false);
        }
    }

    private void SaveGame()
    {
        if (player != null)
        {
            PlayerPrefs.SetFloat(
                "PlayerPositionX",
                player.position.x
            );

            PlayerPrefs.SetFloat(
                "PlayerPositionY",
                player.position.y
            );

            PlayerPrefs.SetFloat(
                "PlayerPositionZ",
                player.position.z
            );

            PlayerPrefs.SetFloat(
                "PlayerRotationY",
                player.eulerAngles.y
            );
        }

        if (dayNightCycle != null)
        {
            PlayerPrefs.SetInt(
                "CurrentDay",
                dayNightCycle.CurrentDay
            );

            PlayerPrefs.SetFloat(
                "TimeOfDay",
                dayNightCycle.TimeOfDay
            );
        }

        if (playerStamina != null)
        {
            PlayerPrefs.SetFloat(
                "CurrentStamina",
                playerStamina.CurrentStamina
            );
        }

        PlayerPrefs.SetInt("HasSaveGame", 1);
        PlayerPrefs.Save();

        Debug.Log(
            "Game saved on Day " +
            dayNightCycle.CurrentDay +
            " at " +
            dayNightCycle.TimeOfDay
        );
    }
}