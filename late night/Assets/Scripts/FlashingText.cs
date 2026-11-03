using TMPro;
using UnityEngine;

public class FlashingText : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private float flashSpeed = 2f;

    [Range(0f, 1f)]
    [SerializeField] private float minimumAlpha = 0.15f;

    [Range(0f, 1f)]
    [SerializeField] private float maximumAlpha = 1f;

    private Color originalColor;

    private void Awake()
    {
        if (text == null)
        {
            text = GetComponent<TMP_Text>();
        }

        originalColor = text.color;
    }

    private void OnEnable()
    {
        SetAlpha(maximumAlpha);
    }

    private void Update()
    {
        float alpha = Mathf.Lerp(
            minimumAlpha,
            maximumAlpha,
            (Mathf.Sin(Time.unscaledTime * flashSpeed) + 1f) / 2f
        );

        SetAlpha(alpha);
    }

    private void OnDisable()
    {
        SetAlpha(maximumAlpha);
    }

    private void SetAlpha(float alpha)
    {
        if (text == null)
            return;

        Color color = originalColor;
        color.a = alpha;
        text.color = color;
    }
}