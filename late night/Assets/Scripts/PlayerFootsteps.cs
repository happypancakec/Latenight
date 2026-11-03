using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Player")]
    [SerializeField]
    private CharacterController characterController;

    [Header("Interior Detection")]
    [SerializeField]
    private BoxCollider houseInteriorZone;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip insideHouseSound;
    [SerializeField] private AudioClip outsideHouseSound;

    [Header("Footstep Timing")]
    [SerializeField] private float walkingInterval = 0.5f;
    [SerializeField] private float runningInterval = 0.32f;
    [SerializeField] private float runningSpeedThreshold = 5.5f;
    [SerializeField] private float minimumMovementSpeed = 0.15f;

    [Header("Sound Variation")]
    [SerializeField, Range(0f, 1f)]
    private float volume = 0.65f;

    [SerializeField]
    private float minimumPitch = 0.95f;

    [SerializeField]
    private float maximumPitch = 1.05f;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugMessages = true;

    private float stepTimer;
    private bool isInsideHouse;

    public bool IsInsideHouse => isInsideHouse;

    private void Awake()
    {
        if (characterController == null)
        {
            characterController =
                GetComponent<CharacterController>();
        }

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        stepTimer = 0f;

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        UpdateInteriorState();
    }

    private void Update()
    {
        UpdateInteriorState();
        UpdateFootsteps();
    }

    private void UpdateInteriorState()
    {
        if (houseInteriorZone == null)
        {
            return;
        }

        Vector3 localPlayerPosition =
            houseInteriorZone.transform
                .InverseTransformPoint(
                    transform.position
                );

        Vector3 localCenter =
            houseInteriorZone.center;

        Vector3 halfSize =
            houseInteriorZone.size * 0.5f;

        Vector3 difference =
            localPlayerPosition - localCenter;

        bool currentlyInside =
            Mathf.Abs(difference.x) <= halfSize.x &&
            Mathf.Abs(difference.z) <= halfSize.z;

        if (currentlyInside == isInsideHouse)
        {
            return;
        }

        isInsideHouse = currentlyInside;

        StopCurrentFootsteps();

        if (showDebugMessages)
        {
            Debug.Log(
                isInsideHouse
                    ? "Player is inside the house. " +
                      "Using inside footsteps."
                    : "Player is outside the house. " +
                      "Using outside footsteps."
            );
        }
    }

    private void UpdateFootsteps()
    {
        if (characterController == null ||
            audioSource == null)
        {
            return;
        }

        Vector3 horizontalVelocity =
            characterController.velocity;

        horizontalVelocity.y = 0f;

        float movementSpeed =
            horizontalVelocity.magnitude;

        bool playerIsMoving =
            characterController.isGrounded &&
            movementSpeed > minimumMovementSpeed;

        if (!playerIsMoving)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer > 0f)
        {
            return;
        }

        PlayFootstep();

        bool playerIsRunning =
            movementSpeed >= runningSpeedThreshold;

        stepTimer = playerIsRunning
            ? runningInterval
            : walkingInterval;
    }

    private void PlayFootstep()
    {
        AudioClip selectedSound = isInsideHouse
            ? insideHouseSound
            : outsideHouseSound;

        if (selectedSound == null)
        {
            if (showDebugMessages)
            {
                Debug.LogWarning(
                    isInsideHouse
                        ? "Inside footstep sound is missing."
                        : "Outside footstep sound is missing."
                );
            }

            return;
        }

        audioSource.pitch = Random.Range(
            minimumPitch,
            maximumPitch
        );

        audioSource.PlayOneShot(
            selectedSound,
            volume
        );
    }

    private void StopCurrentFootsteps()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        stepTimer = 0f;
    }
}