using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillSelector : MonoBehaviour
{
    private enum SelectionStage
    {
        Category,
        Skill
    }

    private sealed class SkillRow
    {
        public readonly string displayName;
        public readonly List<SkillData> tiers = new List<SkillData>();

        public SkillRow(string displayName)
        {
            this.displayName = displayName;
        }
    }

    private static readonly SkillCategory[] Categories =
    {
        SkillCategory.Attack,
        SkillCategory.Heal,
        SkillCategory.Assist
    };

    [Header("Category Panel (Attack / Heal / Assist)")]
    [SerializeField] private RectTransform[] categoryOptions;

    [Header("Skill List Panel")]
    [SerializeField] private RectTransform[] options;
    [SerializeField] private TextMeshProUGUI[] optionTexts;

    [Header("Description Panel")]
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI mpCostText;

    [Header("Focus Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.5411765f, 0f, 1f);

    [Header("Audio")]
    [SerializeField] private AudioClip cursorMoveSound;
    [SerializeField, Min(0f)] private float cursorMoveVolumeScale = 1.3f;
    [SerializeField] private AudioClip commandConfirmSound;
    [SerializeField, Min(0f)] private float commandConfirmVolumeScale = 1.5f;

    [Header("Context")]
    [SerializeField] private BattleManager battleManager;

    private readonly List<SkillData> currentSkills = new List<SkillData>();
    private readonly List<SkillRow> visibleRows = new List<SkillRow>();
    private SelectionStage currentStage;
    private int currentCategoryIndex;
    private int currentRowIndex;
    private int currentTierIndex;

    private void OnEnable()
    {
        EnterCategoryStage(true);
    }

    private void Update()
    {
        if (GameInput.CancelPressed)
        {
            if (currentStage == SelectionStage.Skill)
            {
                EnterCategoryStage(false);
            }
            else if (battleManager != null)
            {
                battleManager.CloseSkillPanelAndReturnToCommand();
            }

            return;
        }

        if (currentStage == SelectionStage.Category)
        {
            HandleCategoryInput();
            return;
        }

        HandleSkillInput();
    }

    private void HandleCategoryInput()
    {
        if (GameInput.DownPressed)
        {
            MoveCategorySelection(1);
            return;
        }

        if (GameInput.UpPressed)
        {
            MoveCategorySelection(-1);
            return;
        }

        if (GameInput.ConfirmPressed && GetSelectableRowCount() > 0)
        {
            PlaySfx(commandConfirmSound, commandConfirmVolumeScale);
            EnterSkillStage();
        }
    }

    private void HandleSkillInput()
    {
        if (GameInput.DownPressed)
        {
            MoveRowSelection(1);
            return;
        }

        if (GameInput.UpPressed)
        {
            MoveRowSelection(-1);
            return;
        }

        if (GameInput.RightPressed)
        {
            MoveTierSelection(1);
            return;
        }

        if (GameInput.LeftPressed)
        {
            MoveTierSelection(-1);
            return;
        }

        if (GameInput.ConfirmPressed)
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

        EnterCategoryStage(true);
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

            optionTexts[i].color = normalColor;

            if (i < visibleRows.Count)
            {
                optionTexts[i].text = BuildRowText(visibleRows[i], i == currentRowIndex);
            }
            else
            {
                optionTexts[i].text = "";
            }
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

    private void EnterCategoryStage(bool resetCategory)
    {
        currentStage = SelectionStage.Category;

        if (resetCategory)
        {
            currentCategoryIndex = 0;
        }

        currentCategoryIndex = Mathf.Clamp(currentCategoryIndex, 0, Categories.Length - 1);
        currentRowIndex = 0;
        currentTierIndex = 0;
        RebuildVisibleRows();
        RefreshOptions();
        ClearDescription();
        RefreshCategoryFocus();
    }

    private void EnterSkillStage()
    {
        if (GetSelectableRowCount() == 0)
        {
            return;
        }

        currentStage = SelectionStage.Skill;
        currentRowIndex = 0;
        currentTierIndex = 0;
        RefreshOptions();
        RefreshCategoryFocus();
        RefreshDescription();
    }

    private void MoveCategorySelection(int direction)
    {
        int previousIndex = currentCategoryIndex;
        currentCategoryIndex += direction;

        if (currentCategoryIndex < 0)
        {
            currentCategoryIndex = Categories.Length - 1;
        }
        else if (currentCategoryIndex >= Categories.Length)
        {
            currentCategoryIndex = 0;
        }

        currentRowIndex = 0;
        currentTierIndex = 0;
        RebuildVisibleRows();
        RefreshOptions();
        ClearDescription();
        RefreshCategoryFocus();

        if (currentCategoryIndex != previousIndex)
        {
            PlaySfx(cursorMoveSound, cursorMoveVolumeScale);
        }
    }

    private void MoveRowSelection(int direction)
    {
        int rowCount = GetSelectableRowCount();
        if (rowCount == 0)
        {
            return;
        }

        int previousIndex = currentRowIndex;
        int nextIndex = (currentRowIndex + direction + rowCount) % rowCount;
        if (nextIndex == previousIndex)
        {
            return;
        }

        currentRowIndex = nextIndex;
        currentTierIndex = 0;
        RefreshOptions();
        RefreshDescription();
        PlaySfx(cursorMoveSound, cursorMoveVolumeScale);
    }

    private void MoveTierSelection(int direction)
    {
        SkillRow row = GetCurrentRow();
        if (row == null || row.tiers.Count <= 1)
        {
            return;
        }

        int previousIndex = currentTierIndex;
        currentTierIndex = (currentTierIndex + direction + row.tiers.Count) % row.tiers.Count;
        RefreshOptions();
        RefreshDescription();

        if (currentTierIndex != previousIndex)
        {
            PlaySfx(cursorMoveSound, cursorMoveVolumeScale);
        }
    }

    private void SelectCurrentSkill()
    {
        if (battleManager == null)
        {
            return;
        }

        SkillData skill = GetCurrentSkill();
        if (skill == null)
        {
            return;
        }

        PlaySfx(commandConfirmSound, commandConfirmVolumeScale);
        battleManager.OnSkillSelected(skill);
    }

    private static void PlaySfx(AudioClip clip, float volumeScale)
    {
        if (clip != null && SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayOneShot(clip, Mathf.Max(0f, volumeScale));
        }
    }

    private void RebuildVisibleRows()
    {
        visibleRows.Clear();
        SkillCategory selectedCategory = Categories[currentCategoryIndex];

        for (int i = 0; i < currentSkills.Count; i++)
        {
            SkillData skill = currentSkills[i];
            if (skill == null || ResolveCategory(skill) != selectedCategory)
            {
                continue;
            }

            string displayName = string.IsNullOrWhiteSpace(skill.skillName)
                ? skill.name
                : skill.skillName;
            SkillRow row = FindRow(displayName);
            if (row == null)
            {
                row = new SkillRow(displayName);
                visibleRows.Add(row);
            }

            row.tiers.Add(skill);
        }

        for (int i = 0; i < visibleRows.Count; i++)
        {
            visibleRows[i].tiers.Sort(
                (left, right) => left.skillTier.CompareTo(right.skillTier));
        }
    }

    private SkillRow FindRow(string displayName)
    {
        for (int i = 0; i < visibleRows.Count; i++)
        {
            if (visibleRows[i].displayName == displayName)
            {
                return visibleRows[i];
            }
        }

        return null;
    }

    private static SkillCategory ResolveCategory(SkillData skill)
    {
        if (skill.skillCategory != SkillCategory.Unspecified)
        {
            return skill.skillCategory;
        }

        return skill.skillType == SkillType.Heal
            ? SkillCategory.Heal
            : SkillCategory.Attack;
    }

    private string BuildRowText(SkillRow row, bool isCurrentRow)
    {
        string text = row.displayName;
        for (int i = 0; i < row.tiers.Count; i++)
        {
            string tierLabel = GetTierLabel(row.tiers[i].skillTier);
            if (currentStage == SelectionStage.Skill && isCurrentRow && i == currentTierIndex)
            {
                tierLabel = $"<color=#{ColorUtility.ToHtmlStringRGBA(selectedColor)}>{tierLabel}</color>";
            }

            text += $"   {tierLabel}";
        }

        return text;
    }

    private static string GetTierLabel(SkillTier tier)
    {
        switch (tier)
        {
            case SkillTier.Beta:
                return "β";
            case SkillTier.Gamma:
                return "γ";
            case SkillTier.Omega:
                return "Ω";
            default:
                return "α";
        }
    }

    private int GetSelectableRowCount()
    {
        int optionLength = options == null ? 0 : options.Length;
        int textLength = optionTexts == null ? 0 : optionTexts.Length;
        int slotCount = Mathf.Min(optionLength, textLength);
        return Mathf.Min(visibleRows.Count, slotCount);
    }

    private SkillRow GetCurrentRow()
    {
        int rowCount = GetSelectableRowCount();
        if (currentRowIndex < 0 || currentRowIndex >= rowCount)
        {
            return null;
        }

        return visibleRows[currentRowIndex];
    }

    private SkillData GetCurrentSkill()
    {
        SkillRow row = GetCurrentRow();
        if (row == null || currentTierIndex < 0 || currentTierIndex >= row.tiers.Count)
        {
            return null;
        }

        return row.tiers[currentTierIndex];
    }

    private void RefreshDescription()
    {
        SkillData skill = GetCurrentSkill();
        if (skill == null)
        {
            ClearDescription();
            return;
        }

        if (descriptionText != null)
        {
            descriptionText.text = skill.description ?? string.Empty;
        }

        if (mpCostText != null)
        {
            mpCostText.text = $"소비MP : {skill.mpCost}";
        }
    }

    private void ClearDescription()
    {
        if (descriptionText != null)
        {
            descriptionText.text = string.Empty;
        }

        if (mpCostText != null)
        {
            mpCostText.text = string.Empty;
        }
    }

    private void RefreshCategoryFocus()
    {
        if (categoryOptions == null)
        {
            return;
        }

        for (int i = 0; i < categoryOptions.Length; i++)
        {
            if (categoryOptions[i] == null)
            {
                continue;
            }

            TextMeshProUGUI categoryText = categoryOptions[i].GetComponent<TextMeshProUGUI>();
            if (categoryText == null)
            {
                categoryText = categoryOptions[i].GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (categoryText != null)
            {
                categoryText.color = i == currentCategoryIndex
                    ? selectedColor
                    : normalColor;
            }
        }
    }
}
