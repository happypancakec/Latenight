using System.Collections;
using UnityEngine;
using TMPro;

public class OpeningCutscene : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;

        [TextArea(2, 5)]
        public string dialogue;
    }

    [Header("Car")]
    [SerializeField] private Transform car;
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float drivingSpeed = 6f;
    [SerializeField] private float turningSpeed = 4f;

    [Header("Car Grounding")]
    [SerializeField] private Transform carGroundCheck;
    [SerializeField] private LayerMask roadLayers;
    [SerializeField] private float groundRayHeight = 2f;
    [SerializeField] private float groundRayDistance = 5f;

    [Header("Car Headlights")]
    [SerializeField] private Light[] carHeadlights;
    [SerializeField] private bool headlightsEnabled = true;
    [SerializeField] private bool keepHeadlightsOnAfterCutscene = true;

    [Header("Dialogue")]
    [SerializeField] private DialogueLine[] dialogueLines;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueText;
    [SerializeField] private GameObject dialoguePanel;

    [Header("Typewriter")]
    [SerializeField] private float charactersPerSecond = 35f;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Tooltip("How many seconds it takes to fade into the cutscene.")]
    [SerializeField] private float fadeInDuration = 20f;

    [Tooltip("How many seconds it takes to fade out of the cutscene.")]
    [SerializeField] private float fadeOutDuration = 20f;

    [SerializeField] private bool fadeIntoGameplay = true;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject staminaBar;

    [Header("Cameras")]
    [SerializeField] private Camera cutsceneCamera;
    [SerializeField] private Camera gameplayCamera;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private PlayerInteraction playerInteraction;

    private int currentWaypoint;
    private int currentDialogueLine;

    private bool finishedDriving;
    private bool finishedDialogue;
    private bool cutsceneEnded;
    private bool endingCutscene;
    private bool isTyping;

    private bool staminaBarWasActive;

    private Coroutine typingCoroutine;

    private void Start()
    {
        BeginCutscene();
    }

    private void Update()
    {
        if (cutsceneEnded)
        {
            return;
        }

        // The car moves while the dialogue and fades are happening.
        MoveCar();

        if (!endingCutscene)
        {
            UpdateContinueText();

            if (
                Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.Space)
            )
            {
                HandleDialogueInput();
            }
        }

        // End immediately when the final dialogue line finishes.
        if (finishedDialogue && !endingCutscene)
        {
            StartCoroutine(EndCutsceneSequence());
        }
    }

    private void BeginCutscene()
    {
        if (cutsceneCamera != null)
        {
            cutsceneCamera.enabled = true;
        }

        if (gameplayCamera != null)
        {
            gameplayCamera.enabled = false;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
        }

        if (staminaBar != null)
        {
            staminaBarWasActive = staminaBar.activeSelf;
            staminaBar.SetActive(false);
        }

        if (carRigidbody == null && car != null)
        {
            carRigidbody = car.GetComponent<Rigidbody>();
        }

        // The script controls the car instead of physics.
        if (carRigidbody != null)
        {
            carRigidbody.isKinematic = true;
        }

        SetHeadlights(headlightsEnabled);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentWaypoint = 0;
        currentDialogueLine = 0;

        finishedDriving =
            car == null ||
            waypoints == null ||
            waypoints.Length == 0;

        finishedDialogue =
            dialogueLines == null ||
            dialogueLines.Length == 0;

        if (car == null)
        {
            Debug.LogError(
                "OpeningCutscene: The Car field is empty."
            );
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError(
                "OpeningCutscene: No waypoints were assigned."
            );
        }

        if (carGroundCheck == null)
        {
            Debug.LogWarning(
                "OpeningCutscene: Car Ground Check is not assigned."
            );
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(!finishedDialogue);
        }

        SetContinueTextVisible(false);

        if (!finishedDialogue)
        {
            DisplayCurrentDialogue();
        }

        // Place the car on the road before it begins moving.
        SnapCarToGround();

        // Start with a black screen and slowly fade in.
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = false;

            StartCoroutine(
                FadeCanvas(
                    1f,
                    0f,
                    fadeInDuration
                )
            );
        }
    }

    private void MoveCar()
    {
        if (
            finishedDriving ||
            car == null ||
            waypoints == null ||
            waypoints.Length == 0
        )
        {
            return;
        }

        // Skip any empty waypoint fields.
        while (
            currentWaypoint < waypoints.Length &&
            waypoints[currentWaypoint] == null
        )
        {
            currentWaypoint++;
        }

        if (currentWaypoint >= waypoints.Length)
        {
            finishedDriving = true;
            return;
        }

        Transform target = waypoints[currentWaypoint];

        Vector3 direction =
            target.position - car.position;

        // Prevent the car from pointing upward or downward.
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            car.rotation = Quaternion.Slerp(
                car.rotation,
                targetRotation,
                turningSpeed * Time.deltaTime
            );
        }

        // Only the Ground Check controls the car's height.
        Vector3 horizontalTarget = target.position;
        horizontalTarget.y = car.position.y;

        car.position = Vector3.MoveTowards(
            car.position,
            horizontalTarget,
            drivingSpeed * Time.deltaTime
        );

        SnapCarToGround();

        // Compare horizontal positions so waypoint height
        // cannot prevent the car from reaching it.
        Vector3 carFlatPosition = car.position;
        Vector3 targetFlatPosition = target.position;

        carFlatPosition.y = 0f;
        targetFlatPosition.y = 0f;

        if (
            Vector3.Distance(
                carFlatPosition,
                targetFlatPosition
            ) <= 0.05f
        )
        {
            currentWaypoint++;

            if (currentWaypoint >= waypoints.Length)
            {
                finishedDriving = true;
            }
        }
    }

    private void SnapCarToGround()
    {
        if (car == null || carGroundCheck == null)
        {
            return;
        }

        Vector3 rayOrigin =
            carGroundCheck.position +
            Vector3.up * groundRayHeight;

        float totalRayDistance =
            groundRayHeight + groundRayDistance;

        if (
            Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit hit,
                totalRayDistance,
                roadLayers,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            float heightDifference =
                hit.point.y -
                carGroundCheck.position.y;

            car.position +=
                Vector3.up * heightDifference;
        }
    }

    private void HandleDialogueInput()
    {
        if (finishedDialogue)
        {
            return;
        }

        // The first click reveals a line that is still typing.
        if (isTyping)
        {
            FinishTypingImmediately();
            return;
        }

        // The next click advances to the following line.
        currentDialogueLine++;

        if (currentDialogueLine >= dialogueLines.Length)
        {
            finishedDialogue = true;
            SetContinueTextVisible(false);
            return;
        }

        DisplayCurrentDialogue();
    }

    private void DisplayCurrentDialogue()
    {
        DialogueLine line =
            dialogueLines[currentDialogueLine];

        if (speakerText != null)
        {
            speakerText.text = line.speaker;
        }

        if (dialogueText == null)
        {
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        SetContinueTextVisible(false);

        typingCoroutine = StartCoroutine(
            TypeDialogue(line.dialogue)
        );
    }

    private IEnumerator TypeDialogue(string dialogue)
    {
        isTyping = true;

        dialogueText.text = dialogue;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalCharacters =
            dialogueText.textInfo.characterCount;

        float visibleCharacterCount = 0f;

        while (
            dialogueText.maxVisibleCharacters <
            totalCharacters
        )
        {
            visibleCharacterCount +=
                charactersPerSecond *
                Time.unscaledDeltaTime;

            dialogueText.maxVisibleCharacters =
                Mathf.Min(
                    Mathf.FloorToInt(
                        visibleCharacterCount
                    ),
                    totalCharacters
                );

            yield return null;
        }

        dialogueText.maxVisibleCharacters =
            totalCharacters;

        isTyping = false;
        typingCoroutine = null;

        CompleteCurrentDialogueLine();
    }

    private void FinishTypingImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters =
                dialogueText.textInfo.characterCount;
        }

        isTyping = false;

        CompleteCurrentDialogueLine();
    }

    private void CompleteCurrentDialogueLine()
    {
        bool isFinalLine =
            currentDialogueLine >=
            dialogueLines.Length - 1;

        if (isFinalLine)
        {
            // The final line automatically starts the ending.
            finishedDialogue = true;
            SetContinueTextVisible(false);
        }
        else
        {
            SetContinueTextVisible(true);
        }
    }

    private void SetContinueTextVisible(bool visible)
    {
        if (continueText != null)
        {
            continueText.gameObject.SetActive(visible);
        }
    }

    private void UpdateContinueText()
    {
        if (
            continueText == null ||
            !continueText.gameObject.activeSelf
        )
        {
            return;
        }

        continueText.alpha =
            0.65f +
            Mathf.Sin(
                Time.unscaledTime * 4f
            ) * 0.25f;
    }

    private void SetHeadlights(bool enabled)
    {
        if (carHeadlights == null)
        {
            return;
        }

        foreach (Light headlight in carHeadlights)
        {
            if (headlight != null)
            {
                headlight.enabled = enabled;
            }
        }
    }

    private IEnumerator EndCutsceneSequence()
    {
        endingCutscene = true;

        SetContinueTextVisible(false);

        // Slowly fade the cutscene to black.
        if (fadeCanvasGroup != null)
        {
            yield return FadeCanvas(
                fadeCanvasGroup.alpha,
                1f,
                fadeOutDuration
            );
        }

        SwitchToGameplay();

        // Fade from black back into gameplay.
        if (
            fadeCanvasGroup != null &&
            fadeIntoGameplay
        )
        {
            yield return FadeCanvas(
                1f,
                0f,
                fadeInDuration
            );
        }

        cutsceneEnded = true;
    }

    private void SwitchToGameplay()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (player != null && playerSpawnPoint != null)
        {
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            player.position =
                playerSpawnPoint.position;

            player.rotation = Quaternion.Euler(
                0f,
                playerSpawnPoint.eulerAngles.y,
                0f
            );

            if (characterController != null)
            {
                characterController.enabled = true;
            }
        }

        if (cutsceneCamera != null)
        {
            cutsceneCamera.enabled = false;
        }

        if (gameplayCamera != null)
        {
            gameplayCamera.enabled = true;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = true;
        }

        if (staminaBar != null)
        {
            staminaBar.SetActive(
                staminaBarWasActive
            );
        }

        if (!keepHeadlightsOnAfterCutscene)
        {
            SetHeadlights(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator FadeCanvas(
        float startingAlpha,
        float endingAlpha,
        float duration
    )
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            fadeCanvasGroup.alpha =
                endingAlpha;

            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime +=
                Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / duration
            );

            progress = Mathf.SmoothStep(
                0f,
                1f,
                progress
            );

            fadeCanvasGroup.alpha =
                Mathf.Lerp(
                    startingAlpha,
                    endingAlpha,
                    progress
                );

            yield return null;
        }

        fadeCanvasGroup.alpha = endingAlpha;
    }
}