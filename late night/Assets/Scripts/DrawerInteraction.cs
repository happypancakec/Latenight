using System.Collections;
using UnityEngine;

public class DrawerInteraction : MonoBehaviour, Interactable
{
    public enum SlideAxis
    {
        LocalX,
        LocalY,
        LocalZ
    }

    [Header("Drawer Movement")]
    [SerializeField] private SlideAxis slideAxis = SlideAxis.LocalZ;

    [Tooltip("Use a negative value if the drawer moves backward.")]
    [SerializeField] private float openDistance = 0.5f;

    [SerializeField] private float movementSpeed = 1.5f;
    [SerializeField] private bool startsOpen = false;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool isOpen;
    private bool isMoving;

    private void Start()
    {
        closedPosition = transform.position;

        Vector3 direction = GetSlideDirection();

        openPosition =
            closedPosition + direction * openDistance;

        isOpen = startsOpen;

        if (startsOpen)
        {
            transform.position = openPosition;
        }
    }

    public void Interact()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveDrawer());
        }
    }

    private IEnumerator MoveDrawer()
    {
        isMoving = true;

        Vector3 targetPosition =
            isOpen ? closedPosition : openPosition;

        while (
            Vector3.Distance(
                transform.position,
                targetPosition
            ) > 0.001f
        )
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    movementSpeed * Time.deltaTime
                );

            yield return null;
        }

        transform.position = targetPosition;

        isOpen = !isOpen;
        isMoving = false;
    }

    private Vector3 GetSlideDirection()
    {
        switch (slideAxis)
        {
            case SlideAxis.LocalX:
                return transform.right.normalized;

            case SlideAxis.LocalY:
                return transform.up.normalized;

            default:
                return transform.forward.normalized;
        }
    }
}