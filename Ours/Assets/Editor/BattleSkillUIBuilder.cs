using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BattleSkillUIBuilder
{
    private const string MenuPath = "Tools/Ours/Build Battle Skill UI";
    private const string UndoName = "Build Battle Skill UI";

    [MenuItem(MenuPath)]
    private static void Build()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogWarning("Battle Skill UI Builder: 열린 Scene이 없습니다.");
            return;
        }

        if (scene.name != "BattleScene")
        {
            Debug.LogWarning("Battle Skill UI Builder: BattleScene을 연 뒤 실행해 주세요.");
            return;
        }

        BattleManager battleManager = FindComponentInScene<BattleManager>(scene);
        if (battleManager == null)
        {
            Debug.LogWarning("Battle Skill UI Builder: BattleManager를 찾지 못했습니다.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(UndoName);

        try
        {
            GameObject skillPanel = ResolveOrCreateSkillPanel(scene, battleManager);
            if (skillPanel == null)
            {
                Undo.RevertAllDownToGroup(undoGroup);
                return;
            }

            Image panelStyle = skillPanel.GetComponent<Image>();
            TextMeshProUGUI textStyle = FindTextStyleSource(skillPanel);

            RectTransform categoryPanel = GetOrCreatePanel(
                skillPanel.transform,
                "CategoryPanel",
                panelStyle,
                new Vector2(0f, 0.42f),
                new Vector2(0.27f, 1f),
                new Vector2(8f, 8f),
                new Vector2(-4f, -8f));

            RectTransform skillListPanel = GetOrCreatePanel(
                skillPanel.transform,
                "SkillListPanel",
                panelStyle,
                new Vector2(0.27f, 0.42f),
                new Vector2(1f, 1f),
                new Vector2(4f, 8f),
                new Vector2(-8f, -8f));

            RectTransform descriptionPanel = GetOrCreatePanel(
                skillPanel.transform,
                "DescriptionPanel",
                panelStyle,
                new Vector2(0f, 0f),
                new Vector2(1f, 0.4f),
                new Vector2(8f, 8f),
                new Vector2(-8f, -4f));

            RectTransform[] categoryOptions = new RectTransform[3];
            categoryOptions[0] = GetOrCreateOption(
                skillPanel.transform, categoryPanel, "Attack", "공격", 0, textStyle);
            categoryOptions[1] = GetOrCreateOption(
                skillPanel.transform, categoryPanel, "Heal", "회복", 1, textStyle);
            categoryOptions[2] = GetOrCreateOption(
                skillPanel.transform, categoryPanel, "Assist", "어시스트", 2, textStyle);

            RectTransform[] skillOptions = new RectTransform[4];
            TextMeshProUGUI[] skillOptionTexts = new TextMeshProUGUI[4];
            for (int i = 0; i < skillOptions.Length; i++)
            {
                string optionName = $"SkillOption{i + 1}";
                skillOptions[i] = GetOrCreateOption(
                    skillPanel.transform, skillListPanel, optionName, string.Empty, i, textStyle);
                skillOptionTexts[i] = skillOptions[i].GetComponent<TextMeshProUGUI>();
            }

            TextMeshProUGUI descriptionText = GetOrCreateDescriptionText(
                descriptionPanel,
                "DescriptionText",
                textStyle,
                new Vector2(0f, 0.34f),
                Vector2.one);

            TextMeshProUGUI mpCostText = GetOrCreateDescriptionText(
                descriptionPanel,
                "MpCostText",
                textStyle,
                Vector2.zero,
                new Vector2(1f, 0.34f));

            SkillSelector skillSelector = ResolveSkillSelector(skillPanel);
            ConnectBattleManager(battleManager, skillPanel, skillSelector);

            if (skillSelector != null)
            {
                ConnectSkillSelector(
                    skillSelector,
                    categoryOptions,
                    skillOptions,
                    skillOptionTexts,
                    descriptionText,
                    mpCostText,
                    battleManager);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = skillPanel;
            EditorGUIUtility.PingObject(skillPanel);
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log(
                "Battle Skill UI Builder: SkillPanel 3창 구성과 SerializedField 연결을 완료했습니다. " +
                "Edit > Undo Build Battle Skill UI로 되돌릴 수 있습니다.");
        }
        catch (System.Exception exception)
        {
            Undo.RevertAllDownToGroup(undoGroup);
            Debug.LogException(exception);
        }
    }

    private static T FindComponentInScene<T>(Scene scene) where T : Component
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            T component = roots[i].GetComponentInChildren<T>(true);
            if (component != null)
            {
                return component;
            }
        }

        return null;
    }

    private static GameObject ResolveOrCreateSkillPanel(Scene scene, BattleManager battleManager)
    {
        SerializedObject managerObject = new SerializedObject(battleManager);
        SerializedProperty skillPanelProperty = managerObject.FindProperty("skillPanel");
        GameObject skillPanel = skillPanelProperty != null
            ? skillPanelProperty.objectReferenceValue as GameObject
            : null;

        if (skillPanel == null || skillPanel.scene != scene)
        {
            Transform existing = FindTransformInScene(scene, "SkillPanel");
            if (existing != null)
            {
                skillPanel = existing.gameObject;
            }
        }

        if (skillPanel != null)
        {
            return skillPanel;
        }

        Transform parent = FindTransformInScene(scene, "BattleUI");
        if (parent == null)
        {
            parent = FindTransformInScene(scene, "Canvas");
        }

        if (parent == null)
        {
            Debug.LogWarning("Battle Skill UI Builder: BattleUI 또는 Canvas를 찾지 못했습니다.");
            return null;
        }

        skillPanel = CreateUIObject("SkillPanel", parent).gameObject;
        RectTransform rect = skillPanel.GetComponent<RectTransform>();
        Undo.RecordObject(rect, UndoName);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1000f, 520f);
        rect.anchoredPosition = Vector2.zero;

        Image image = Undo.AddComponent<Image>(skillPanel);
        image.color = new Color(0f, 0f, 0f, 0.7f);
        skillPanel.SetActive(false);
        return skillPanel;
    }

    private static RectTransform GetOrCreatePanel(
        Transform root,
        string name,
        Image styleSource,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        RectTransform rect = GetOrCreateRectTransform(root, root, name);
        ConfigureStretchRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);

        Image image = rect.GetComponent<Image>();
        if (image == null)
        {
            image = Undo.AddComponent<Image>(rect.gameObject);
        }

        Undo.RecordObject(image, UndoName);
        if (styleSource != null)
        {
            image.sprite = styleSource.sprite;
            image.type = styleSource.type;
            image.color = styleSource.color;
            image.pixelsPerUnitMultiplier = styleSource.pixelsPerUnitMultiplier;
        }
        else
        {
            image.color = new Color(0f, 0f, 0f, 0.7f);
        }

        image.raycastTarget = false;
        return rect;
    }

    private static RectTransform GetOrCreateOption(
        Transform searchRoot,
        RectTransform parent,
        string name,
        string defaultText,
        int index,
        TextMeshProUGUI styleSource)
    {
        RectTransform rect = GetOrCreateRectTransform(searchRoot, parent, name);
        Undo.RecordObject(rect, UndoName);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(-24f, 52f);
        rect.anchoredPosition = new Vector2(0f, -12f - index * 60f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        TextMeshProUGUI text = rect.GetComponent<TextMeshProUGUI>();
        bool createdText = text == null;
        if (createdText)
        {
            text = Undo.AddComponent<TextMeshProUGUI>(rect.gameObject);
        }

        ApplyTextStyle(text, styleSource);
        if (createdText || !string.IsNullOrEmpty(defaultText))
        {
            Undo.RecordObject(text, UndoName);
            text.text = defaultText;
        }

        return rect;
    }

    private static TextMeshProUGUI GetOrCreateDescriptionText(
        RectTransform parent,
        string name,
        TextMeshProUGUI styleSource,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        RectTransform rect = GetOrCreateRectTransform(parent, parent, name);
        ConfigureStretchRect(
            rect,
            anchorMin,
            anchorMax,
            new Vector2(24f, 8f),
            new Vector2(-24f, -8f));

        TextMeshProUGUI text = rect.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = Undo.AddComponent<TextMeshProUGUI>(rect.gameObject);
            text.text = string.Empty;
        }

        ApplyTextStyle(text, styleSource);
        return text;
    }

    private static RectTransform GetOrCreateRectTransform(
        Transform searchRoot,
        Transform parent,
        string name)
    {
        Transform existing = FindDescendant(searchRoot, name);
        RectTransform rect;

        if (existing != null)
        {
            rect = existing as RectTransform;
            if (rect == null)
            {
                throw new System.InvalidOperationException(
                    $"Battle Skill UI Builder: {name}에 RectTransform이 없습니다.");
            }

            if (rect.parent != parent)
            {
                Undo.SetTransformParent(rect, parent, UndoName);
            }
        }
        else
        {
            rect = CreateUIObject(name, parent);
        }

        return rect;
    }

    private static RectTransform CreateUIObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(gameObject, UndoName);
        Undo.SetTransformParent(gameObject.transform, parent, UndoName);
        return gameObject.GetComponent<RectTransform>();
    }

    private static void ConfigureStretchRect(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        Undo.RecordObject(rect, UndoName);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void ApplyTextStyle(TextMeshProUGUI text, TextMeshProUGUI styleSource)
    {
        Undo.RecordObject(text, UndoName);
        if (styleSource != null && styleSource != text)
        {
            text.font = styleSource.font;
            text.fontSize = styleSource.fontSize;
            text.fontStyle = styleSource.fontStyle;
            text.color = styleSource.color;
        }
        else if (text.fontSize <= 0f)
        {
            text.fontSize = 36f;
        }

        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;
    }

    private static TextMeshProUGUI FindTextStyleSource(GameObject skillPanel)
    {
        TextMeshProUGUI[] texts = skillPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].name.StartsWith("SkillOption"))
            {
                return texts[i];
            }
        }

        return texts.Length > 0 ? texts[0] : null;
    }

    private static SkillSelector ResolveSkillSelector(GameObject skillPanel)
    {
        SkillSelector rootSelector = skillPanel.GetComponent<SkillSelector>();
        if (rootSelector != null)
        {
            return rootSelector;
        }

        SkillSelector existingSelector = skillPanel.GetComponentInChildren<SkillSelector>(true);
        SkillSelector newSelector = Undo.AddComponent<SkillSelector>(skillPanel);

        if (existingSelector != null)
        {
            EditorUtility.CopySerialized(existingSelector, newSelector);
            Undo.RecordObject(existingSelector, UndoName);
            existingSelector.enabled = false;
            EditorUtility.SetDirty(existingSelector);
        }

        return newSelector;
    }

    private static void ConnectSkillSelector(
        SkillSelector selector,
        RectTransform[] categoryOptions,
        RectTransform[] skillOptions,
        TextMeshProUGUI[] skillOptionTexts,
        TextMeshProUGUI descriptionText,
        TextMeshProUGUI mpCostText,
        BattleManager battleManager)
    {
        Undo.RecordObject(selector, UndoName);
        SerializedObject serializedSelector = new SerializedObject(selector);
        serializedSelector.Update();

        SetArrayReferences(serializedSelector, "categoryOptions", categoryOptions);
        SetArrayReferences(serializedSelector, "options", skillOptions);
        SetArrayReferences(serializedSelector, "optionTexts", skillOptionTexts);
        SetObjectReference(serializedSelector, "descriptionText", descriptionText);
        SetObjectReference(serializedSelector, "mpCostText", mpCostText);
        SetObjectReference(serializedSelector, "battleManager", battleManager);

        serializedSelector.ApplyModifiedProperties();
        EditorUtility.SetDirty(selector);
    }

    private static void ConnectBattleManager(
        BattleManager battleManager,
        GameObject skillPanel,
        SkillSelector skillSelector)
    {
        Undo.RecordObject(battleManager, UndoName);
        SerializedObject serializedManager = new SerializedObject(battleManager);
        serializedManager.Update();
        SetObjectReference(serializedManager, "skillPanel", skillPanel);
        if (skillSelector != null)
        {
            SetObjectReference(serializedManager, "skillSelector", skillSelector);
        }

        serializedManager.ApplyModifiedProperties();
        EditorUtility.SetDirty(battleManager);
    }

    private static void SetObjectReference(
        SerializedObject serializedObject,
        string propertyName,
        Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning(
                $"Battle Skill UI Builder: {serializedObject.targetObject.name}.{propertyName} 필드를 찾지 못했습니다.");
            return;
        }

        property.objectReferenceValue = value;
    }

    private static void SetArrayReferences<T>(
        SerializedObject serializedObject,
        string propertyName,
        T[] values) where T : Object
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning(
                $"Battle Skill UI Builder: {serializedObject.targetObject.name}.{propertyName} 필드를 찾지 못했습니다.");
            return;
        }

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static Transform FindTransformInScene(Scene scene, string name)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform found = FindDescendant(roots[i].transform, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static Transform FindDescendant(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDescendant(root.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
