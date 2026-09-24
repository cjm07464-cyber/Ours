using UnityEngine;

public class StoryEventTrigger : MonoBehaviour
{
    [SerializeField] private GameEventRunner eventRunner;
    [SerializeField] private GameEventSequence eventSequence;
    [SerializeField] private string completedFlagId;
    [SerializeField] private bool triggerOnEnter = true;

    private bool isRunning;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggerOnEnter || isRunning || !other.CompareTag("Player"))
        {
            return;
        }

        if (IsCompleted())
        {
            return;
        }

        RunEvent();
    }

    private void RunEvent()
    {
        if (eventRunner == null || eventSequence == null)
        {
            return;
        }

        isRunning = true;
        eventRunner.Run(eventSequence, () => isRunning = false);
    }

    private bool IsCompleted()
    {
        return !string.IsNullOrWhiteSpace(completedFlagId) &&
               GameManager.Instance != null &&
               GameManager.Instance.HasStoryFlag(completedFlagId);
    }
}
