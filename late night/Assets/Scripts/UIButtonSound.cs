using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField, Range(0f, 1f)]
    private float volume = 0.6f;

    private Button button;
    private AudioSource audioSource;

    private void Awake()
    {
        button = GetComponent<Button>();

        audioSource =
            GetComponentInParent<ComputerUIController>()
                ?.GetComponent<AudioSource>();

        button.onClick.AddListener(PlayClickSound);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    private void PlayClickSound()
    {
        if (audioSource == null || clickSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(clickSound, volume);
    }
}