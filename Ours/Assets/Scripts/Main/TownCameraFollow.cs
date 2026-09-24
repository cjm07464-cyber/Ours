using UnityEngine;

public class TownCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 nextPosition = target.position + offset;
        nextPosition.z = transform.position.z;
        transform.position = nextPosition;
    }
}
