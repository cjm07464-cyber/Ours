using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class ForestMapTransition : MonoBehaviour
{
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Transform targetPlayerPoint;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform targetCameraPoint;
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private bool enableCameraFollowOnArrival;
    [SerializeField] private CameraFollow cameraFollowComponent;

    private bool hasTriggeredThisEntry;
    private bool isTransitioning;
    private Sequence fadeSequence;

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
        if (hasTriggeredThisEntry || isTransitioning || !other.CompareTag("Player"))
        {
            return;
        }

        Rigidbody2D targetRigidbody = playerRigidbody != null ? playerRigidbody : other.attachedRigidbody;
        if (targetRigidbody == null || targetPlayerPoint == null)
        {
            Debug.LogWarning($"{nameof(ForestMapTransition)} on {name} needs a player Rigidbody2D and target player point.");
            return;
        }

        hasTriggeredThisEntry = true;
        ForestPlayerController playerController = targetRigidbody.GetComponent<ForestPlayerController>();

        if (fadeOverlay == null)
        {
            MovePlayer(targetRigidbody);
            MoveCamera();
            Physics2D.SyncTransforms();
            EnableCameraFollowIfNeeded(targetRigidbody);
            return;
        }

        PlayFadeTransition(targetRigidbody, playerController);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hasTriggeredThisEntry = false;
        }
    }

    private void MovePlayer(Rigidbody2D targetRigidbody)
    {
        targetRigidbody.velocity = Vector2.zero;
        targetRigidbody.angularVelocity = 0f;
        targetRigidbody.position = targetPlayerPoint.position;
    }

    private void MoveCamera()
    {
        if (targetCamera == null || targetCameraPoint == null)
        {
            return;
        }

        Vector3 cameraPosition = targetCamera.transform.position;
        cameraPosition.x = targetCameraPoint.position.x;
        cameraPosition.y = targetCameraPoint.position.y;
        targetCamera.transform.position = cameraPosition;
    }

    private void PlayFadeTransition(Rigidbody2D targetRigidbody, ForestPlayerController playerController)
    {
        isTransitioning = true;

        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }

        targetRigidbody.velocity = Vector2.zero;

        float safeFadeDuration = Mathf.Max(0.01f, fadeDuration);

        fadeOverlay.gameObject.SetActive(true);
        fadeSequence?.Kill();
        fadeSequence = DOTween.Sequence()
            .Append(fadeOverlay.DOFade(1f, safeFadeDuration).SetEase(Ease.Linear))
            .AppendCallback(() =>
            {
                MovePlayer(targetRigidbody);
                MoveCamera();
                Physics2D.SyncTransforms();
                EnableCameraFollowIfNeeded(targetRigidbody);
            })
            .Append(fadeOverlay.DOFade(0f, safeFadeDuration).SetEase(Ease.Linear))
            .OnComplete(() =>
            {
                if (playerController != null)
                {
                    playerController.SetCanMove(true);
                }

                isTransitioning = false;
            });
    }

    private void EnableCameraFollowIfNeeded(Rigidbody2D targetRigidbody)
    {
        if (enableCameraFollowOnArrival && cameraFollowComponent != null)
        {
            if (targetRigidbody != null)
            {
                cameraFollowComponent.target = targetRigidbody.transform;
            }

            cameraFollowComponent.SnapToTarget();
            cameraFollowComponent.enabled = true;
        }
    }

    private void OnDestroy()
    {
        fadeSequence?.Kill();
    }
}
