using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject menuRoot;

    [Header("Info")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI goldText;

    [Header("Command")]
    [SerializeField] private RectTransform cursor;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI bagText;
    [SerializeField] private TextMeshProUGUI saveText;
    [SerializeField] private TextMeshProUGUI quitText;

    [Header("Description")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Optional")]
    [SerializeField] private Transform playerTransform;

    private readonly string[] menuDescriptions =
    {
        "현재 능력치를 확인합니다.",
        "소지품을 확인합니다.",
        "현재 상태를 저장합니다.",
        "타이틀 화면으로 돌아갑니다."
    };

    private TextMeshProUGUI[] commandTexts;
    private int selectedIndex;
    private bool isMenuOpen;
    private Vector2 cursorStartPos;
    private float cursorStepY;

    private void Awake()
    {
        commandTexts = new[] { statusText, bagText, saveText, quitText };

        if (cursor != null)
        {
            cursorStartPos = cursor.anchoredPosition;
            if (statusText != null && bagText != null)
            {
                cursorStepY = Mathf.Abs(statusText.rectTransform.anchoredPosition.y - bagText.rectTransform.anchoredPosition.y);
                if (cursorStepY <= 0.01f)
                {
                    cursorStepY = 32f;
                }
            }
            else
            {
                cursorStepY = 32f;
            }
        }

        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "MainScene")
            return;

        if (!isMenuOpen)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                OpenMenu();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.X))
        {
            CloseMenu();
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangeSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            ExecuteCurrentSelection();
        }
    }

    private void OpenMenu()
    {
        if (menuRoot == null)
        {
            Debug.LogWarning("MainMenuManager: menuRoot가 연결되지 않았습니다.");
            return;
        }

        isMenuOpen = true;
        selectedIndex = 0;

        menuRoot.SetActive(true);
        Time.timeScale = 0f;

        RefreshInfoPanel();
        UpdateSelectionVisual();
        UpdateDescriptionBySelection();
    }

    private void CloseMenu()
    {
        isMenuOpen = false;

        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void ChangeSelection(int delta)
    {
        selectedIndex = (selectedIndex + delta + commandTexts.Length) % commandTexts.Length;
        UpdateSelectionVisual();
        UpdateDescriptionBySelection();
    }

    private void ExecuteCurrentSelection()
    {
        switch (selectedIndex)
        {
            case 0: // 스탯
                ShowStats();
                break;
            case 1: // 가방
                SetDescription("아직 가진 물건이 없습니다.");
                break;
            case 2: // 저장
                SaveCurrentState();
                SetDescription("저장했습니다.");
                break;
            case 3: // 게임종료
                Time.timeScale = 1f;
                SceneManager.LoadScene("Title");
                break;
        }
    }

    private void RefreshInfoPanel()
    {
        if (GameManager.Instance == null)
            return;

        if (nameText != null)
        {
            nameText.text = $"이름: {GameManager.Instance.playerName}";
        }

        if (hpText != null)
        {
            hpText.text = $"HP: {GameManager.Instance.currentHP} / {GameManager.Instance.maxHP}";
        }

        if (goldText != null)
        {
            goldText.text = $"G: {GameManager.Instance.gold}";
        }
    }

    private void ShowStats()
    {
        if (GameManager.Instance == null)
            return;

        SetDescription(
            $"LV {GameManager.Instance.level}\n" +
            $"EXP {GameManager.Instance.exp}\n" +
            $"공격력 {GameManager.Instance.attack}\n" +
            $"방어력 {GameManager.Instance.defense}");
    }

    private void SaveCurrentState()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("MainMenuManager: GameManager가 없어 저장할 수 없습니다.");
            return;
        }

        GameManager.Instance.currentSceneName = SceneManager.GetActiveScene().name;

        if (playerTransform != null)
        {
            GameManager.Instance.playerPosition = playerTransform.position;
        }

        SaveSystem.SaveGame();
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < commandTexts.Length; i++)
        {
            if (commandTexts[i] == null)
                continue;

            commandTexts[i].color = i == selectedIndex ? Color.yellow : Color.white;
        }

        if (cursor != null)
        {
            cursor.anchoredPosition = new Vector2(cursorStartPos.x, cursorStartPos.y - (cursorStepY * selectedIndex));
        }
    }

    private void UpdateDescriptionBySelection()
    {
        SetDescription(menuDescriptions[selectedIndex]);
    }

    private void SetDescription(string message)
    {
        if (descriptionText != null)
        {
            descriptionText.text = message;
        }
    }
}
