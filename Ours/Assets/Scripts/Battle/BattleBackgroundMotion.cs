using DG.Tweening;
using UnityEngine;

public class BattleBackgroundMotion : MonoBehaviour
{
    [Header("Position")]
    [SerializeField] private float positionAmplitudeX = 4f;
    [SerializeField] private float positionAmplitudeY = 3f;
    [SerializeField] private float positionDuration = 3.5f;

    [Header("Rotation")]
    [SerializeField] private float rotationAmplitude = 0.25f;
    [SerializeField] private float rotationDuration = 4.5f;

    [Header("Scale")]
    [SerializeField] private float scaleAmplitudeX = 0.008f;
    [SerializeField] private float scaleAmplitudeY = 0.006f;
    [SerializeField] private float scaleDuration = 3f;
    [SerializeField] private float edgePaddingScale = 0.01f;

    private RectTransform targetRect;
    private Vector2 originalAnchoredPosition;
    private Vector3 originalLocalScale;
    private Vector3 originalLocalEulerAngles;
    private Sequence positionSequence;
    private Sequence rotationSequence;
    private Sequence scaleSequence;

    private void Awake()
    {
        targetRect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (targetRect == null)
        {
            targetRect = GetComponent<RectTransform>();
        }

        if (targetRect == null)
        {
            enabled = false;
            return;
        }

        CacheOriginalTransform();
        StartMotion();
    }

    private void OnDisable()
    {
        StopMotion();
        RestoreOriginalTransform();
    }

    private void OnDestroy()
    {
        StopMotion();
    }

    private void CacheOriginalTransform()
    {
        originalAnchoredPosition = targetRect.anchoredPosition;
        originalLocalScale = targetRect.localScale;
        originalLocalEulerAngles = targetRect.localEulerAngles;
    }

    private void StartMotion()
    {
        StopMotion();

        float safePositionDuration = Mathf.Max(0.01f, positionDuration);
        float safeRotationDuration = Mathf.Max(0.01f, rotationDuration);
        float safeScaleDuration = Mathf.Max(0.01f, scaleDuration);

        Vector2 positionOffset = new Vector2(positionAmplitudeX, positionAmplitudeY);
        positionSequence = DOTween.Sequence()
            .Append(targetRect.DOAnchorPos(originalAnchoredPosition + positionOffset, safePositionDuration)
                .SetEase(Ease.InOutSine))
            .Append(targetRect.DOAnchorPos(originalAnchoredPosition - positionOffset, safePositionDuration * 1.1f)
                .SetEase(Ease.InOutSine))
            .Append(targetRect.DOAnchorPos(originalAnchoredPosition, safePositionDuration * 0.9f)
                .SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true);

        Vector3 positiveRotation = originalLocalEulerAngles + new Vector3(0f, 0f, rotationAmplitude);
        Vector3 negativeRotation = originalLocalEulerAngles - new Vector3(0f, 0f, rotationAmplitude);
        rotationSequence = DOTween.Sequence()
            .Append(targetRect.DOLocalRotate(positiveRotation, safeRotationDuration, RotateMode.Fast)
                .SetEase(Ease.InOutSine))
            .Append(targetRect.DOLocalRotate(negativeRotation, safeRotationDuration * 1.1f, RotateMode.Fast)
                .SetEase(Ease.InOutSine))
            .Append(targetRect.DOLocalRotate(originalLocalEulerAngles, safeRotationDuration * 0.9f, RotateMode.Fast)
                .SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true);

        float padding = Mathf.Max(0f, edgePaddingScale);
        Vector3 paddedScale = new Vector3(
            originalLocalScale.x * (1f + padding),
            originalLocalScale.y * (1f + padding),
            originalLocalScale.z);
        Vector3 scaleA = new Vector3(
            paddedScale.x * (1f + Mathf.Max(0f, scaleAmplitudeX)),
            paddedScale.y * (1f - Mathf.Max(0f, scaleAmplitudeY)),
            paddedScale.z);
        Vector3 scaleB = new Vector3(
            paddedScale.x * (1f - Mathf.Max(0f, scaleAmplitudeX)),
            paddedScale.y * (1f + Mathf.Max(0f, scaleAmplitudeY)),
            paddedScale.z);

        targetRect.localScale = paddedScale;
        scaleSequence = DOTween.Sequence()
            .Append(targetRect.DOScale(scaleA, safeScaleDuration).SetEase(Ease.InOutSine))
            .Append(targetRect.DOScale(scaleB, safeScaleDuration * 1.1f).SetEase(Ease.InOutSine))
            .Append(targetRect.DOScale(paddedScale, safeScaleDuration * 0.9f).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true);
    }

    private void StopMotion()
    {
        positionSequence?.Kill();
        rotationSequence?.Kill();
        scaleSequence?.Kill();
        positionSequence = null;
        rotationSequence = null;
        scaleSequence = null;
    }

    private void RestoreOriginalTransform()
    {
        if (targetRect == null)
        {
            return;
        }

        targetRect.anchoredPosition = originalAnchoredPosition;
        targetRect.localScale = originalLocalScale;
        targetRect.localEulerAngles = originalLocalEulerAngles;
    }
}
