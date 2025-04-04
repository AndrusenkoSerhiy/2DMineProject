using Scriptables.Stats;
using UI;
using UnityEngine;
using UnityEngine.UI;
using StatModifier = Stats.StatModifier;

public class FoodModifierUI : MonoBehaviour {
  [SerializeField] private Image image;
  [SerializeField] private Image border;
  [SerializeField] protected TooltipTrigger tooltipTrigger;
  [SerializeField] private string emptyText = "Empty food slot";
  [SerializeField] private Color emptyColor;
  [SerializeField] private ModifierDisplayObject modifier;

  private StatModifier statModifier;

  private void Start() {
    SetDefaultImages();
    ClearTooltip();
  }

  public void Update() {
    if (statModifier == null || statModifier.TimeLeft <= 0) {
      return;
    }

    SetContent();
    tooltipTrigger.UpdateText();
  }

  public string Id => modifier.Id;

  public void Show(StatModifier modifier) {
    statModifier = modifier;
    image.sprite = modifier.modifierDisplayObject.display;
    image.color = modifier.modifierDisplayObject.iconColor;
    border.sprite = modifier.modifierDisplayObject.border;
    border.color = modifier.modifierDisplayObject.borderColor;
    tooltipTrigger.header = modifier.modifierDisplayObject.name;
    SetContent();
  }

  public void Hide() {
    statModifier = null;
    SetDefaultImages();
    ClearTooltip();
  }

  private void SetContent() {
    tooltipTrigger.content = statModifier.modifierDisplayObject.description;
    tooltipTrigger.content += "\n";
    tooltipTrigger.content += "Value: " + statModifier.Value;
    if (statModifier.Duration > 0) {
      tooltipTrigger.content += ", Duration: " + (int)statModifier.TimeLeft;
    }
  }

  private void SetDefaultImages() {
    image.sprite = modifier.display;
    border.sprite = modifier.border;
    image.color = emptyColor;
    border.color = emptyColor;
  }

  private void ClearTooltip() {
    tooltipTrigger.header = string.Empty;
    tooltipTrigger.content = emptyText;
  }
}