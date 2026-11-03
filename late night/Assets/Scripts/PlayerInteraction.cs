using System;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private Interactable currentInteractable;
    private PickupInteractable currentPickup;

    public bool IsLookingAtInteractable =>
        currentInteractable != null ||
        currentPickup != null;

    public bool IsLookingAtPickup =>
        currentPickup != null;

    private void Update()
    {
        CheckForInteractables();

        // Right-click collects a pickup.
        if (
            currentPickup != null &&
            Input.GetMouseButtonDown(1)
        )
        {
            currentPickup.Interact();
            return;
        }

        // E operates doors, drawers, beds, and other objects.
        if (
            currentInteractable != null &&
            currentPickup == null &&
            Input.GetKeyDown(interactionKey)
        )
        {
            currentInteractable.Interact();
        }
    }

    private void CheckForInteractables()
    {
        currentInteractable = null;
        currentPickup = null;

        if (playerCamera == null)
        {
            return;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            interactionDistance,
            interactionLayers
        );

        Array.Sort(
            hits,
            (first, second) =>
                first.distance.CompareTo(second.distance)
        );

        foreach (RaycastHit hit in hits)
        {
            PickupInteractable pickup =
                hit.collider
                    .GetComponentInParent<PickupInteractable>();

            if (pickup != null)
            {
                currentPickup = pickup;
                break;
            }

            Interactable interactable =
                hit.collider
                    .GetComponentInParent<Interactable>();

            if (
                interactable != null &&
                currentInteractable == null
            )
            {
                currentInteractable = interactable;

                // Continue searching in case a pickup is
                // behind the drawer's collider.
                continue;
            }

            // Stop the ray from detecting pickups through
            // ordinary walls or non-interactable objects.
            if (interactable == null)
            {
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(
            playerCamera.transform.position,
            playerCamera.transform.forward *
            interactionDistance
        );
    }
}