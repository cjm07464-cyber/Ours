using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ForestNameEntryController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject nameEntryPanel;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI previewText;

    [Header("Input")]
    [SerializeField] private TMP_InputField nameInputField;

    [Header("Buttons")]
    [SerializeField] private Button newYesButton;
    [SerializeField] private Button newNoButton;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "TitleScene";
    [SerializeField] private bool startNewGameBeforeLoad;
    [SerializeField] private bool markForestCompletedOnConfirm = true;
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private AudioSource forestBgmAudioSource;
    [SerializeField] private float exitFadeDuration = 1.5f;

    private string submittedName = "";
    private string activeDestinationSceneName;
    private bool activeStartNewGameBeforeLoad;
    private bool activeMarkForestCompletedOnConfirm;
    private bool listenersRegistered;
    private bool isExiting;
    private Sequence exitSequence;

    public void ShowNameEntry()
    {
        ShowNameEntryInternal(titleSceneName, startNewGameBeforeLoad, markForestCompletedOnConfirm);
    }

    public void ShowNameEntryForNewGame(string destinationSceneName)
    {
        ShowNameEntryInternal(destinationSceneName, true, false);
    }

    private void ShowNameEntryInternal(string destinationSceneName, bool shouldStartNewGameBeforeLoad, bool shouldMarkForestCompleted)
    {
        RegisterListeners();

        activeDestinationSceneName = string.IsNullOrWhiteSpace(destinationSceneName)
            ? titleSceneName
            : destinationSceneName;
        activeStartNewGameBeforeLoad = shouldStartNewGameBeforeLoad;
        activeMarkForestCompletedOnConfirm = shouldMarkForestCompleted;
        submittedName = "";
        isExiting = false;

        if (nameEntryPanel != null)
        {
            nameEntryPanel.SetActive(true);
            nameEntryPanel.transform.SetAsLastSibling();
        }

        ShowInputStep();
    }

    public void SubmitName()
    {
        string candidateName = nameInputField != null ? nameInputField.text.Trim() : "";
        if (string.IsNullOrEmpty(candidateName))
        {
            FocusInputField();
            return;
        }

        submittedName = candidateName;
        ShowConfirmStep();
    }

    public void ConfirmName()
    {
        if (isExiting)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(submittedName))
        {
            ShowInputStep();
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("ForestNameEntryController: GameManager가 없어 이름을 저장할 수 없습니다.");
            return;
        }

        if (activeMarkForestCompletedOnConfirm)
        {
            GameManager.Instance.MarkForestCompleted();
        }

        if (!GameManager.Instance.SetPendingPlayerName(submittedName))
        {
            ShowInputStep();
            return;
        }

        if (activeStartNewGameBeforeLoad)
        {
            GameManager.Instance.StartNewGame(submittedName);
            GameManager.Instance.ClearStartupSession();
        }

        PlayExitTransition();
    }

    public void RejectName()
    {
        ShowInputStep();
    }

    private void ShowInputStep()
    {
        if (questionText != null)
        {
            questionText.text = "이 아이의 이름은?";
        }

        if (previewText != null)
        {
            previewText.gameObject.SetActive(false);
            previewText.text = "";
        }

        if (nameInputField != null)
        {
            nameInputField.gameObject.SetActive(true);
            nameInputField.text = "";
        }

        SetButtonVisible(newYesButton, false);
        SetButtonVisible(newNoButton, false);
        FocusInputField();
    }

    private void ShowConfirmStep()
    {
        if (questionText != null)
        {
            questionText.text = "이게 좋을까?";
        }

        if (nameInputField != null)
        {
            nameInputField.DeactivateInputField();
            nameInputField.gameObject.SetActive(false);
        }

        if (previewText != null)
        {
            previewText.text = submittedName;
            previewText.gameObject.SetActive(true);
        }

        SetButtonVisible(newYesButton, true);
        SetButtonVisible(newNoButton, true);
        SelectButton(newYesButton);
    }

    private void RegisterListeners()
    {
        if (listenersRegistered)
        {
            return;
        }

        listenersRegistered = true;

        if (nameInputField != null)
        {
            nameInputField.onSubmit.AddListener(_ => SubmitName());
        }

        if (newYesButton != null)
        {
            newYesButton.onClick.AddListener(ConfirmName);
        }

        if (newNoButton != null)
        {
            newNoButton.onClick.AddListener(RejectName);
        }
    }

    private void FocusInputField()
    {
        if (nameInputField == null)
        {
            return;
        }

        EventSystem.current?.SetSelectedGameObject(nameInputField.gameObject);
        nameInputField.Select();
        nameInputField.ActivateInputField();
    }

    private void SelectButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        EventSystem.current?.SetSelectedGameObject(button.gameObject);
        button.Select();
    }

    private void PlayExitTransition()
    {
        isExiting = true;

        FreezeNameEntryVisuals();

        if (newYesButton != null)
        {
            newYesButton.interactable = false;
        }

        if (newNoButton != null)
        {
            newNoButton.interactable = false;
        }

        float duration = Mathf.Max(0.01f, exitFadeDuration);
        exitSequence?.Kill();
        exitSequence = DOTween.Sequence();

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.transform.SetAsLastSibling();

            Color color = fadeOverlay.color;
            color.r = 0f;
            color.g = 0f;
            color.b = 0f;
            fadeOverlay.color = color;

            exitSequence.Join(fadeOverlay.DOFade(1f, duration));
        }

        if (forestBgmAudioSource != null)
        {
            exitSequence.Join(forestBgmAudioSource.DOFade(0f, duration));
        }

        if (fadeOverlay == null && forestBgmAudioSource == null)
        {
            CompleteExitTransition();
            return;
        }

        exitSequence.OnComplete(CompleteExitTransition);
    }

    private void CompleteExitTransition()
    {
        if (nameEntryPanel != null)
        {
            nameEntryPanel.SetActive(false);
        }

        SceneManager.LoadScene(GetActiveDestinationSceneName());
    }

    private string GetActiveDestinationSceneName()
    {
        return string.IsNullOrWhiteSpace(activeDestinationSceneName)
            ? titleSceneName
            : activeDestinationSceneName;
    }

    private void FreezeNameEntryVisuals()
    {
        if (nameEntryPanel == null)
        {
            return;
        }

        nameEntryPanel.SetActive(true);

        CanvasGroup[] canvasGroups = nameEntryPanel.GetComponentsInChildren<CanvasGroup>(true);
        for (int i = 0; i < canvasGroups.Length; i++)
        {
            canvasGroups[i].DOKill();
            canvasGroups[i].alpha = 1f;
        }

        Graphic[] graphics = nameEntryPanel.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i].DOKill();
        }
    }

    private void SetButtonVisible(Button button, bool visible)
    {
        if (button != null)
        {
            button.gameObject.SetActive(visible);
        }
    }

    private void OnDestroy()
    {
        exitSequence?.Kill();
    }
}
