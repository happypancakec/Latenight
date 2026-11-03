using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private PlayerStamina playerStamina;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 200f;
    [SerializeField] private float lookLimit = 85f;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;
    private float playerYaw;

    public bool IsSprinting { get; private set; }

    private void Awake()
    {
        controller =
            GetComponent<CharacterController>();

        if (playerStamina == null)
        {
            playerStamina =
                GetComponent<PlayerStamina>();
        }

        // Prevent a positive Inspector value from
        // launching the player upward.
        gravity = -Mathf.Abs(gravity);

        if (Mathf.Approximately(gravity, 0f))
        {
            gravity = -20f;
        }

        playerYaw = transform.eulerAngles.y;

        if (playerCamera != null)
        {
            Vector3 cameraEuler =
                playerCamera.localEulerAngles;

            cameraPitch =
                NormalizeAngle(cameraEuler.x);
        }

        transform.rotation = Quaternion.Euler(
            0f,
            playerYaw,
            0f
        );

        LockCursor();
    }

    private void OnDisable()
    {
        IsSprinting = false;
        verticalVelocity = 0f;

        if (playerStamina != null)
        {
            playerStamina.SetSprinting(false);
        }
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleCursor();
    }

    private void HandleMouseLook()
    {
        if (playerCamera == null)
        {
            return;
        }

        float mouseX =
            Input.GetAxisRaw("Mouse X") *
            mouseSensitivity *
            Time.deltaTime;

        float mouseY =
            Input.GetAxisRaw("Mouse Y") *
            mouseSensitivity *
            Time.deltaTime;

        playerYaw += mouseX;
        cameraPitch -= mouseY;

        cameraPitch = Mathf.Clamp(
            cameraPitch,
            -lookLimit,
            lookLimit
        );

        transform.rotation = Quaternion.Euler(
            0f,
            playerYaw,
            0f
        );

        playerCamera.localRotation =
            Quaternion.Euler(
                cameraPitch,
                0f,
                0f
            );
    }

    private void HandleMovement()
    {
        float horizontal =
            Input.GetAxisRaw("Horizontal");

        float forward =
            Input.GetAxisRaw("Vertical");

        Vector3 movement =
            transform.right * horizontal +
            transform.forward * forward;

        movement = Vector3.ClampMagnitude(
            movement,
            1f
        );

        bool hasMovement =
            movement.sqrMagnitude > 0.01f;

        bool shiftHeld =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        bool canSprint =
            playerStamina == null ||
            playerStamina.CanSprint;

        IsSprinting =
            shiftHeld &&
            hasMovement &&
            canSprint;

        if (playerStamina != null)
        {
            playerStamina.SetSprinting(
                IsSprinting
            );
        }

        float currentSpeed =
            IsSprinting
                ? sprintSpeed
                : walkSpeed;

        bool groundedBeforeMove =
            controller.isGrounded;

        if (groundedBeforeMove &&
            verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (groundedBeforeMove &&
            Input.GetButtonDown("Jump"))
        {
            verticalVelocity = Mathf.Sqrt(
                jumpHeight *
                -2f *
                gravity
            );
        }

        verticalVelocity +=
            gravity *
            Time.deltaTime;

        Vector3 finalMovement =
            movement * currentSpeed;

        finalMovement.y =
            verticalVelocity;

        CollisionFlags collisionFlags =
            controller.Move(
                finalMovement *
                Time.deltaTime
            );

        bool hitGround =
            (collisionFlags &
             CollisionFlags.Below) != 0;

        bool hitCeiling =
            (collisionFlags &
             CollisionFlags.Above) != 0;

        if (hitGround &&
            verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        // Immediately cancel upward motion when
        // the Character Controller hits a ceiling.
        if (hitCeiling &&
            verticalVelocity > 0f)
        {
            verticalVelocity = -2f;
        }
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }

    private void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;
    }
}