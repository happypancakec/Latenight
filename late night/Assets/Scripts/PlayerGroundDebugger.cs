using UnityEngine;

public class PlayerGroundDebugger : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CheckGround();
        }
    }

    private void CheckGround()
    {
        Vector3 origin =
            transform.position +
            Vector3.up * 0.25f;

        if (Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            100f,
            ~0,
            QueryTriggerInteraction.Ignore
        ))
        {
            Debug.Log(
                "SOLID OBJECT BELOW PLAYER: " +
                GetObjectPath(
                    hit.collider.transform
                ) +
                " | Collider: " +
                hit.collider.GetType().Name +
                " | Distance: " +
                hit.distance +
                " | Player Y: " +
                transform.position.y
            );
        }
        else
        {
            Debug.Log(
                "No solid collider found below player."
            );
        }
    }

    private string GetObjectPath(
        Transform objectTransform
    )
    {
        string path =
            objectTransform.name;

        Transform parent =
            objectTransform.parent;

        while (parent != null)
        {
            path =
                parent.name +
                "/" +
                path;

            parent = parent.parent;
        }

        return path;
    }
}