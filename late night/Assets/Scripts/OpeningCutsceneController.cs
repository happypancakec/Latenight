using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpeningCutsceneController : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneShot
    {
        [Header("Image")]
        public Sprite image;

        [Header("Dialogue")]
        public string speaker;

        [TextArea(3, 8)]
        public string dialogue;

        [Header("Camera Movement")]
        public Vector2 startPosition;
        public Vector2 endPosition;

        [Min(0.1f)]
        public float startZoom = 1f;

        [Min(0.1f)]
        public float endZoom = 1.08f;

        [Min(0.1f)]
        public float movementDuration = 6f;
    }

    [Header("Cutscene Shots")]
    public CutsceneShot[] shots;

    [Header("UI References")]
    public Image cutsceneImage;
    public RectTransform cutsceneImageRect;
    public TMP_Text speakerText;
    public TMP_Text dialogueText;
    public TMP_Text continueText;
    public CanvasGroup fadeOverlay;

    [Header("Dialogue Settings")]
    [Min(0.001f)]
    public float typingSpeed = 0.035f;

    public string continueMessage = "Click to continue";

    [Header("Fade Settings")]
    [Min(0f)]
    public float fadeInDuration = 2f;

    [Min(0f)]
    public float fadeOutDuration = 2f;

    [Header("Scene Settings")]
    public string gameplaySceneName = "SampleScene";

    private int currentShotIndex;
    private bool isTyping;
    private bool canAdvance;
    private bool cutsceneEnding;

    private string currentFullDialogue;

    private Coroutine typingCoroutine;
    private Coroutine movementCoroutine;

    private void Start()
    {
        if (cutsceneImageRect == null && cutsceneImage != null)
        {
            cutsceneImageRect = cutsceneImage.rectTransform;
        }

        if (continueText != null)
        {
            continueText.text = continueMessage;
            continueText.gameObject.SetActive(false);
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 1f;
            fadeOverlay.blocksRaycasts = true;
        }

        StartCoroutine(BeginCutscene());
    }

    private void Update()
    {
        if (cutsceneEnding)
            return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            HandleAdvanceInput();
        }
    }

    private IEnumerator BeginCutscene()
    {
        if (shots == null || shots.Length == 0)
        {
            Debug.LogError("The opening cutscene has no shots assigned.");
            yield break;
        }

        currentShotIndex = 0;
        DisplayShot(currentShotIndex);

        yield return Fade(1f, 0f, fadeInDuration);

        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = false;
        }

        StartDialogue();
    }

    private void DisplayShot(int shotIndex)
    {
        if (shotIndex < 0 || shotIndex >= shots.Length)
            return;

        CutsceneShot shot = shots[shotIndex];

        if (cutsceneImage != null)
        {
            cutsceneImage.sprite = shot.image;
            cutsceneImage.color = Color.white;
        }

        if (speakerText != null)
        {
            speakerText.text = shot.speaker;
            speakerText.gameObject.SetActive(
                !string.IsNullOrWhiteSpace(shot.speaker)
            );
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (continueText != null)
        {
            continueText.gameObject.SetActive(false);
        }

        if (cutsceneImageRect != null)
        {
            cutsceneImageRect.anchoredPosition = shot.startPosition;
            cutsceneImageRect.localScale =
                Vector3.one * shot.startZoom;
        }

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        movementCoroutine = StartCoroutine(
            AnimateShotMovement(shot)
        );
    }

    private void StartDialogue()
    {
        if (currentShotIndex < 0 ||
            currentShotIndex >= shots.Length)
        {
            return;
        }

        currentFullDialogue = shots[currentShotIndex].dialogue;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(
            TypeDialogue(currentFullDialogue)
        );
    }

    private IEnumerator TypeDialogue(string textToType)
    {
        isTyping = true;
        canAdvance = false;

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (continueText != null)
        {
            continueText.gameObject.SetActive(false);
        }

        foreach (char character in textToType)
        {
            if (dialogueText != null)
            {
                dialogueText.text += character;
            }

            if (character == '.' ||
                character == '!' ||
                character == '?')
            {
                yield return new WaitForSecondsRealtime(
                    typingSpeed * 4f
                );
            }
            else if (character == ',')
            {
                yield return new WaitForSecondsRealtime(
                    typingSpeed * 2f
                );
            }
            else
            {
                yield return new WaitForSecondsRealtime(
                    typingSpeed
                );
            }
        }

        FinishTyping();
    }

    private void HandleAdvanceInput()
    {
        if (isTyping)
        {
            CompleteCurrentDialogueImmediately();
            return;
        }

        if (!canAdvance)
            return;

        AdvanceToNextShot();
    }

    private void CompleteCurrentDialogueImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = currentFullDialogue;
        }

        FinishTyping();
    }

    private void FinishTyping()
    {
        isTyping = false;
        canAdvance = true;

        if (continueText != null)
        {
            bool finalShot =
                currentShotIndex >= shots.Length - 1;

            continueText.text = finalShot
                ? "Click to begin"
                : continueMessage;

            continueText.gameObject.SetActive(true);
        }
    }

    private void AdvanceToNextShot()
    {
        canAdvance = false;

        if (continueText != null)
        {
            continueText.gameObject.SetActive(false);
        }

        if (currentShotIndex >= shots.Length - 1)
        {
            StartCoroutine(EndCutscene());
            return;
        }

        currentShotIndex++;

        DisplayShot(currentShotIndex);
        StartDialogue();
    }

    private IEnumerator AnimateShotMovement(CutsceneShot shot)
    {
        if (cutsceneImageRect == null)
            yield break;

        float elapsedTime = 0f;
        float duration = Mathf.Max(
            0.1f,
            shot.movementDuration
        );

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float amount = Mathf.Clamp01(
                elapsedTime / duration
            );

            amount = Mathf.SmoothStep(0f, 1f, amount);

            cutsceneImageRect.anchoredPosition =
                Vector2.Lerp(
                    shot.startPosition,
                    shot.endPosition,
                    amount
                );

            float currentZoom = Mathf.Lerp(
                shot.startZoom,
                shot.endZoom,
                amount
            );

            cutsceneImageRect.localScale =
                Vector3.one * currentZoom;

            yield return null;
        }

        cutsceneImageRect.anchoredPosition =
            shot.endPosition;

        cutsceneImageRect.localScale =
            Vector3.one * shot.endZoom;
    }

    private IEnumerator EndCutscene()
    {
        cutsceneEnding = true;

        if (continueText != null)
        {
            continueText.gameObject.SetActive(false);
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
        }

        yield return Fade(0f, 1f, fadeOutDuration);

        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError(
                "No gameplay scene name was entered."
            );

            yield break;
        }
        if (SceneManager.sceneCountInBuildSettings > 1)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            Debug.LogError(
                "The gameplay scene is missing from the Build Profile."
            );
        }

       
    }

    private IEnumerator Fade(
        float startingAlpha,
        float endingAlpha,
        float duration
    )
    {
        if (fadeOverlay == null)
            yield break;

        if (duration <= 0f)
        {
            fadeOverlay.alpha = endingAlpha;
            yield break;
        }

        float elapsedTime = 0f;
        fadeOverlay.alpha = startingAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float amount = Mathf.Clamp01(
                elapsedTime / duration
            );

            fadeOverlay.alpha = Mathf.Lerp(
                startingAlpha,
                endingAlpha,
                amount
            );

            yield return null;
        }

        fadeOverlay.alpha = endingAlpha;
    }
}