using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public CharacterData speaker;
    public string expressionId = "normal";
    [TextArea]
    public string text;
    public bool showPortrait;
    public bool showSpeakerName = true;
    public AudioClip lineStartSound;
    public bool instantText = false;
}

[CreateAssetMenu(fileName = "New Dialogue Sequence", menuName = "Dialogue/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();
}
