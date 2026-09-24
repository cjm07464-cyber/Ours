using UnityEngine;

public class WorldToUIFollower : MonoBehaviour
{
    [SerializeField] private Transform worldTarget;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RectTransform uiTarget;

    private void LateUpdate()
    {
        if (worldTarget == null || targetCamera == null || uiTarget == null)
            return;

        Vector3 screenPosition =
            targetCamera.WorldToScreenPoint(worldTarget.position);

        uiTarget.position = screenPosition;
    }
}