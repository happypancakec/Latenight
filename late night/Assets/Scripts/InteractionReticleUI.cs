using UnityEngine;
using UnityEngine.UI;

public class InteractionReticleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Image reticleImage;

    [Header("Appearance")]
    [SerializeField]
    private Color normalColor =
        new Color(1f, 1f, 1f, 0.5f);

    [SerializeField]
    private Color interactableColor =
        new Color(1f, 0.85f, 0.2f, 1f);

    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float interactableScale = 1.4f;
    [SerializeField] private float transitionSpeed = 10f;

    [Header("Glow")]
    [SerializeField] private bool pulseWhenInteractable = true;
    [SerializeField] private float pulseAmount = 0.1f;
    [SerializeField] private float pulseSpeed = 5f;

    private void Update()
    {
        if (
            playerInteraction == null ||
            reticleImage == null
        )
        {
            return;
        }

        bool canInteract =
            playerInteraction.IsLookingAtInteractable;

        Color targetColor =
            canInteract ? interactableColor : normalColor;

        reticleImage.color = Color.Lerp(
            reticleImage.color,
            targetColor,
            transitionSpeed * Time.deltaTime
        );

        float targetScale =
            canInteract ? interactableScale : normalScale;

        if (canInteract && pulseWhenInteractable)
        {
            targetScale +=
                Mathf.Sin(Time.time * pulseSpeed) *
                pulseAmount;
        }

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            Vector3.one * targetScale,
            transitionSpeed * Time.deltaTime
        );
    }
}
