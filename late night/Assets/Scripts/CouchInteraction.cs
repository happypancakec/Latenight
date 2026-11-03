using TMPro;
using UnityEngine;

public class CouchInteraction : MonoBehaviour
{
    [Header("Couch Points")]
    [SerializeField] private Transform sitPoint;
    [SerializeField] private Transform standPoint;

    [Header("Interaction")]
    [SerializeField]
    private KeyCode sitKey =
        KeyCode.E;

    [SerializeField]
    private KeyCode standKey =
        KeyCode.Q;

    [SerializeField, Min(0.5f)]
    private float interactionDistance = 3f;

    [SerializeField] private TMP_Text promptText;

    private Transform player;
    private FirstPersonController playerController;
    private CharacterController characterController;

    private bool isSeated;
    private bool promptIsShowing;

    private const string RemoteSaveKey =
        "HasTVRemote";

    public bool IsSeated => isSeated;

    private void Start()
    {
        FindPlayer();
        HideOwnPrompt();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();

            if (player == null)
            {
                return;
            }
        }

        if (isSeated)
        {
            ShowSeatedInstructions();

            if (Input.GetKeyDown(standKey))
            {
                StandUp();
            }

            return;
        }

        float distanceSquared =
            (
                transform.position -
                player.position
            ).sqrMagnitude;

        float interactionDistanceSquared =
            interactionDistance *
            interactionDistance;

        bool playerIsClose =
            distanceSquared <=
            interactionDistanceSquared;

        if (playerIsClose)
        {
            ShowSitPrompt();

            if (Input.GetKeyDown(sitKey))
            {
                SitDown();
            }
        }
        else if (promptIsShowing)
        {
            HideOwnPrompt();
        }
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (playerObject == null)
        {
            playerController =
                FindAnyObjectByType
                    <FirstPersonController>();

            if (playerController != null)
            {
                player =
                    playerController.transform;

                characterController =
                    playerController.GetComponent
                        <CharacterController>();
            }

            return;
        }

        player = playerObject.transform;

        playerController =
            playerObject.GetComponent
                <FirstPersonController>();

        if (playerController == null)
        {
            playerController =
                playerObject.GetComponentInChildren
                    <FirstPersonController>();
        }

        characterController =
            playerObject.GetComponent
                <CharacterController>();
    }

    private void SitDown()
    {
        if (
            player == null ||
            sitPoint == null
        )
        {
            Debug.LogError(
                "CouchInteraction needs a Player " +
                "and SitPoint."
            );

            return;
        }

        HideOwnPrompt();

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        MovePlayerSafely(
            sitPoint.position,
            sitPoint.rotation
        );

        isSeated = true;

        ShowSeatedInstructions();

        Debug.Log(
            "The player sat on the couch."
        );
    }

    private void StandUp()
    {
        if (!isSeated)
        {
            return;
        }

        if (standPoint == null)
        {
            Debug.LogError(
                "CouchInteraction does not have a " +
                "StandPoint assigned."
            );

            return;
        }

        isSeated = false;
        HideOwnPrompt();

        MovePlayerSafely(
            standPoint.position,
            standPoint.rotation
        );

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        Debug.Log(
            "The player stood up from the couch."
        );
    }

    private void MovePlayerSafely(
        Vector3 position,
        Quaternion rotation
    )
    {
        bool controllerWasEnabled =
            characterController != null &&
            characterController.enabled;

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        player.SetPositionAndRotation(
            position,
            rotation
        );

        if (
            characterController != null &&
            controllerWasEnabled
        )
        {
            characterController.enabled = true;
        }
    }

    private void ShowSitPrompt()
    {
        if (promptText == null)
        {
            return;
        }

        promptText.gameObject.SetActive(true);
        promptText.text =
            "Press E to sit on the couch";

        promptIsShowing = true;
    }

    public void ShowSeatedInstructions()
    {
        if (promptText == null)
        {
            return;
        }

        bool hasRemote =
            PlayerPrefs.GetInt(
                RemoteSaveKey,
                0
            ) == 1;

        promptText.gameObject.SetActive(true);

        if (hasRemote)
        {
            promptText.text =
                "Left Click: Change Channel  |  " +
                "Right Click: TV Power  |  " +
                "Q: Stand Up";
        }
        else
        {
            promptText.text =
                "You need the TV remote  |  " +
                "Q: Stand Up";
        }

        promptIsShowing = true;
    }

    private void HideOwnPrompt()
    {
        if (!promptIsShowing)
        {
            return;
        }

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }

        promptIsShowing = false;
    }

    private void OnDisable()
    {
        if (
            isSeated &&
            player != null &&
            standPoint != null
        )
        {
            StandUp();
        }
        else
        {
            HideOwnPrompt();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionDistance
        );
    }
}