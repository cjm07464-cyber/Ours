using System;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private string fallbackPlayerName = "할로";

    private DialogueSequence currentSequence;
    private Action onComplete;
    private Action<int> onLineStarted;
    private int lineIndex;

    public bool IsRunning { get; private set; }

    public void Run(
        DialogueSequence sequence,
        Action onComplete = null,
        Action<int> onLineStarted = null)
    {
        if (IsRunning)
        {
            return;
        }

        this.onComplete = onComplete;
        this.onLineStarted = onLineStarted;
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

        int currentLineIndex = lineIndex;
        DialogueLine line = currentSequence.lines[lineIndex];
        lineIndex++;

        if (line == null)
        {
            PlayCurrentLine();
            return;
        }

        dialogueController.SetContinueToNextLine(HasRemainingLine());
        onLineStarted?.Invoke(currentLineIndex);

        string text = ResolveText(line.text);
        PlayLineStartSound(line.lineStartSound);
        ConfigureLineEndSound(line);

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
                () => CompleteLine(line));
            return;
        }

        if (line.speaker != null)
        {
            dialogueController.ShowPortraitBoxOnly(text, line.speaker.dialogueTypeSound, () => CompleteLine(line));
            return;
        }

        if (line.instantText)
        {
            dialogueController.ShowInstant(text, () => CompleteLine(line));
            return;
        }

        dialogueController.Show(text, () => CompleteLine(line));
    }

    private void PlayLineStartSound(AudioClip clip)
    {
        if (clip != null && SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayOneShot(clip);
        }
    }

    private void ConfigureLineEndSound(DialogueLine line)
    {
        if (dialogueController == null || line == null)
        {
            return;
        }

        dialogueController.ConfigureLineEndSound(
            line.lineEndSound,
            line.lineEndSoundVolume,
            line.waitForLineEndSound,
            line.pauseBgmDuringLineEndSound,
            line.bgmResumeFadeDuration);
    }

    private void CompleteLine(DialogueLine line)
    {
        if (line != null && line.stopLineStartSoundOnAdvance && SFXManager.Instance != null)
        {
            SFXManager.Instance.Stop();
        }

        PlayCurrentLine();
    }

    private void Complete()
    {
        IsRunning = false;
        currentSequence = null;

        Action callback = onComplete;
        onComplete = null;
        onLineStarted = null;
        callback?.Invoke();
    }

    private bool HasRemainingLine()
    {
        if (currentSequence == null || currentSequence.lines == null)
        {
            return false;
        }

        for (int i = lineIndex; i < currentSequence.lines.Count; i++)
        {
            if (currentSequence.lines[i] != null)
            {
                return true;
            }
        }

        return false;
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
