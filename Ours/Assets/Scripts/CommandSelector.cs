using UnityEngine;
using UnityEngine.UI;

public class CommandSelector : MonoBehaviour
{
    private const int CommandCount = 6;
    private const int ColumnCount = 3;

    [SerializeField] private BattleManager battleManager;

    [Header("Battle Command Focus")]
    [SerializeField] private RectTransform[] focusSlots;
    [SerializeField] private Image[] focusBackgrounds;
    [SerializeField] private float selectedYOffset = 12f;
    [SerializeField] private float floatAmplitude = 4f;
    [SerializeField] private float floatSpeed = 2.5f;
    [SerializeField] private Color selectedBackgroundColor = new Color(1f, 0.5411765f, 0f, 1f);
    [SerializeField] private Color normalBackgroundColor = Color.black;

    [Header("Audio")]
    [SerializeField] private AudioClip cursorMoveSound;
    [SerializeField, Min(0f)] private float cursorMoveVolumeScale = 1.3f;
    [SerializeField] private AudioClip commandConfirmSound;
    [SerializeField, Min(0f)] private float commandConfirmVolumeScale = 1.5f;

    private int currentIndex;
    private Vector2[] focusBasePositions;
    private float floatAnimationTime;

    void Awake()
    {
        CacheFocusBasePositions();
    }

    void OnEnable()
    {
        currentIndex = 0;
        floatAnimationTime = 0f;
        CacheFocusBasePositions();
        ApplyFocusSelection();
    }

    void OnDisable()
    {
        ResetFocusVisuals();
    }

    void Update()
    {
        int previousIndex = currentIndex;

        if (GameInput.RightPressed)
        {
            if ((currentIndex + 1) % ColumnCount != 0)
            {
                currentIndex++;
            }
        }
        else if (GameInput.LeftPressed)
        {
            if (currentIndex % ColumnCount != 0)
            {
                currentIndex--;
            }
        }
        else if (GameInput.DownPressed)
        {
            int nextIndex = currentIndex + ColumnCount;
            if (nextIndex < CommandCount)
            {
                currentIndex = nextIndex;
            }
        }
        else if (GameInput.UpPressed)
        {
            int nextIndex = currentIndex - ColumnCount;
            if (nextIndex >= 0)
            {
                currentIndex = nextIndex;
            }
        }

        if (currentIndex != previousIndex)
        {
            floatAnimationTime = 0f;
            ApplyFocusSelection();
            PlaySfx(cursorMoveSound, cursorMoveVolumeScale);
        }

        UpdateSelectedSlotFloat();

        if (GameInput.ConfirmPressed)
        {
            ExecuteCurrentCommand();
        }
    }

    private void ExecuteCurrentCommand()
    {
        if (battleManager == null)
        {
            Debug.LogWarning("CommandSelector: BattleManager가 연결되지 않았습니다.");
            return;
        }

        PlaySfx(commandConfirmSound, commandConfirmVolumeScale);

        switch (currentIndex)
        {
            case 0: // 공격
                battleManager.OnAttackCommand();
                break;

            case 1: // 스킬
                battleManager.OpenSkillPanel();
                break;

            case 2: // 전화
                battleManager.OnPhoneCommand();
                break;

            case 3: // 방어
                battleManager.OnDefenseCommand();
                break;

            case 4: // 가방
                battleManager.ShowUnavailableCommandMessage();
                break;

            case 5: // 도망
                battleManager.OnEscapeCommand();
                break;
        }
    }

    private void PlaySfx(AudioClip clip, float volumeScale)
    {
        if (clip != null && SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayOneShot(clip, Mathf.Max(0f, volumeScale));
        }
    }

    private void CacheFocusBasePositions()
    {
        if (focusSlots == null)
        {
            focusBasePositions = null;
            return;
        }

        if (focusBasePositions != null && focusBasePositions.Length == focusSlots.Length)
        {
            return;
        }

        focusBasePositions = new Vector2[focusSlots.Length];
        for (int i = 0; i < focusSlots.Length; i++)
        {
            if (focusSlots[i] != null)
            {
                focusBasePositions[i] = focusSlots[i].anchoredPosition;
            }
        }
    }

    private void ApplyFocusSelection()
    {
        CacheFocusBasePositions();

        if (focusSlots != null && focusBasePositions != null)
        {
            for (int i = 0; i < focusSlots.Length; i++)
            {
                if (focusSlots[i] == null)
                {
                    continue;
                }

                Vector2 position = focusBasePositions[i];
                if (i == currentIndex)
                {
                    position.y += selectedYOffset;
                }

                focusSlots[i].anchoredPosition = position;
            }
        }

        if (focusBackgrounds != null)
        {
            for (int i = 0; i < focusBackgrounds.Length; i++)
            {
                if (focusBackgrounds[i] != null)
                {
                    focusBackgrounds[i].color = i == currentIndex
                        ? selectedBackgroundColor
                        : normalBackgroundColor;
                }
            }
        }
    }

    private void UpdateSelectedSlotFloat()
    {
        if (focusSlots == null || focusBasePositions == null ||
            currentIndex < 0 || currentIndex >= focusSlots.Length ||
            currentIndex >= focusBasePositions.Length ||
            focusSlots[currentIndex] == null)
        {
            return;
        }

        floatAnimationTime += Time.deltaTime * Mathf.Max(0f, floatSpeed);
        float floatOffset = Mathf.Sin(floatAnimationTime) * Mathf.Max(0f, floatAmplitude);
        Vector2 position = focusBasePositions[currentIndex];
        position.y += selectedYOffset + floatOffset;
        focusSlots[currentIndex].anchoredPosition = position;
    }

    private void ResetFocusVisuals()
    {
        if (focusSlots != null && focusBasePositions != null)
        {
            for (int i = 0; i < focusSlots.Length && i < focusBasePositions.Length; i++)
            {
                if (focusSlots[i] != null)
                {
                    focusSlots[i].anchoredPosition = focusBasePositions[i];
                }
            }
        }

        if (focusBackgrounds != null)
        {
            for (int i = 0; i < focusBackgrounds.Length; i++)
            {
                if (focusBackgrounds[i] != null)
                {
                    focusBackgrounds[i].color = normalBackgroundColor;
                }
            }
        }
    }
}
