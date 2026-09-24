using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatusSlotUI : MonoBehaviour
{
    private const string FallbackPlayerName = "할로";

    [SerializeField] private Image slotBackgroundImage;
    [SerializeField] private Image characterFaceImage;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text mpText;

    public Image SlotBackgroundImage => slotBackgroundImage;
    public Image CharacterFaceImage => characterFaceImage;

    public void Refresh()
    {
        Refresh(GameManager.Instance);
    }

    public void Refresh(GameManager gameManager)
    {
        if (gameManager == null)
        {
            SetText(characterNameText, "");
            SetText(hpText, "0");
            SetText(mpText, "0");
            return;
        }

        SetText(characterNameText, GetDisplayPlayerName(gameManager));
        SetText(hpText, gameManager.currentHP.ToString());
        SetText(mpText, gameManager.currentMP.ToString());
    }

    private string GetDisplayPlayerName(GameManager gameManager)
    {
        if (gameManager != null && !string.IsNullOrWhiteSpace(gameManager.playerName))
        {
            return gameManager.playerName;
        }

        return FallbackPlayerName;
    }

    private void SetText(TMP_Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }
}
