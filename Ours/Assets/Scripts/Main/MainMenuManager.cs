using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private const string MainSceneName = "MainScene";

    private static MainMenuManager instance;

    private GameObject canvasObject;
    private GameObject menuRoot;

    private TextMeshProUGUI nameText;
    private TextMeshProUGUI hpText;
    private TextMeshProUGUI goldText;

    private RectTransform cursorRect;
    private TextMeshProUGUI descriptionText;

    private readonly string[] menuItems = { "스탯", "가방", "저장하기", "게임종료" };
    private readonly string[] menuDescriptions =
    {
        "현재 능력치를 확인합니다.",
        "소지품을 확인합니다.",
        "현재 상태를 저장합니다.",
        "타이틀 화면으로 돌아갑니다."
    };

    private readonly TextMeshProUGUI[] commandTexts = new TextMeshProUGUI[4];

    private int selectedIndex;
    private bool isMenuOpen;
    private float cursorStepY = 36f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != MainSceneName)
        {
            return;
        }

        if (instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("MainMenuManager_Auto");
        instance = managerObject.AddComponent<MainMenuManager>();
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
        if (SceneManager.GetActiveScene().name != MainSceneName)
        {
            enabled = false;
            return;
        }

        EnsureEventSystem();
        BuildRuntimeUI();
        CloseMenuImmediate();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != MainSceneName)
        {
            return;
        }

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
            return;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeSelection(1);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ExecuteSelection();
        }
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void BuildRuntimeUI()
    {
        canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        menuRoot = CreateUIObject("MenuRoot", canvasObject.transform);
        RectTransform rootRect = menuRoot.GetComponent<RectTransform>();
        StretchToFullScreen(rootRect);

        GameObject infoPanel = CreatePanel("InfoPanel", menuRoot.transform, new Vector2(32f, -32f), new Vector2(560f, 210f));
        nameText = CreateText("NameText", infoPanel.transform, "이름:", new Vector2(24f, -24f));
        hpText = CreateText("HPText", infoPanel.transform, "HP:", new Vector2(24f, -84f));
        goldText = CreateText("GoldText", infoPanel.transform, "G:", new Vector2(24f, -144f));

        GameObject commandPanel = CreatePanel("CommandPanel", menuRoot.transform, new Vector2(32f, -260f), new Vector2(560f, 300f));

        TextMeshProUGUI statusText = CreateText("StatusText", commandPanel.transform, menuItems[0], new Vector2(90f, -30f));
        TextMeshProUGUI bagText = CreateText("BagText", commandPanel.transform, menuItems[1], new Vector2(90f, -96f));
        TextMeshProUGUI saveText = CreateText("SaveText", commandPanel.transform, menuItems[2], new Vector2(90f, -162f));
        TextMeshProUGUI quitText = CreateText("QuitText", commandPanel.transform, menuItems[3], new Vector2(90f, -228f));

        commandTexts[0] = statusText;
        commandTexts[1] = bagText;
        commandTexts[2] = saveText;
        commandTexts[3] = quitText;

        GameObject cursor = CreateUIObject("Cursor", commandPanel.transform);
        cursorRect = cursor.GetComponent<RectTransform>();
        cursorRect.anchorMin = new Vector2(0f, 1f);
        cursorRect.anchorMax = new Vector2(0f, 1f);
        cursorRect.pivot = new Vector2(0f, 1f);
        cursorRect.anchoredPosition = new Vector2(30f, -30f);
        cursorRect.sizeDelta = new Vector2(50f, 50f);

        TextMeshProUGUI cursorText = cursor.AddComponent<TextMeshProUGUI>();
        cursorText.text = "▶";
        cursorText.color = Color.white;
        cursorText.fontSize = 42f;
        cursorText.alignment = TextAlignmentOptions.MidlineLeft;

        GameObject descriptionPanel = CreatePanel("DescriptionPanel", menuRoot.transform, new Vector2(32f, 32f), new Vector2(1856f, 220f));
        RectTransform descriptionRect = descriptionPanel.GetComponent<RectTransform>();
        descriptionRect.anchorMin = new Vector2(0f, 0f);
        descriptionRect.anchorMax = new Vector2(0f, 0f);
        descriptionRect.pivot = new Vector2(0f, 0f);

        descriptionText = CreateText("DescriptionText", descriptionPanel.transform, string.Empty, new Vector2(24f, -24f));
        descriptionText.rectTransform.sizeDelta = new Vector2(1800f, 170f);
        descriptionText.enableWordWrapping = true;
    }

    private GameObject CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject panel = CreateUIObject(name, parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image background = panel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.92f);

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(2f, -2f);

        return panel;
    }

    private TextMeshProUGUI CreateText(string name, Transform parent, string value, Vector2 anchoredPosition)
    {
        GameObject textObject = CreateUIObject(name, parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(480f, 60f);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.color = Color.white;
        text.fontSize = 42f;
        text.alignment = TextAlignmentOptions.Left;

        return text;
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private void StretchToFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void OpenMenu()
    {
        if (menuRoot == null)
        {
            return;
        }

        isMenuOpen = true;
        selectedIndex = 0;
        menuRoot.SetActive(true);
        Time.timeScale = 0f;

        RefreshInfoText();
        UpdateSelectionVisual();
        UpdateDescriptionDefault();
    }

    private void CloseMenu()
    {
        isMenuOpen = false;
        menuRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    private void CloseMenuImmediate()
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
        selectedIndex = (selectedIndex + delta + menuItems.Length) % menuItems.Length;
        UpdateSelectionVisual();
        UpdateDescriptionDefault();
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < commandTexts.Length; i++)
        {
            if (commandTexts[i] == null)
            {
                continue;
            }

            commandTexts[i].color = Color.white;
        }

        if (cursorRect != null)
        {
            cursorRect.anchoredPosition = new Vector2(30f, -30f - (cursorStepY * selectedIndex));
        }
    }

    private void UpdateDescriptionDefault()
    {
        SetDescription(menuDescriptions[selectedIndex]);
    }

    private void ExecuteSelection()
    {
        switch (selectedIndex)
        {
            case 0:
                ShowStatusDetails();
                break;
            case 1:
                SetDescription("아직 가진 물건이 없습니다.");
                break;
            case 2:
                SaveCurrentGameState();
                SetDescription("저장했습니다.");
                break;
            case 3:
                Time.timeScale = 1f;
                SceneManager.LoadScene("Title");
                break;
        }
    }

    private void ShowStatusDetails()
    {
        if (GameManager.Instance == null)
        {
            SetDescription("GameManager를 찾을 수 없습니다.");
            return;
        }

        SetDescription(
            $"LV {GameManager.Instance.level}\n" +
            $"EXP {GameManager.Instance.exp}\n" +
            $"공격력 {GameManager.Instance.attack}\n" +
            $"방어력 {GameManager.Instance.defense}");
    }

    private void SaveCurrentGameState()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("MainMenuManager: GameManager가 없어 저장할 수 없습니다.");
            return;
        }

        GameManager.Instance.currentSceneName = SceneManager.GetActiveScene().name;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            playerObject = GameObject.Find("Player");
        }

        if (playerObject != null)
        {
            GameManager.Instance.playerPosition = playerObject.transform.position;
        }
        else
        {
            Debug.LogWarning("MainMenuManager: Player 오브젝트를 찾지 못해 위치 저장을 생략합니다.");
        }

        SaveSystem.SaveGame();
    }

    private void RefreshInfoText()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        nameText.text = $"이름: {GameManager.Instance.playerName}";
        hpText.text = $"HP: {GameManager.Instance.currentHP} / {GameManager.Instance.maxHP}";
        goldText.text = $"G: {GameManager.Instance.gold}";
    }

    private void SetDescription(string message)
    {
        if (descriptionText != null)
        {
            descriptionText.text = message;
        }
    }
}
