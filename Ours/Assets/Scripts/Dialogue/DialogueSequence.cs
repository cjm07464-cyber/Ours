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
    public bool stopLineStartSoundOnAdvance;
    public bool instantText = false;
    public AudioClip lineEndSound;
    [Range(0f, 1f)]
    public float lineEndSoundVolume = 1f;
    public bool waitForLineEndSound;
    public bool pauseBgmDuringLineEndSound;
    public float bgmResumeFadeDuration = 1f;
}

[CreateAssetMenu(fileName = "New Dialogue Sequence", menuName = "Dialogue/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();
}
