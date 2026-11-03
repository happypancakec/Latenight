using UnityEngine;

public class GlowPulse : MonoBehaviour
{
    [SerializeField] private Light glowLight;
    [SerializeField] private float minimumIntensity = 0.7f;
    [SerializeField] private float maximumIntensity = 2f;
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Optional Movement")]
    [SerializeField] private bool floatUpAndDown = false;
    [SerializeField] private float floatHeight = 0.15f;
    [SerializeField] private float floatSpeed = 1.5f;

    private Vector3 startingPosition;

    private void Awake()
    {
        if (glowLight == null)
        {
            glowLight = GetComponent<Light>();
        }

        startingPosition = transform.localPosition;
    }

    private void Update()
    {
        float pulse = (
            Mathf.Sin(Time.time * pulseSpeed) + 1f
        ) * 0.5f;

        if (glowLight != null)
        {
            glowLight.intensity = Mathf.Lerp(
                minimumIntensity,
                maximumIntensity,
                pulse
            );
        }

        if (floatUpAndDown)
        {
            float offset =
                Mathf.Sin(Time.time * floatSpeed) *
                floatHeight;

            transform.localPosition =
                startingPosition + Vector3.up * offset;
        }
    }
}