using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Map4CameraFollowZone : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform player;
    [SerializeField] private Transform fixedCameraPoint;
    [SerializeField] private float returnDuration = 0.5f;

    private bool playerInside;
    private Tween returnTween;

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null)
        {
            trigger.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerInside || !other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;
        returnTween?.Kill();
        Transform targetPlayer = player != null ? player : other.transform;

        if (cameraFollow != null)
        {
            float fixedX = fixedCameraPoint != null
                ? fixedCameraPoint.position.x
                : cameraFollow.transform.position.x;
            float fixedY = cameraFollow.transform.position.y;

            cameraFollow.followX = false;
            cameraFollow.followY = true;
            cameraFollow.SetFixedPosition(fixedX, fixedY);
            cameraFollow.StartSmoothCatchUp(targetPlayer);
            cameraFollow.enabled = true;
        }

        KeepCameraFixedX();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!playerInside || !other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;

        if (cameraFollow != null)
        {
            cameraFollow.enabled = false;
        }

        ReturnCameraToFixedPoint();
    }

    private void ReturnCameraToFixedPoint()
    {
        if (targetCamera == null || fixedCameraPoint == null)
        {
            return;
        }

        returnTween?.Kill();
        KeepCameraFixedX();

        float safeReturnDuration = Mathf.Max(0.01f, returnDuration);
        returnTween = targetCamera.transform
            .DOMoveY(fixedCameraPoint.position.y, safeReturnDuration)
            .SetEase(Ease.OutCubic)
            .OnUpdate(KeepCameraFixedX)
            .OnComplete(MoveCameraToFixedPoint);
    }

    private void MoveCameraToFixedPoint()
    {
        if (targetCamera == null || fixedCameraPoint == null)
        {
            return;
        }

        Vector3 cameraPosition = targetCamera.transform.position;
        cameraPosition.x = fixedCameraPoint.position.x;
        cameraPosition.y = fixedCameraPoint.position.y;
        targetCamera.transform.position = cameraPosition;
    }

    private void KeepCameraFixedX()
    {
        if (targetCamera == null || fixedCameraPoint == null)
        {
            return;
        }

        Vector3 cameraPosition = targetCamera.transform.position;
        cameraPosition.x = fixedCameraPoint.position.x;
        targetCamera.transform.position = cameraPosition;
    }

    private void OnDestroy()
    {
        returnTween?.Kill();
    }
}
