using UnityEngine;
using UnityEngine.UI;

public class ComputerScreenEffect : MonoBehaviour
{
    [Header("Screen Tint")]
    [SerializeField] private Image screenTint;

    [Header("Flicker")]
    [SerializeField] private float minimumAlpha = 0.055f;
    [SerializeField] private float maximumAlpha = 0.085f;
    [SerializeField] private float flickerSpeed = 8f;

    private Color tintColor;
    private float flickerOffset;

    private void Awake()
    {
        if (screenTint == null)
        {
            return;
        }

        tintColor = screenTint.color;
        flickerOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (screenTint == null)
        {
            return;
        }

        float noise = Mathf.PerlinNoise(
            flickerOffset,
            Time.unscaledTime * flickerSpeed
        );

        tintColor.a = Mathf.Lerp(
            minimumAlpha,
            maximumAlpha,
            noise
        );

        screenTint.color = tintColor;
    }
}