using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time")]
    [SerializeField, Range(0f, 24f)]
    private float timeOfDay = 8f;

    [SerializeField, Min(1f)]
    private float fullDayLength = 300f;

    [SerializeField, Min(1)]
    private int currentDay = 1;

    [SerializeField]
    private bool timeIsRunning = true;

    [Header("Sun and Moon")]
    [SerializeField] private Light sun;
    [SerializeField] private Light moon;

    [SerializeField]
    private float maximumSunIntensity = 1.2f;

    [SerializeField]
    private float maximumMoonIntensity = 0.25f;

    [Header("Sun Colors")]
    [SerializeField]
    private Color sunriseColor =
        new Color32(
            255,
            165,
            100,
            255
        );

    [SerializeField]
    private Color middayColor =
        new Color32(
            255,
            244,
            214,
            255
        );

    [SerializeField]
    private Color sunsetColor =
        new Color32(
            255,
            120,
            80,
            255
        );

    [Header("Ambient Colors")]
    [SerializeField]
    private Color nightAmbientColor =
        new Color32(
            25,
            32,
            55,
            255
        );

    [SerializeField]
    private Color dayAmbientColor =
        new Color32(
            170,
            180,
            190,
            255
        );

    [Header("Day and Night Hours")]
    [SerializeField, Range(0f, 24f)]
    private float daytimeStartsAt = 6f;

    [SerializeField, Range(0f, 24f)]
    private float nighttimeStartsAt = 20f;

    [Header("Rotation")]
    [SerializeField]
    private float sunRotationOffset = -90f;

    public float CurrentTime =>
        timeOfDay;

    public float TimeOfDay
    {
        get => timeOfDay;
        set => SetTime(value);
    }

    public int CurrentDay =>
        currentDay;

    public bool TimeIsRunning =>
        timeIsRunning;

    private void Start()
    {
        timeOfDay =
            NormalizeTime(timeOfDay);

        currentDay =
            Mathf.Max(1, currentDay);

        UpdateLighting();
    }

    private void Update()
    {
        if (timeIsRunning)
        {
            AdvanceClock();
        }

        UpdateLighting();
    }

    public bool IsNight()
    {
        return IsNight(
            nighttimeStartsAt,
            daytimeStartsAt
        );
    }

    public bool IsNight(
        float nightStartsAt,
        float nightEndsAt
    )
    {
        nightStartsAt =
            NormalizeTime(nightStartsAt);

        nightEndsAt =
            NormalizeTime(nightEndsAt);

        if (nightStartsAt > nightEndsAt)
        {
            return
                timeOfDay >= nightStartsAt ||
                timeOfDay < nightEndsAt;
        }

        return
            timeOfDay >= nightStartsAt &&
            timeOfDay < nightEndsAt;
    }

    public bool IsDay()
    {
        return !IsNight();
    }

    private void AdvanceClock()
    {
        if (fullDayLength <= 0f)
        {
            return;
        }

        float gameHoursPerSecond =
            24f / fullDayLength;

        timeOfDay +=
            gameHoursPerSecond *
            Time.deltaTime;

        while (timeOfDay >= 24f)
        {
            timeOfDay -= 24f;

            BeginNewDay();

            Debug.Log(
                $"A new day has started. " +
                $"Day {currentDay}"
            );
        }
    }

    public void SetTime(float newTime)
    {
        timeOfDay =
            NormalizeTime(newTime);

        UpdateLighting();
    }

    public void SetDay(int newDay)
    {
        currentDay =
            Mathf.Max(1, newDay);
    }

    public void SetDayAndTime(
        int newDay,
        float newTime
    )
    {
        currentDay =
            Mathf.Max(1, newDay);

        timeOfDay =
            NormalizeTime(newTime);

        UpdateLighting();
    }

    public void AdvanceTime(float hours)
    {
        if (hours < 0f)
        {
            Debug.LogWarning(
                "AdvanceTime does not accept " +
                "negative hours."
            );

            return;
        }

        timeOfDay += hours;

        while (timeOfDay >= 24f)
        {
            timeOfDay -= 24f;

            BeginNewDay();
        }

        UpdateLighting();
    }

    public void AdvanceToNextDay(
        float wakeUpTime = 8f
    )
    {
        BeginNewDay();

        timeOfDay =
            NormalizeTime(wakeUpTime);

        UpdateLighting();

        Debug.Log(
            $"Advanced to Day {currentDay} " +
            $"at {timeOfDay:0.00}."
        );
    }

    private void BeginNewDay()
    {
        ClearPreviousDayTasks();

        currentDay++;
    }

    private void ClearPreviousDayTasks()
    {
        TaskManager taskManager =
            TaskManager.Instance;

        if (taskManager == null)
        {
            taskManager =
                FindAnyObjectByType<TaskManager>();
        }

        if (taskManager == null)
        {
            Debug.LogWarning(
                "DayNightCycle could not find " +
                "TaskManager."
            );

            return;
        }

        taskManager.ClearTasksForNewDay();
    }

    public void PauseTime()
    {
        timeIsRunning = false;
    }

    public void ResumeTime()
    {
        timeIsRunning = true;
    }

    private float NormalizeTime(float time)
    {
        time %= 24f;

        if (time < 0f)
        {
            time += 24f;
        }

        return time;
    }

    private void UpdateLighting()
    {
        float daylightAmount =
            CalculateDaylightAmount(
                timeOfDay
            );

        UpdateSun(daylightAmount);
        UpdateMoon(daylightAmount);
        UpdateAmbientLight(
            daylightAmount
        );
    }

    private float CalculateDaylightAmount(
        float hour
    )
    {
        if (
            hour < 6f ||
            hour >= 18f
        )
        {
            return 0f;
        }

        float daylightProgress =
            (hour - 6f) / 12f;

        return Mathf.Sin(
            daylightProgress *
            Mathf.PI
        );
    }

    private void UpdateSun(
        float daylightAmount
    )
    {
        if (sun == null)
        {
            return;
        }

        float normalizedTime =
            timeOfDay / 24f;

        float sunAngle =
            normalizedTime *
            360f +
            sunRotationOffset;

        sun.transform.rotation =
            Quaternion.Euler(
                sunAngle,
                0f,
                0f
            );

        sun.intensity =
            daylightAmount *
            maximumSunIntensity;

        if (timeOfDay < 12f)
        {
            float morningBlend =
                Mathf.InverseLerp(
                    6f,
                    12f,
                    timeOfDay
                );

            sun.color =
                Color.Lerp(
                    sunriseColor,
                    middayColor,
                    morningBlend
                );
        }
        else
        {
            float eveningBlend =
                Mathf.InverseLerp(
                    12f,
                    18f,
                    timeOfDay
                );

            sun.color =
                Color.Lerp(
                    middayColor,
                    sunsetColor,
                    eveningBlend
                );
        }

        sun.gameObject.SetActive(
            daylightAmount > 0.001f
        );
    }

    private void UpdateMoon(
        float daylightAmount
    )
    {
        if (moon == null)
        {
            return;
        }

        float normalizedTime =
            timeOfDay / 24f;

        float moonAngle =
            normalizedTime *
            360f +
            sunRotationOffset +
            180f;

        moon.transform.rotation =
            Quaternion.Euler(
                moonAngle,
                0f,
                0f
            );

        float nightAmount =
            1f - daylightAmount;

        moon.intensity =
            nightAmount *
            maximumMoonIntensity;

        moon.gameObject.SetActive(
            nightAmount > 0.001f
        );
    }

    private void UpdateAmbientLight(
        float daylightAmount
    )
    {
        RenderSettings.ambientLight =
            Color.Lerp(
                nightAmbientColor,
                dayAmbientColor,
                daylightAmount
            );
    }
}