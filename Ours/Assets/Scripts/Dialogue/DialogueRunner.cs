using System;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private string fallbackPlayerName = "할로";

    private DialogueSequence currentSequence;
    private Action onComplete;
    private int lineIndex;

    public bool IsRunning { get; private set; }

    public void Run(DialogueSequence sequence, Action onComplete = null)
    {
        if (IsRunning)
        {
            return;
        }

        this.onComplete = onComplete;
        currentSequence = sequence;
        lineIndex = 0;
        IsRunning = true;

        PlayCurrentLine();
    }

    private void PlayCurrentLine()
    {
        if (dialogueController == null ||
            currentSequence == null ||
            currentSequence.lines == null ||
            lineIndex >= currentSequence.lines.Count)
        {
            Complete();
            return;
        }

        DialogueLine line = currentSequence.lines[lineIndex];
        lineIndex++;

        if (line == null)
        {
            PlayCurrentLine();
            return;
        }

        string text = ResolveText(line.text);
        PlayLineStartSound(line.lineStartSound);

        if (line.showPortrait && line.speaker != null)
        {
            dialogueController.ShowPortrait(
                text,
                line.speaker.displayName,
                line.speaker.GetPortrait(line.expressionId),
                line.speaker.portraitBackgroundColor,
                line.speaker.portraitGradientColor,
                line.showSpeakerName,
                line.speaker.dialogueTypeSound,
                PlayCurrentLine);
            return;
        }

        if (line.speaker != null)
        {
            dialogueController.ShowPortraitBoxOnly(text, line.speaker.dialogueTypeSound, PlayCurrentLine);
            return;
        }

        if (line.instantText)
        {
            dialogueController.ShowInstant(text, PlayCurrentLine);
            return;
        }

        dialogueController.Show(text, PlayCurrentLine);
    }

    private void PlayLineStartSound(AudioClip clip)
    {
        if (clip != null && SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayOneShot(clip);
        }
    }

    private void Complete()
    {
        IsRunning = false;
        currentSequence = null;

        Action callback = onComplete;
        onComplete = null;
        callback?.Invoke();
    }

    private string ResolveText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        GameManager gameManager = GameManager.Instance;
        string playerName = fallbackPlayerName;
        string level = "1";
        string expToNextLevel = "0";

        if (gameManager != null)
        {
            if (!string.IsNullOrWhiteSpace(gameManager.playerName))
            {
                playerName = gameManager.playerName;
            }

            level = gameManager.level.ToString();
            expToNextLevel = gameManager.GetExpToNextLevel().ToString();
        }

        return text
            .Replace("{player}", playerName)
            .Replace("{level}", level)
            .Replace("{expToNextLevel}", expToNextLevel);
    }
}
