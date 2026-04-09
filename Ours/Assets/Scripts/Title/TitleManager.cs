using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public GameObject continuePanel;
    public GameObject newGamePanel;
    public GameObject introPanel;
    public GameObject confirmResetPanel;

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI introText;
    public TextMeshProUGUI namePreviewText;
    public TMP_InputField nameInputField;
    public GameObject new_yesButton;
    public GameObject new_noButton;
    public GameObject continue_yesButton;
    public GameObject continue_noButton;
    public GameObject Reset_yesButton;
    public GameObject Reset_noButton;

    public AudioSource introAudioSource;
    private string enterName;
    private int introIndex = 0;
    public float typingSpeed = 0.05f;
    private string[] introLines =
    {
        "옛날 옛적...\n하지만 아주 오래전 이야기도 아닌 이야기\n",
        "조용하고 작은 마을에\n한 아이가 살고 있었습니다\n",
        "아이는 오늘도 뒷산에 놀다가\n하늘에서 반짝이는 빛이 떨어지는 것을 보았습니다.\n",
        "그 빛 속에서 나타난 것은\n한 번도 본 적 없는\n신기한 생명체였습니다.\n",
        "아이는 다가가 조심스럽게\n손을 내밀었고\n그 작은 외계인과 친구가 되어주었습니다.\n",
        "둘은 함께 웃고, 뛰어놀고\n세상에서 가장 특별한 시간을 보냈습니다.\n",
        "하지만 시간은 조용히 흐르고\n이별의 날이 다가왔습니다.\n",
        "외계인은 아이와\n다시 만날 것을 약속하고\n아이에게 초능력을 선물해주었습니다.\n",
        "그렇게 시간은 다시 흐르고...."
    };
    private bool isTyping = false;
    private bool canNext = false;
    private Coroutine typingCoroutine;
    void Start()
    {
        HideAllPanels(); // 🔥 이거 반드시 먼저

        bool HasSaveData = SaveSystem.HasSaveData();
        if (HasSaveData)
        {
            continuePanel.SetActive(true);
            newGamePanel.SetActive(false);
        }
        else
        {
            continuePanel.SetActive(false);
            newGamePanel.SetActive(true);
            ShowNameInputStep();
        }

    }
    void Update()
    {
        if (!introPanel.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            HandleIntroZInput();
        }
    }
    private void HideAllPanels()
    {
        continuePanel.SetActive(false);
        newGamePanel.SetActive(false);
        confirmResetPanel.SetActive(false);
        introPanel.SetActive(false);
    }
    public void ShowNameInputStep()
    {
        questionText.text = "이 아이의 이름은?";
        nameInputField.gameObject.SetActive(true);
        namePreviewText.gameObject.SetActive(false);

        new_yesButton.SetActive(false);
        new_noButton.SetActive(false);

        nameInputField.text = "";
        nameInputField.ActivateInputField();
    }

    public void OnSubmitName()
    {
        enterName = nameInputField.text;
        if (string.IsNullOrEmpty(enterName))
        {
            questionText.text = "...이 아이의 이름은?";
            nameInputField.ActivateInputField();
            return;
        }

        questionText.text = "이게 좋을까?";
        namePreviewText.text = enterName;

        nameInputField.gameObject.SetActive(false);
        namePreviewText.gameObject.SetActive(true);

        new_yesButton.SetActive(true);
        new_noButton.SetActive(true);
    }

    public void OnclickYes()
    {
        Debug.Log("이름 확정: " + enterName);

        HideAllPanels();
        introPanel.SetActive(true);
        GameManager.Instance.StartNewGame(enterName);
        StartIntro();
    }

    public void OnclickNo()
    {
        ShowNameInputStep();
    }

    public void StartIntro()
    {
        if (introAudioSource != null)
        {
            introAudioSource.Play();
        }
        introIndex = 0;
        ShowCurrentIntroLine();
    }
    private void ShowCurrentIntroLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeLine(introLines[introIndex]));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        canNext = false;
        introText.text = "";

        foreach (char c in line)
        {
            introText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        canNext = true;
        typingCoroutine = null;
    }

    private void HandleIntroZInput()
    {
        if (isTyping)
        {
            SkipTyping();
            return;
        }

        if (canNext)
        {
            introIndex++;

            if (introIndex >= introLines.Length)
            {
                if (introAudioSource != null)
                {
                    introAudioSource.Stop();
                }

                GameManager.Instance.introPlayed = true;
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
                return;
            }

            ShowCurrentIntroLine();
        }
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        introText.text = introLines[introIndex];
        isTyping = false;
        canNext = true;
    }
    public void OnClickContinueYes()
    {
        SaveSystem.LoadGame();
        SceneManager.LoadScene("MainScene");
    }
    public void OnClickContinueNo()
    {
        HideAllPanels();
        confirmResetPanel.SetActive(true);
    }

    public void OnClickResetNo()
    {
        HideAllPanels();
        continuePanel.SetActive(true);
    }

    public void OnClickResetYes()
    {
        SaveSystem.DeleteSaveData();

        HideAllPanels();
        newGamePanel.SetActive(true);

        ShowNameInputStep();
    }
}
