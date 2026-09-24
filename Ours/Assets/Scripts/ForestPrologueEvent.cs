using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class ForestPrologueEvent : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private ForestPlayerController playerController;
    [SerializeField] private SpriteRenderer alienSpriteRenderer;
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private ForestNameEntryController nameEntryController;
    [SerializeField] private AudioSource tensionAudioSource;
    [SerializeField] private AudioSource openingAudioSource;
    [SerializeField] private AudioClip fallingClip;
    [SerializeField] private AudioClip crashClip;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform ufoFocusPoint;
    [SerializeField] private CameraFollow cameraFollowComponent;
    [SerializeField] private Transform playerNightLight;
    [SerializeField] private Transform lightFocusPoint;
    [SerializeField] private PlayerReactionIcon playerReactionIcon;

    [Header("Alien Sprites")]
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite sideSprite;
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite walkSprite;

    [Header("Timing")]
    [SerializeField] private float upDuration = 2f;
    [SerializeField] private float sideDuration = 2f;
    [SerializeField] private float frontDuration = 1.3f;
    [SerializeField] private float walkPoseDuration = 0.25f;

    [SerializeField] private float walkMoveDuration = 1.8f;
    [SerializeField] private Vector2 walkMoveOffset = new Vector2(0f, -0.3f);
    [SerializeField] private float sceneFadeInDuration = 0.8f;
    [SerializeField] private float crashDelay = 0.5f;
    [SerializeField] private float cameraFocusDuration = 1.2f;
    [SerializeField, Range(0f, 1f)] private float nightOverlayAlpha = 0.5f;
    [SerializeField] private float frontAudioDelay = 0.6f;
    [SerializeField] private float tensionFadeDuration = 1.5f;
    [SerializeField] private float tensionClimaxLeadTime = 0.2f;
    [SerializeField] private float reactionDelay = 0.15f;

    [Header("UFO Smoke")]
    [SerializeField] private SpriteRenderer smokeRenderer;
    [SerializeField] private Sprite smoke1;
    [SerializeField] private Sprite smoke2;
    [SerializeField] private Sprite smoke3;
    [SerializeField] private float smokeFrameDuration = 0.25f;
    [SerializeField] private int smokeCycles = 2;
    [SerializeField] private float smoke2YOffset = 0.12f;
    [SerializeField] private float smoke3YOffset = 0.25f;

    [Header("UFO Red Sign")]
    [SerializeField] private Image redSignImage;
    [SerializeField] private float redSignDimAlpha = 0.15f;
    [SerializeField] private float redSignBlinkDuration = 0.3f;

    [Header("UFO Shutdown")]
    [SerializeField] private AudioSource ufoAudioSource;
    [SerializeField] private AudioClip ufoShutdownClip;
    [SerializeField] private float afterShutdownDelay = 0.7f;

    private bool isRunning;
    private Tween cameraFocusTween;
    private Tween redSignBlinkTween;
    private Tween redSignFadeTween;
    private Tween tensionFadeTween;
    private Tween alienWalkTween;
    private Tween playerNightLightTween;


    private IEnumerator Start()
    {
        _ = nightOverlayAlpha; // Serialized compatibility: NightOverlay is now controlled only in Inspector.

        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }

        if (fadeOverlay != null)
        {
            SetFadeAlpha(1f);
        }

        if (openingAudioSource != null && fallingClip != null)
        {
            openingAudioSource.PlayOneShot(fallingClip);
        }

        if (crashDelay > 0f)
        {
            yield return new WaitForSeconds(crashDelay);
        }

        if (openingAudioSource != null && crashClip != null)
        {
            openingAudioSource.PlayOneShot(crashClip);
        }

        yield return WaitForOpeningSoundsToFinish();

        if (fadeOverlay == null)
        {
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }

            yield break;
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, sceneFadeInDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            SetFadeAlpha(Mathf.Lerp(1f, 0f, t));

            yield return null;
        }

        SetFadeAlpha(0f);

        if (playerController != null)
        {
            playerController.SetCanMove(true);
        }
    }

    private IEnumerator WaitForOpeningSoundsToFinish()
    {
        if (openingAudioSource == null)
        {
            yield break;
        }

        float fallingEndTime = fallingClip != null ? fallingClip.length : 0f;
        float crashEndTime = crashClip != null ? Mathf.Max(0f, crashDelay) + crashClip.length : 0f;
        float finalEndTime = Mathf.Max(fallingEndTime, crashEndTime);
        float remainingWait = finalEndTime - Mathf.Max(0f, crashDelay);

        if (remainingWait > 0f)
        {
            yield return new WaitForSeconds(remainingWait);
        }
    }
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
        if (isRunning || !other.CompareTag("Player"))
        {
            return;
        }

        StartCoroutine(PlayEvent());
    }

    private IEnumerator PlayEvent()
    {
        isRunning = true;

        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }

        if (cameraFollowComponent != null)
        {
            cameraFollowComponent.enabled = false;
        }

        yield return FocusCameraOnUfoRoutine();
        yield return PlayUfoMalfunctionRoutine();

        SetAlienSprite(upSprite);
        yield return new WaitForSeconds(upDuration);

        SetAlienSprite(sideSprite);
        yield return new WaitForSeconds(sideDuration);

        SetAlienSprite(frontSprite);
        yield return PlayFrontToWalkAndFadeRoutine();

        if (nameEntryController != null)
        {
            nameEntryController.ShowNameEntry();
        }
        else
        {
            Debug.LogWarning("ForestPrologueEvent: ForestNameEntryController가 연결되지 않았습니다.");
        }
    }

    private IEnumerator FocusCameraOnUfoRoutine()
    {
        if (targetCamera == null || ufoFocusPoint == null)
        {
            yield break;
        }

        cameraFocusTween?.Kill();
        playerNightLightTween?.Kill();

        cameraFocusTween = targetCamera.transform
            .DOMoveY(ufoFocusPoint.position.y, Mathf.Max(0.01f, cameraFocusDuration))
            .SetEase(Ease.Linear);

        if (playerNightLight != null)
        {
            Transform lightTarget = lightFocusPoint != null ? lightFocusPoint : ufoFocusPoint;
            playerNightLightTween = playerNightLight
                .DOMoveY(lightTarget.position.y, Mathf.Max(0.01f, cameraFocusDuration))
                .SetEase(Ease.Linear);
        }

        yield return cameraFocusTween.WaitForCompletion();

        if (playerNightLightTween != null)
        {
            yield return playerNightLightTween.WaitForCompletion();
        }

        yield return new WaitForSeconds(reactionDelay);

        if (playerReactionIcon != null)
        {
            playerReactionIcon.PlayReaction();
        }
    }

    private IEnumerator PlayUfoMalfunctionRoutine()
    {
        StartRedSignBlink();

        Sprite[] smokeFrames = { smoke1, smoke2, smoke3 };
        int safeCycles = Mathf.Max(0, smokeCycles);
        float safeFrameDuration = Mathf.Max(0.01f, smokeFrameDuration);
        Vector3 smokeBaseLocalPosition = smokeRenderer != null
            ? smokeRenderer.transform.localPosition
            : Vector3.zero;

        for (int cycle = 0; cycle < safeCycles; cycle++)
        {
            for (int i = 0; i < smokeFrames.Length; i++)
            {
                if (smokeRenderer != null)
                {
                    smokeRenderer.transform.localPosition = GetSmokeLocalPosition(smokeBaseLocalPosition, i);
                    smokeRenderer.sprite = smokeFrames[i];
                }

                yield return new WaitForSeconds(safeFrameDuration);
            }

            if (smokeRenderer != null)
            {
                smokeRenderer.sprite = null;
                smokeRenderer.transform.localPosition = smokeBaseLocalPosition;
            }

            yield return new WaitForSeconds(safeFrameDuration);
        }

        if (smokeRenderer != null)
        {
            smokeRenderer.sprite = null;
            smokeRenderer.transform.localPosition = smokeBaseLocalPosition;
        }

        StopRedSignBlink();
        yield return PlayUfoShutdownRoutine();
    }

    private Vector3 GetSmokeLocalPosition(Vector3 baseLocalPosition, int smokeFrameIndex)
    {
        Vector3 localPosition = baseLocalPosition;

        if (smokeFrameIndex == 1)
        {
            localPosition.y += smoke2YOffset;
        }
        else if (smokeFrameIndex == 2)
        {
            localPosition.y += smoke3YOffset;
        }

        return localPosition;
    }

    private void StartRedSignBlink()
    {
        if (redSignImage == null)
        {
            return;
        }

        redSignBlinkTween?.Kill();
        SetImageAlpha(redSignImage, 1f);
        redSignBlinkTween = redSignImage
            .DOFade(Mathf.Clamp01(redSignDimAlpha), Mathf.Max(0.01f, redSignBlinkDuration))
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopRedSignBlink()
    {
        redSignBlinkTween?.Kill();
        redSignBlinkTween = null;

        if (redSignImage != null)
        {
            SetImageAlpha(redSignImage, 1f);
        }
    }

    private IEnumerator PlayUfoShutdownRoutine()
    {
        if (redSignImage != null)
        {
            SetImageAlpha(redSignImage, 1f);
        }

        if (ufoAudioSource != null && ufoShutdownClip != null)
        {
            ufoAudioSource.PlayOneShot(ufoShutdownClip);
        }

        float shutdownDuration = ufoShutdownClip != null
            ? Mathf.Max(0.01f, ufoShutdownClip.length)
            : 1f;

        redSignFadeTween?.Kill();
        if (redSignImage != null)
        {
            redSignFadeTween = redSignImage.DOFade(0f, shutdownDuration).SetEase(Ease.Linear);
        }

        yield return new WaitForSeconds(shutdownDuration);

        if (redSignImage != null)
        {
            SetImageAlpha(redSignImage, 0f);
        }

        if (afterShutdownDelay > 0f)
        {
            yield return new WaitForSeconds(afterShutdownDelay);
        }
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    private IEnumerator PlayFrontToWalkAndFadeRoutine()
    {
        float delay = Mathf.Max(0f, frontAudioDelay);

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        bool hasTensionClip = tensionAudioSource != null && tensionAudioSource.clip != null;
        PlayTensionSoundFadeIn();

        if (hasTensionClip)
        {
            float clipLength = Mathf.Max(0f, tensionAudioSource.clip.length);
            float walkStartTime = Mathf.Max(0f, clipLength - Mathf.Max(0f, tensionClimaxLeadTime));

            if (walkStartTime > 0f)
            {
                yield return new WaitForSeconds(walkStartTime);
            }

            StartWalkPoseAndMove();

            float timeUntilAudioEnd = Mathf.Max(0f, clipLength - walkStartTime);
            if (timeUntilAudioEnd > 0f)
            {
                yield return new WaitForSeconds(timeUntilAudioEnd);
            }

            alienWalkTween?.Kill();
            SetFadeAlpha(1f);
            yield break;
        }

        float fallbackWait = Mathf.Max(0f, frontDuration - delay);
        if (fallbackWait > 0f)
        {
            yield return new WaitForSeconds(fallbackWait);
        }

        yield return PlayWalkPoseAndMoveRoutine();
        SetFadeAlpha(1f);
    }

    private float GetFrontWalkStartWaitAfterAudioStart()
    {
        if (tensionAudioSource != null && tensionAudioSource.clip != null)
        {
            return Mathf.Max(0f, tensionAudioSource.clip.length - Mathf.Max(0f, tensionClimaxLeadTime));
        }

        return Mathf.Max(0f, frontDuration - Mathf.Max(0f, frontAudioDelay));
    }

    private void PlayTensionSoundFadeIn()
    {
        if (tensionAudioSource == null)
        {
            return;
        }

        tensionFadeTween?.Kill();

        float targetVolume = tensionAudioSource.volume;
        tensionAudioSource.volume = 0f;
        tensionAudioSource.Play();
        tensionFadeTween = tensionAudioSource
            .DOFade(targetVolume, Mathf.Max(0.01f, tensionFadeDuration))
            .SetEase(Ease.Linear);
    }


    private IEnumerator PlayWalkPoseAndMoveRoutine()
    {
        Tween walkTween = StartWalkPoseAndMove();
        if (walkTween != null)
        {
            yield return walkTween.WaitForCompletion();
        }
        else if (walkPoseDuration > 0f)
        {
            yield return new WaitForSeconds(walkPoseDuration);
        }
    }

    private Tween StartWalkPoseAndMove()
    {
        SetAlienSprite(walkSprite);

        Transform alienTransform =
            alienSpriteRenderer != null ? alienSpriteRenderer.transform : null;

        if (alienTransform == null)
        {
            return null;
        }

        Vector3 startPosition =
            alienTransform.position;

        Vector3 targetPosition =
            startPosition + (Vector3)walkMoveOffset;
        targetPosition.z = startPosition.z;

        float duration = Mathf.Max(0.01f, walkMoveDuration);

        alienWalkTween?.Kill();
        alienWalkTween = alienTransform
            .DOMove(targetPosition, duration)
            .SetEase(Ease.Linear);

        return alienWalkTween;
    }

    private void SetAlienSprite(Sprite sprite)
    {
        if (alienSpriteRenderer != null && sprite != null)
        {
            alienSpriteRenderer.sprite = sprite;
        }
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeOverlay == null)
        {
            return;
        }

        Color color = fadeOverlay.color;
        color.a = Mathf.Clamp01(alpha);
        fadeOverlay.color = color;
    }

    private void OnDisable()
    {
        KillTweens();
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    private void KillTweens()
    {
        cameraFocusTween?.Kill();
        redSignBlinkTween?.Kill();
        redSignFadeTween?.Kill();
        tensionFadeTween?.Kill();
        alienWalkTween?.Kill();
        playerNightLightTween?.Kill();
    }
}
