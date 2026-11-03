using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCollisionDebugger : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField]
    private KeyCode scanKey =
        KeyCode.K;

    [SerializeField]
    private KeyCode toggleLiveLoggingKey =
        KeyCode.L;

    [Header("Scan")]
    [Tooltip(
        "Extra space around the CharacterController " +
        "included in the collider scan."
    )]
    [SerializeField, Min(0f)]
    private float scanPadding = 0.4f;

    [Header("Live Collision Logging")]
    [SerializeField]
    private bool logLiveCollisions;

    [Tooltip(
        "Prevents the same collider from flooding " +
        "the Console every frame."
    )]
    [SerializeField, Min(0.05f)]
    private float repeatLogDelay = 0.5f;

    private CharacterController controller;

    private readonly Dictionary<Collider, float>
        lastLogTimes =
            new Dictionary<Collider, float>();

    private void Awake()
    {
        controller =
            GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(scanKey))
        {
            ScanNearbyColliders();
        }

        if (
            Input.GetKeyDown(
                toggleLiveLoggingKey
            )
        )
        {
            logLiveCollisions =
                !logLiveCollisions;

            Debug.Log(
                "PLAYER COLLISION DEBUG | " +
                "Live logging: " +
                logLiveCollisions
            );
        }
    }

    private void ScanNearbyColliders()
    {
        Bounds scanBounds =
            controller.bounds;

        scanBounds.Expand(
            scanPadding * 2f
        );

        Collider[] detectedColliders =
            Physics.OverlapBox(
                scanBounds.center,
                scanBounds.extents,
                Quaternion.identity,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide
            );

        Debug.Log(
            "PLAYER COLLISION SCAN | " +
            $"Player position: {transform.position} | " +
            $"Detected: {detectedColliders.Length}"
        );

        foreach (
            Collider detectedCollider
            in detectedColliders
        )
        {
            if (detectedCollider == null)
            {
                continue;
            }

            if (detectedCollider == controller)
            {
                continue;
            }

            PrintColliderInformation(
                "NEARBY",
                detectedCollider
            );
        }

        if (detectedColliders.Length <= 1)
        {
            Debug.Log(
                "PLAYER COLLISION SCAN | " +
                "No outside colliders were detected."
            );
        }
    }

    private void OnControllerColliderHit(
        ControllerColliderHit hit
    )
    {
        if (
            !logLiveCollisions ||
            hit.collider == null
        )
        {
            return;
        }

        Collider hitCollider =
            hit.collider;

        if (
            lastLogTimes.TryGetValue(
                hitCollider,
                out float previousLogTime
            ) &&
            Time.time - previousLogTime <
            repeatLogDelay
        )
        {
            return;
        }

        lastLogTimes[hitCollider] =
            Time.time;

        PrintColliderInformation(
            "CONTROLLER HIT",
            hitCollider
        );

        Debug.Log(
            "COLLISION DETAILS | " +
            $"Move direction: {hit.moveDirection} | " +
            $"Move length: {hit.moveLength:0.000} | " +
            $"Collision normal: {hit.normal} | " +
            $"Collision point: {hit.point}"
        );
    }

    private void PrintColliderInformation(
        string detectionType,
        Collider detectedCollider
    )
    {
        string objectPath =
            GetObjectPath(
                detectedCollider.transform
            );

        Vector3 closestPoint =
            detectedCollider.ClosestPoint(
                controller.bounds.center
            );

        float distance =
            Vector3.Distance(
                controller.bounds.center,
                closestPoint
            );

        Rigidbody attachedRigidbody =
            detectedCollider.attachedRigidbody;

        string rigidbodyInformation =
            attachedRigidbody == null
                ? "None"
                : attachedRigidbody.name +
                  $" | Kinematic: " +
                  attachedRigidbody.isKinematic;

        Debug.Log(
            $"PLAYER COLLISION DEBUG | " +
            $"Type: {detectionType} | " +
            $"Object: {objectPath} | " +
            $"Collider: " +
            $"{detectedCollider.GetType().Name} | " +
            $"Enabled: {detectedCollider.enabled} | " +
            $"Is Trigger: {detectedCollider.isTrigger} | " +
            $"Distance: {distance:0.000} | " +
            $"Layer: " +
            $"{LayerMask.LayerToName(detectedCollider.gameObject.layer)} | " +
            $"Rigidbody: {rigidbodyInformation}"
        );
    }

    private string GetObjectPath(
        Transform target
    )
    {
        string path =
            target.name;

        Transform currentParent =
            target.parent;

        while (currentParent != null)
        {
            path =
                currentParent.name +
                "/" +
                path;

            currentParent =
                currentParent.parent;
        }

        return path;
    }
}