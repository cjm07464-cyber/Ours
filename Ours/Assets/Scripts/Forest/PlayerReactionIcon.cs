using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PlayerReactionIcon : MonoBehaviour
{
    [SerializeField] private SpriteRenderer reactionRenderer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip reactionClip;
    [SerializeField] private float showDuration = 0.45f;
    [SerializeField] private float reactionScale = 0.6f;
    [SerializeField] private float popScale = 0.7f;
    private Coroutine reactionCoroutine;
    private Vector3 originalLocalScale;
    private Vector3 originalLocalPosition;

    private void Awake()
    {
        if (reactionRenderer != null)
        {
            originalLocalScale = reactionRenderer.transform.localScale;
            originalLocalPosition = reactionRenderer.transform.localPosition;
        }

        _ = reactionScale;
        _ = popScale;

        if (reactionRenderer != null)
            reactionRenderer.enabled = false;
    }

    public void PlayReaction()
    {
        if (reactionCoroutine != null)
            StopCoroutine(reactionCoroutine);

        reactionCoroutine = StartCoroutine(ReactionRoutine());
    }

    private IEnumerator ReactionRoutine()
    {
        if (reactionRenderer == null)
            yield break;

        reactionRenderer.transform.DOKill();

        reactionRenderer.enabled = true;
        reactionRenderer.transform.localPosition = originalLocalPosition;
        reactionRenderer.transform.localScale = Vector3.zero;

        if (audioSource != null && reactionClip != null)
            audioSource.PlayOneShot(reactionClip);

        reactionRenderer.transform
            .DOScale(originalLocalScale * 1.15f, 0.08f)
            .SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.1f);

        reactionRenderer.transform
            .DOScale(originalLocalScale, 0.05f);

        yield return new WaitForSeconds(showDuration);

        reactionRenderer.transform
            .DOScale(0f, 0.08f)
            .SetEase(Ease.InBack);

        yield return new WaitForSeconds(0.08f);

        reactionRenderer.enabled = false;
        reactionRenderer.transform.localPosition = originalLocalPosition;
        reactionRenderer.transform.localScale = originalLocalScale;
        reactionCoroutine = null;
    }
}
