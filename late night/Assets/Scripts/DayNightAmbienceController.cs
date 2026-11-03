using UnityEngine;
using UnityEngine.SceneManagement;

public class DayNightAmbienceController : MonoBehaviour
{
    [Header("Game Time")]
    [SerializeField] private DayNightCycle dayNightCycle;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource dayAmbience;
    [SerializeField] private AudioSource nightAmbience;

    [Header("Time Settings")]
    [SerializeField] private float daytimeStartsAt = 6f;
    [SerializeField] private float nighttimeStartsAt = 20f;

    [Header("Computer")]
    [SerializeField]
    private string computerSceneName =
        "ComputerScene";

    [SerializeField]
    private bool muteWhileUsingComputer = true;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)]
    private float maximumDayVolume = 0.4f;

    [SerializeField, Range(0f, 1f)]
    private float maximumNightVolume = 0.45f;

    [SerializeField, Min(0.1f)]
    private float fadeDuration = 4f;

    [SerializeField, Min(0.1f)]
    private float computerFadeDuration = 1f;

    private void Start()
    {
        if (dayNightCycle == null)
        {
            dayNightCycle =
                FindAnyObjectByType<DayNightCycle>();
        }

        PrepareAudioSource(dayAmbience);
        PrepareAudioSource(nightAmbience);

        SetStartingVolumes();
    }

    private void Update()
    {
        if (dayNightCycle == null)
        {
            dayNightCycle =
                FindAnyObjectByType<DayNightCycle>();

            if (dayNightCycle == null)
            {
                return;
            }
        }

        bool computerIsOpen =
            muteWhileUsingComputer &&
            IsComputerSceneLoaded();

        if (computerIsOpen)
        {
            FadeAudio(
                dayAmbience,
                0f,
                maximumDayVolume,
                computerFadeDuration
            );

            FadeAudio(
                nightAmbience,
                0f,
                maximumNightVolume,
                computerFadeDuration
            );

            return;
        }

        bool isDaytime = IsDaytime(
            dayNightCycle.CurrentTime
        );

        float targetDayVolume =
            isDaytime ? maximumDayVolume : 0f;

        float targetNightVolume =
            isDaytime ? 0f : maximumNightVolume;

        FadeAudio(
            dayAmbience,
            targetDayVolume,
            maximumDayVolume,
            fadeDuration
        );

        FadeAudio(
            nightAmbience,
            targetNightVolume,
            maximumNightVolume,
            fadeDuration
        );
    }

    private void PrepareAudioSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;

        if (!source.isPlaying)
        {
            source.Play();
        }
    }

    private void SetStartingVolumes()
    {
        if (dayNightCycle == null)
        {
            return;
        }

        if (muteWhileUsingComputer &&
            IsComputerSceneLoaded())
        {
            if (dayAmbience != null)
            {
                dayAmbience.volume = 0f;
            }

            if (nightAmbience != null)
            {
                nightAmbience.volume = 0f;
            }

            return;
        }

        bool isDaytime = IsDaytime(
            dayNightCycle.CurrentTime
        );

        if (dayAmbience != null)
        {
            dayAmbience.volume =
                isDaytime ? maximumDayVolume : 0f;
        }

        if (nightAmbience != null)
        {
            nightAmbience.volume =
                isDaytime ? 0f : maximumNightVolume;
        }
    }

    private bool IsComputerSceneLoaded()
    {
        Scene computerScene =
            SceneManager.GetSceneByName(
                computerSceneName
            );

        return
            computerScene.IsValid() &&
            computerScene.isLoaded;
    }

    private bool IsDaytime(float currentTime)
    {
        return
            currentTime >= daytimeStartsAt &&
            currentTime < nighttimeStartsAt;
    }

    private void FadeAudio(
        AudioSource source,
        float targetVolume,
        float maximumVolume,
        float duration
    )
    {
        if (source == null)
        {
            return;
        }

        float safeDuration =
            Mathf.Max(0.1f, duration);

        float fadeSpeed =
            maximumVolume / safeDuration;

        source.volume = Mathf.MoveTowards(
            source.volume,
            targetVolume,
            fadeSpeed * Time.unscaledDeltaTime
        );
    }
}