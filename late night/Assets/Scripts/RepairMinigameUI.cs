using System;
using TMPro;
using UnityEngine;

public class RepairMinigameUI : MonoBehaviour
{
    [Header("Minigame UI")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private RectTransform movingMarker;
    [SerializeField] private RectTransform successZone;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text resultText;

    [Header("Minigame Settings")]
    [SerializeField] private float markerSpeed = 0.65f;

    [SerializeField, Range(0.05f, 0.75f)]
    private float successZoneWidth = 0.22f;

    [SerializeField] private int successesRequired = 3;

    [Header("Player")]
    [SerializeField] private Behaviour playerController;

    [Header("Minigame Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip successfulHitSound;

    [SerializeField, Range(0f, 1f)]
    private float successfulHitVolume = 0.7f;

    [Header("Completion Celebration")]
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TMP_Text completionTitleText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private ParticleSystem confettiParticles;
    [SerializeField] private AudioClip completionSound;

    [SerializeField, Range(0f, 1f)]
    private float completionSoundVolume = 0.9f;

    private int successfulHits;
    private bool minigameActive;
    private bool completionMessageOpen;

    private float zoneCenter;
    private float markerTimer;

    private Action successCallback;
    private Action cancelCallback;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }

        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (completionMessageOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.Return))
            {
                CloseCompletionMessage();
            }

            return;
        }

        if (!minigameActive)
        {
            return;
        }

        markerTimer += Time.unscaledDeltaTime;

        float markerPosition = Mathf.PingPong(
            markerTimer * markerSpeed,
            1f
        );

        SetHorizontalPosition(
            movingMarker,
            markerPosition,
            0.035f
        );

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckTiming(markerPosition);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelMinigame();
        }
    }

    public void BeginMinigame(
        Action onSuccess,
        Action onCancelled
    )
    {
        successCallback = onSuccess;
        cancelCallback = onCancelled;

        successfulHits = 0;
        markerTimer = 0f;
        minigameActive = true;
        completionMessageOpen = false;

        MoveSuccessZone();

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(true);
        }

        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }

        SetPlayerControl(false);

        if (resultText != null)
        {
            resultText.text = "";
        }

        RefreshProgress();
    }

    private void CheckTiming(float markerPosition)
    {
        float distance = Mathf.Abs(
            markerPosition - zoneCenter
        );

        bool successful =
            distance <= successZoneWidth * 0.5f;

        if (successful)
        {
            successfulHits++;
            PlaySuccessfulHitSound();

            if (resultText != null)
            {
                resultText.text = "Good!";

                resultText.color = new Color32(
                    70,
                    220,
                    105,
                    255
                );
            }

            RefreshProgress();

            if (successfulHits >= successesRequired)
            {
                FinishMinigame();
                return;
            }

            MoveSuccessZone();
        }
        else if (resultText != null)
        {
            resultText.text = "Missed—try again!";

            resultText.color = new Color32(
                225,
                70,
                70,
                255
            );
        }
    }

    private void MoveSuccessZone()
    {
        float minimumCenter =
            0.1f + successZoneWidth * 0.5f;

        float maximumCenter =
            0.9f - successZoneWidth * 0.5f;

        zoneCenter = UnityEngine.Random.Range(
            minimumCenter,
            maximumCenter
        );

        SetHorizontalPosition(
            successZone,
            zoneCenter,
            successZoneWidth
        );
    }

    private void PlaySuccessfulHitSound()
    {
        if (audioSource == null ||
            successfulHitSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            successfulHitSound,
            successfulHitVolume
        );
    }

    private void FinishMinigame()
    {
        minigameActive = false;

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }

        Action callback = successCallback;

        successCallback = null;
        cancelCallback = null;

        callback?.Invoke();
    }

    private void CancelMinigame()
    {
        minigameActive = false;

        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }

        successCallback = null;

        Action callback = cancelCallback;
        cancelCallback = null;

        SetPlayerControl(true);
        LockGameplayCursor();

        callback?.Invoke();
    }

    public void ShowCompletionCelebration(int moneyEarned)
    {
        completionMessageOpen = true;

        if (completionTitleText != null)
        {
            completionTitleText.text = "YOU DID IT!";
        }

        if (rewardText != null)
        {
            rewardText.text =
                $"You earned ${moneyEarned}!";
        }

        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
            completionPanel.transform.SetAsLastSibling();
        }

        if (confettiParticles != null)
        {
            confettiParticles.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );

            confettiParticles.Play();
        }

        if (audioSource != null &&
            completionSound != null)
        {
            audioSource.PlayOneShot(
                completionSound,
                completionSoundVolume
            );
        }

        SetPlayerControl(false);
        UnlockCursor();
    }

    public void CloseCompletionMessage()
    {
        if (!completionMessageOpen)
        {
            return;
        }

        completionMessageOpen = false;

        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }

        if (confettiParticles != null)
        {
            confettiParticles.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmitting
            );
        }

        SetPlayerControl(true);
        LockGameplayCursor();
    }

    private void SetPlayerControl(bool enabled)
    {
        if (playerController != null)
        {
            playerController.enabled = enabled;
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockGameplayCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void RefreshProgress()
    {
        if (progressText == null)
        {
            return;
        }

        progressText.text =
            $"Successful repairs: {successfulHits}/" +
            $"{successesRequired}";
    }

    private void SetHorizontalPosition(
        RectTransform target,
        float center,
        float width
    )
    {
        if (target == null)
        {
            return;
        }

        float halfWidth = width * 0.5f;

        target.anchorMin = new Vector2(
            center - halfWidth,
            0f
        );

        target.anchorMax = new Vector2(
            center + halfWidth,
            1f
        );

        target.offsetMin = Vector2.zero;
        target.offsetMax = Vector2.zero;
    }
}