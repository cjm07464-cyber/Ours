using System.Collections.Generic;
using UnityEngine;

public enum GameEventStepType
{
    Dialogue,
    SystemMessage,
    GiveItem,
    SetStoryFlag,
    Reaction,
    Choice,
    SaveGame
}

[System.Serializable]
public class GameEventStep
{
    public GameEventStepType stepType;

    [Header("Dialogue")]
    public DialogueSequence dialogueSequence;

    [Header("System Message")]
    [TextArea]
    public string messageText;
    public AudioClip messageSound;
    public bool pauseBgmWhileOpen;
    public float bgmResumeFadeDuration = 1f;

    [Header("Give Item")]
    public string itemId;
    public int count = 1;

    [Header("Story Flag")]
    public string flagId;

    [Header("Choice")]
    public GameEventSequence yesEventSequence;
    public GameEventSequence noEventSequence;
}

[CreateAssetMenu(fileName = "New Game Event Sequence", menuName = "Events/Game Event Sequence")]
public class GameEventSequence : ScriptableObject
{
    public List<GameEventStep> steps = new List<GameEventStep>();
}
