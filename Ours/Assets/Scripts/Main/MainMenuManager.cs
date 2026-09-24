using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainMenuManager : MonoBehaviour
{
    private const string TownSceneName = GameManager.TownSceneName;
    private const int MenuItemCount = 5;
    private const float UnselectedTextX = -32f;
    private const float SelectedTextX = 5f;
    private const float CursorX = -20f;

    private static readonly float[] SlotYPositions = { 215f, 105f, -5f, -115f, -225f };

    private static MainMenuManager instance;

    [Header("References")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private RectTransform cursor;
    [SerializeField] private RectTransform statusText;
    [SerializeField] private RectTransform equipmentText;
    [SerializeField] private RectTransform bagText;
    [SerializeField] private RectTransform phoneText;
    [SerializeField] private RectTransform closeText;
    [FormerlySerializedAs("playerStatusSlot")]
    [SerializeField] private CharacterStatusSlotUI characterStatusSlot;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private GameEventRunner phoneEventRunner;
    [SerializeField] private GameEventSequence phoneEventSequence;

    [Header("Unlock")]
    [SerializeField] private string menuUnlockFlagId = GameManager.FieldMenuUnlockFlagId;

    private readonly RectTransform[] menuTexts = new RectTransform[MenuItemCount];
    private int selectedIndex;
    private bool isMenuOpen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneLoaded()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        EnsureMainMenuManager(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureMainMenuManager(scene);
    }

    private static void EnsureMainMenuManager(Scene scene)
    {
        if (scene.name != TownSceneName || instance != null)
        {
            return;
        }

        MainMenuManager existingManager = FindObjectOfType<MainMenuManager>(true);
        if (existingManager != null)
        {
            instance = existingManager;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        CacheMenuTextSlots();
        CloseMenuImmediate();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != TownSceneName)
        {
            return;
        }

        if (!isMenuOpen)
        {
            if (GameInput.MenuPressed && CanOpenMenu())
            {
                OpenMenu();
            }

            return;
        }

        if (GameInput.MenuPressed || GameInput.CancelPressed)
        {
            CloseMenu(true);
            return;
        }

        if (GameInput.UpPressed)
        {
            ChangeSelection(-1);
            return;
        }

        if (GameInput.DownPressed)
        {
            ChangeSelection(1);
            return;
        }

        if (GameInput.ConfirmPressed)
        {
            ExecuteSelection();
        }
    }

    private void CacheMenuTextSlots()
    {
        menuTexts[0] = statusText;
        menuTexts[1] = equipmentText;
        menuTexts[2] = bagText;
        menuTexts[3] = phoneText;
        menuTexts[4] = closeText;
    }

    private bool CanOpenMenu()
    {
        if (!IsFieldMenuUnlocked() || IsDialogueOpen())
        {
            return false;
        }

        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        return playerController == null || !playerController.IsAutoMoving;
    }

    private bool IsFieldMenuUnlocked()
    {
        if (GameManager.Instance == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(menuUnlockFlagId))
        {
            return GameManager.Instance.IsFieldMenuUnlocked();
        }

        return GameManager.Instance.HasStoryFlag(menuUnlockFlagId);
    }

    private bool IsDialogueOpen()
    {
        if (dialogueController == null)
        {
            dialogueController = FindObjectOfType<DialogueController>(true);
        }

        return dialogueController != null && dialogueController.IsOpen;
    }

    private void OpenMenu()
    {
        if (mainMenuUI == null)
        {
            return;
        }

        isMenuOpen = true;
        selectedIndex = 0;
        mainMenuUI.SetActive(true);
        SetPlayerCanMove(false);
        RefreshCharacterStatusSlot();
        UpdateSelectionVisual();
    }

    private void CloseMenu(bool restorePlayerMovement)
    {
        isMenuOpen = false;

        if (mainMenuUI != null)
        {
            mainMenuUI.SetActive(false);
        }

        if (restorePlayerMovement)
        {
            SetPlayerCanMove(true);
        }
    }

    private void CloseMenuImmediate()
    {
        isMenuOpen = false;

        if (mainMenuUI != null)
        {
            mainMenuUI.SetActive(false);
        }
    }

    private void SetPlayerCanMove(bool canMove)
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        if (playerController != null)
        {
            playerController.SetCanMove(canMove);
        }
    }

    private void RefreshCharacterStatusSlot()
    {
        if (characterStatusSlot == null)
        {
            characterStatusSlot = FindObjectOfType<CharacterStatusSlotUI>(true);
        }

        if (characterStatusSlot != null)
        {
            characterStatusSlot.Refresh();
        }
    }

    private void ChangeSelection(int delta)
    {
        selectedIndex = (selectedIndex + delta + MenuItemCount) % MenuItemCount;
        UpdateSelectionVisual();
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < menuTexts.Length; i++)
        {
            SetTextPosition(menuTexts[i], i == selectedIndex ? SelectedTextX : UnselectedTextX, SlotYPositions[i]);
        }

        SetTextPosition(cursor, CursorX, SlotYPositions[selectedIndex]);
    }

    private void SetTextPosition(RectTransform rectTransform, float x, float y)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchoredPosition = new Vector2(x, y);
    }

    private void ExecuteSelection()
    {
        switch (selectedIndex)
        {
            case 0:
                Debug.Log("상태 선택");
                break;
            case 1:
                Debug.Log("장비 선택");
                break;
            case 2:
                Debug.Log("가방 선택");
                break;
            case 3:
                StartPhoneEvent();
                break;
            case 4:
                CloseMenu(true);
                break;
        }
    }

    private void StartPhoneEvent()
    {
        if (phoneEventRunner == null || phoneEventSequence == null)
        {
            Debug.LogWarning("MainMenuManager: 통화용 GameEventRunner 또는 GameEventSequence가 연결되지 않았습니다.");
            return;
        }

        if (phoneEventRunner.IsRunning)
        {
            return;
        }

        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        phoneEventRunner.SetPlayerController(playerController);
        CloseMenu(false);
        phoneEventRunner.Run(phoneEventSequence, () => SetPlayerCanMove(true));
    }
}
