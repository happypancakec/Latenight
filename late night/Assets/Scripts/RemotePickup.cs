using TMPro;
using UnityEngine;

public class RemotePickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform remoteCarryPoint;

    [SerializeField]
    private TMP_Text promptText;

    [Header("Interaction")]
    [SerializeField]
    private KeyCode pickupKey = KeyCode.E;

    [SerializeField, Min(0.5f)]
    private float pickupDistance = 3f;

    [Header("Carried Transform")]
    [Tooltip(
        "Position relative to RemoteCarryPoint."
    )]
    [SerializeField]
    private Vector3 carriedLocalPosition =
        Vector3.zero;

    [Tooltip(
        "Rotation relative to RemoteCarryPoint."
    )]
    [SerializeField]
    private Vector3 carriedLocalRotation =
        Vector3.zero;

    [Tooltip(
        "Changes the remote's size while held."
    )]
    [SerializeField, Range(0.1f, 2f)]
    private float carriedScaleMultiplier = 1f;

    private Transform player;

    private Collider[] remoteColliders;
    private Rigidbody remoteRigidbody;

    private Vector3 originalScale;

    private bool isCarried;
    private bool promptIsShowing;

    private const string RemoteSaveKey =
        "HasTVRemote";

    public bool HasRemote =>
        PlayerPrefs.GetInt(
            RemoteSaveKey,
            0
        ) == 1;

    public bool IsCarried =>
        isCarried;

    private void Awake()
    {
        originalScale =
            transform.localScale;

        remoteColliders =
            GetComponentsInChildren<Collider>(
                true
            );

        remoteRigidbody =
            GetComponent<Rigidbody>();
    }

    private void Start()
    {
        FindPlayerAndCarryPoint();

        if (HasRemote)
        {
            PickUpRemote();
        }
        else
        {
            HidePrompt();
        }
    }

    private void Update()
    {
        if (isCarried)
        {
            return;
        }

        if (player == null)
        {
            FindPlayerAndCarryPoint();

            if (player == null)
            {
                return;
            }
        }

        float distanceSquared =
            (
                transform.position -
                player.position
            ).sqrMagnitude;

        float pickupDistanceSquared =
            pickupDistance *
            pickupDistance;

        bool playerIsClose =
            distanceSquared <=
            pickupDistanceSquared;

        if (playerIsClose)
        {
            ShowPrompt();

            if (Input.GetKeyDown(pickupKey))
            {
                PickUpRemote();
            }
        }
        else if (promptIsShowing)
        {
            HidePrompt();
        }
    }

    private void FindPlayerAndCarryPoint()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (playerObject != null)
        {
            player =
                playerObject.transform;
        }
        else
        {
            FirstPersonController controller =
                FindAnyObjectByType
                    <FirstPersonController>();

            if (controller != null)
            {
                player =
                    controller.transform;
            }
        }

        if (
            remoteCarryPoint == null &&
            player != null
        )
        {
            Camera playerCamera =
                player.GetComponentInChildren<Camera>();

            if (playerCamera != null)
            {
                remoteCarryPoint =
                    playerCamera.transform.Find(
                        "RemoteCarryPoint"
                    );
            }
        }
    }

    private void PickUpRemote()
    {
        if (isCarried)
        {
            return;
        }

        if (remoteCarryPoint == null)
        {
            FindPlayerAndCarryPoint();
        }

        if (remoteCarryPoint == null)
        {
            Debug.LogError(
                "RemotePickup could not find " +
                "RemoteCarryPoint. Assign it in " +
                "the Inspector."
            );

            return;
        }

        isCarried = true;

        HidePrompt();
        DisableRemotePhysics();

        transform.SetParent(
            remoteCarryPoint,
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

        PlayerPrefs.SetInt(
            RemoteSaveKey,
            1
        );

        PlayerPrefs.Save();

        Debug.Log(
            "The player picked up the TV remote."
        );
    }

    private void DisableRemotePhysics()
    {
        if (remoteColliders != null)
        {
            foreach (
                Collider remoteCollider
                in remoteColliders
            )
            {
                if (remoteCollider != null)
                {
                    remoteCollider.enabled =
                        false;
                }
            }
        }

        if (remoteRigidbody != null)
        {
            remoteRigidbody.linearVelocity =
                Vector3.zero;

            remoteRigidbody.angularVelocity =
                Vector3.zero;

            remoteRigidbody.useGravity =
                false;

            remoteRigidbody.isKinematic =
                true;

            remoteRigidbody.detectCollisions =
                false;
        }
    }

    private void ShowPrompt()
    {
        if (promptText == null)
        {
            return;
        }

        promptText.gameObject.SetActive(
            true
        );

        promptText.text =
            "Press E to pick up the TV remote";

        promptIsShowing = true;
    }

    private void HidePrompt()
    {
        if (!promptIsShowing)
        {
            return;
        }

        if (promptText != null)
        {
            promptText.gameObject.SetActive(
                false
            );
        }

        promptIsShowing = false;
    }

    private void OnDisable()
    {
        HidePrompt();
    }

    [ContextMenu("Forget Saved Remote")]
    private void ForgetSavedRemote()
    {
        PlayerPrefs.DeleteKey(
            RemoteSaveKey
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Saved TV remote ownership cleared."
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            pickupDistance
        );
    }
}