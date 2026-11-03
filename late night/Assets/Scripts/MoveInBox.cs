using TMPro;
using UnityEngine;

public class MoveInBox : MonoBehaviour
{
    [Header("Box")]
    [SerializeField] private string boxId = "bedroom";
    [SerializeField]
    private string boxDisplayName =
        "bedroom box";

    [Header("Interaction")]
    [SerializeField]
    private KeyCode pickupKey =
        KeyCode.E;

    [SerializeField] private TMP_Text promptText;

    [Header("Carried Transform")]
    [Tooltip("Position relative to BoxCarryPoint.")]
    [SerializeField]
    private Vector3 carriedLocalPosition =
        Vector3.zero;

    [Tooltip("Rotation relative to BoxCarryPoint.")]
    [SerializeField]
    private Vector3 carriedLocalRotation =
        Vector3.zero;

    [Tooltip("Size multiplier while the box is carried.")]
    [SerializeField, Range(0.1f, 2f)]
    private float carriedScaleMultiplier = 0.65f;

    [Header("Visuals")]
    [SerializeField] private GameObject boxGlow;

    private MoveInMinigameManager minigameManager;

    private Collider[] boxColliders;
    private Rigidbody boxRigidbody;

    private Vector3 originalScale;

    private bool playerNearby;
    private bool isBeingCarried;
    private bool hasBeenDelivered;

    public string BoxId
    {
        get
        {
            if (string.IsNullOrWhiteSpace(boxId))
            {
                return "";
            }

            return boxId.Trim();
        }
    }

    public bool IsBeingCarried =>
        isBeingCarried;

    public bool HasBeenDelivered =>
        hasBeenDelivered;

    private void Awake()
    {
        originalScale = transform.localScale;

        boxColliders =
            GetComponentsInChildren<Collider>(true);

        boxRigidbody =
            GetComponent<Rigidbody>();

        FindManager();
    }

    private void Start()
    {
        ShowPrompt(false);
        SetGlowVisible(true);
    }

    private void Update()
    {
        if (
            hasBeenDelivered ||
            isBeingCarried ||
            !playerNearby
        )
        {
            return;
        }

        if (Input.GetKeyDown(pickupKey))
        {
            TryPickUp();
        }
    }

    private void FindManager()
    {
        if (minigameManager != null)
        {
            return;
        }

        minigameManager =
            GetComponentInParent<MoveInMinigameManager>();

        if (minigameManager == null)
        {
            minigameManager =
                FindAnyObjectByType<MoveInMinigameManager>();
        }
    }

    private void TryPickUp()
    {
        FindManager();

        if (minigameManager == null)
        {
            Debug.LogError(
                $"{name} could not find " +
                "MoveInMinigameManager."
            );

            return;
        }

        minigameManager.PickUpBox(this);
    }

    public void BeginCarrying(Transform carryPoint)
    {
        if (hasBeenDelivered)
        {
            return;
        }

        if (carryPoint == null)
        {
            Debug.LogError(
                $"{name} received a null " +
                "BoxCarryPoint."
            );

            return;
        }

        isBeingCarried = true;
        playerNearby = false;

        ShowPrompt(false);
        SetGlowVisible(false);
        SetBoxCollision(false);

        transform.SetParent(
            carryPoint,
            false
        );

        transform.localPosition =
            carriedLocalPosition;

        transform.localRotation =
            Quaternion.Euler(
                carriedLocalRotation
            );

        transform.localScale =
            originalScale *
            carriedScaleMultiplier;
    }

    public void DeliverBox()
    {
        hasBeenDelivered = true;
        isBeingCarried = false;
        playerNearby = false;

        ShowPrompt(false);
        SetGlowVisible(false);
        SetBoxCollision(false);

        gameObject.SetActive(false);
    }

    private void SetBoxCollision(bool collisionEnabled)
    {
        if (boxColliders != null)
        {
            foreach (
                Collider boxCollider
                in boxColliders
            )
            {
                if (boxCollider != null)
                {
                    boxCollider.enabled =
                        collisionEnabled;
                }
            }
        }

        if (boxRigidbody != null)
        {
            if (!collisionEnabled)
            {
                boxRigidbody.linearVelocity =
                    Vector3.zero;

                boxRigidbody.angularVelocity =
                    Vector3.zero;
            }

            boxRigidbody.detectCollisions =
                collisionEnabled;

            boxRigidbody.isKinematic =
                !collisionEnabled;

            boxRigidbody.useGravity =
                collisionEnabled;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (
            hasBeenDelivered ||
            isBeingCarried
        )
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerNearby = true;
        ShowPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerNearby = false;
        ShowPrompt(false);
    }

    private void ShowPrompt(bool show)
    {
        if (promptText == null)
        {
            return;
        }

        bool shouldShow =
            show &&
            !isBeingCarried &&
            !hasBeenDelivered;

        promptText.gameObject.SetActive(
            shouldShow
        );

        if (shouldShow)
        {
            promptText.text =
                $"Press E to pick up " +
                $"{boxDisplayName}";
        }
    }

    private void SetGlowVisible(bool visible)
    {
        if (boxGlow == null)
        {
            return;
        }

        boxGlow.SetActive(
            visible &&
            !isBeingCarried &&
            !hasBeenDelivered
        );
    }
}