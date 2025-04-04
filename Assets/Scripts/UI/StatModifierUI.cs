using Stats;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class StatModifierUI : MonoBehaviour {
  [SerializeField] private Image image;
  [SerializeField] private Image border;
  [SerializeField] protected TooltipTrigger tooltipTrigger;

  private StatModifier statModifier;

  public void Update() {
    if (statModifier == null || statModifier.TimeLeft <= 0) {
      return;
    }

    SetContent();
    tooltipTrigger.UpdateText();
  }

  public void Show(StatModifier modifier) {
    statModifier = modifier;
    image.sprite = modifier.modifierDisplayObject.display;
    image.color = modifier.modifierDisplayObject.iconColor;
    border.sprite = modifier.modifierDisplayObject.border;
    border.color = modifier.modifierDisplayObject.borderColor;
    tooltipTrigger.header = modifier.modifierDisplayObject.name;
    SetContent();
    ActivateItem(true);
  }

  public void Hide() {
    ActivateItem(false);
    statModifier = null;
    image.sprite = null;
    image.color = new Color(255, 255, 255, 0);
    HideBorder();
    ClearTooltip();
  }

  private void ClearTooltip() {
    tooltipTrigger.header = string.Empty;
    tooltipTrigger.content = string.Empty;
  }

  private void HideBorder() {
    border.sprite = null;
  }

  private void ActivateItem(bool state) {
    gameObject.SetActive(state);
  }

  private void SetContent() {
    tooltipTrigger.content = statModifier.modifierDisplayObject.description;
    tooltipTrigger.content += "\n";
    tooltipTrigger.content += "Value: " + statModifier.Value;
    if (statModifier.Duration > 0) {
      tooltipTrigger.content += ", Duration: " + (int)statModifier.TimeLeft;
    }
  }
}