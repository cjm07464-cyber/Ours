using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TownRoomTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private Image fadeOverlay;

    [Header("Timing")]
    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private float fadeInDuration = 0.25f;

    private bool isTransitioning;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning || !other.CompareTag("Player"))
        {
            return;
        }

        if (playerController == null)
        {
            playerController = other.GetComponent<PlayerController>();
        }

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        if (playerController == null || targetPoint == null)
        {
            yield break;
        }

        isTransitioning = true;
        playerController.SetCanMove(false);

        yield return FadeRoutine(1f, fadeOutDuration);

        MovePlayerToTarget();

        yield return FadeRoutine(0f, fadeInDuration);

        playerController.SetCanMove(true);
        isTransitioning = false;
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        if (fadeOverlay == null)
        {
            yield break;
        }

        fadeOverlay.gameObject.SetActive(true);
        float safeDuration = Mathf.Max(0.01f, duration);
        Tween tween = fadeOverlay.DOFade(Mathf.Clamp01(targetAlpha), safeDuration).SetEase(Ease.Linear);
        yield return tween.WaitForCompletion();
    }

    private void MovePlayerToTarget()
    {
        Vector3 targetPosition = targetPoint.position;
        Rigidbody2D playerRigidbody = playerController.GetComponent<Rigidbody2D>();

        if (playerRigidbody != null)
        {
            playerRigidbody.position = targetPosition;
            Physics2D.SyncTransforms();
            return;
        }

        playerController.transform.position = targetPosition;
    }
}
