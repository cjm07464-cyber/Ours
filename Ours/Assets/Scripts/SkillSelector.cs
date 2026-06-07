using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillSelector : MonoBehaviour
{
    [SerializeField] private RectTransform selector;
    [SerializeField] private RectTransform[] options;
    [SerializeField] private TextMeshProUGUI[] optionTexts;
    [SerializeField] private BattleManager battleManager;

    private readonly List<SkillData> currentSkills = new List<SkillData>();
    private int currentIndex;

    private void Awake()
    {
        if (selector == null)
        {
            selector = GetComponent<RectTransform>();
        }
    }

    private void OnEnable()
    {
        currentIndex = 0;
        MoveSelectorTo(currentIndex);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (battleManager != null)
            {
                battleManager.CloseSkillPanelAndReturnToCommand();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveSelection(1);
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveSelection(-1);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SelectCurrentSkill();
        }
    }

    public void SetSkills(IReadOnlyList<SkillData> skills)
    {
        currentSkills.Clear();

        if (skills != null)
        {
            for (int i = 0; i < skills.Count; i++)
            {
                if (skills[i] != null)
                {
                    currentSkills.Add(skills[i]);
                }
            }
        }

        currentIndex = 0;
        RefreshOptions();
        MoveSelectorTo(currentIndex);
    }

    private void RefreshOptions()
    {
        AutoResolveOptionTexts();

        int textCount = optionTexts == null ? 0 : optionTexts.Length;

        for (int i = 0; i < textCount; i++)
        {
            if (optionTexts[i] == null)
            {
                continue;
            }

            if (i < currentSkills.Count)
            {
                optionTexts[i].text = currentSkills[i].skillName;
            }
            else
            {
                optionTexts[i].text = "";
            }
        }

        if (currentSkills.Count == 0 && textCount > 0 && optionTexts[0] != null)
        {
            optionTexts[0].text = "사용 가능한 스킬이 없다.";
        }
    }

    private void AutoResolveOptionTexts()
    {
        if ((optionTexts != null && optionTexts.Length > 0) || options == null || options.Length == 0)
        {
            return;
        }

        optionTexts = new TextMeshProUGUI[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
            {
                optionTexts[i] = options[i].GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void MoveSelection(int direction)
    {
        if (currentSkills.Count == 0)
        {
            return;
        }

        int optionCount = GetSelectableOptionCount();
        if (optionCount == 0)
        {
            return;
        }

        currentIndex += direction;

        if (currentIndex < 0)
        {
            currentIndex = optionCount - 1;
        }
        else if (currentIndex >= optionCount)
        {
            currentIndex = 0;
        }

        MoveSelectorTo(currentIndex);
    }

    private int GetSelectableOptionCount()
    {
        int optionLength = options == null ? 0 : options.Length;
        return Mathf.Min(currentSkills.Count, optionLength);
    }

    private void SelectCurrentSkill()
    {
        if (battleManager == null || currentSkills.Count == 0)
        {
            return;
        }

        if (currentIndex < 0 || currentIndex >= currentSkills.Count)
        {
            return;
        }

        battleManager.OnSkillSelected(currentSkills[currentIndex]);
    }

    private void MoveSelectorTo(int index)
    {
        if (selector == null || options == null || options.Length == 0)
        {
            return;
        }

        if (index < 0 || index >= options.Length || options[index] == null)
        {
            return;
        }

        Vector3 basePos = options[index].position;
        selector.position = new Vector3(basePos.x - 90f, basePos.y, basePos.z);
    }
}
