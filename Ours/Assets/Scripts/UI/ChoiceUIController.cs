using System;
using UnityEngine;

public class ChoiceUIController : MonoBehaviour
{
    private const int YesIndex = 0;
    private const int NoIndex = 1;

    [SerializeField] private GameObject choicePanel;
    [SerializeField] private RectTransform cursor;
    [SerializeField] private RectTransform yesText;
    [SerializeField] private RectTransform noText;
    [SerializeField] private Vector2 cursorOffset = new Vector2(-40f, 0f);

    private Action<bool> onChoiceSelected;
    private int selectedIndex;
    private int openedFrame = -1;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        HidePanel();
    }

    private void Update()
    {
        if (!IsOpen)
        {
            return;
        }

        if (Time.frameCount == openedFrame)
        {
            return;
        }

        if (GameInput.UpPressed || GameInput.DownPressed)
        {
            ToggleSelection();
            return;
        }

        if (GameInput.CancelPressed)
        {
            Complete(false);
            return;
        }

        if (GameInput.ConfirmPressed)
        {
            Complete(selectedIndex == YesIndex);
        }
    }

    public void Show(Action<bool> onSelected)
    {
        onChoiceSelected = onSelected;
        selectedIndex = YesIndex;
        openedFrame = Time.frameCount;
        IsOpen = true;

        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
        }

        UpdateCursor();
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        onChoiceSelected = null;
        HidePanel();
    }

    private void Complete(bool selectedYes)
    {
        if (!IsOpen)
        {
            return;
        }

        Action<bool> callback = onChoiceSelected;
        IsOpen = false;
        onChoiceSelected = null;
        HidePanel();
        callback?.Invoke(selectedYes);
    }

    private void ToggleSelection()
    {
        selectedIndex = selectedIndex == YesIndex ? NoIndex : YesIndex;
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        if (cursor == null)
        {
            return;
        }

        RectTransform targetText = selectedIndex == YesIndex ? yesText : noText;
        if (targetText == null)
        {
            return;
        }

        cursor.position = targetText.position + (Vector3)cursorOffset;
    }

    private void HidePanel()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
    }
}
