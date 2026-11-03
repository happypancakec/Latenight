using System.Collections;
using UnityEngine;

public class DoorInteraction : MonoBehaviour, Interactable
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    [Header("Door Settings")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openingSpeed = 2f;
    [SerializeField] private bool startsOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private bool isOpen;
    private bool isMoving;

    private void Start()
    {
        closedRotation = transform.localRotation;

        Vector3 axis = GetRotationAxis();

        openRotation =
            closedRotation *
            Quaternion.AngleAxis(openAngle, axis);

        isOpen = startsOpen;

        if (startsOpen)
        {
            transform.localRotation = openRotation;
        }
    }

    public void Interact()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveDoor());
        }
    }

    private IEnumerator MoveDoor()
    {
        isMoving = true;

        Quaternion targetRotation =
            isOpen ? closedRotation : openRotation;

        while (
            Quaternion.Angle(
                transform.localRotation,
                targetRotation
            ) > 0.1f
        )
        {
            transform.localRotation =
                Quaternion.RotateTowards(
                    transform.localRotation,
                    targetRotation,
                    openingSpeed * 100f * Time.deltaTime
                );

            yield return null;
        }

        transform.localRotation = targetRotation;
        isOpen = !isOpen;
        isMoving = false;
    }

    private Vector3 GetRotationAxis()
    {
        switch (rotationAxis)
        {
            case RotationAxis.X:
                return Vector3.right;

            case RotationAxis.Z:
                return Vector3.forward;

            default:
                return Vector3.up;
        }
    }
}