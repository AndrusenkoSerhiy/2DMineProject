using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Scriptables.Craft.Recipe;

namespace Craft {
  public class RecipeDetailRow : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI resourceName;
    [SerializeField] private Image resourceIcon;
    [SerializeField] private TextMeshProUGUI countText;

    public void SetRow(CraftingMaterial craftingMaterial) {
      resourceName.text = craftingMaterial.Material.data.Name;
      resourceIcon.sprite = craftingMaterial.Material.UiDisplay;
      resourceIcon.color = new Color(255, 255, 255, 255);
      //TODO: "22/$Amount"
      countText.text = craftingMaterial.Amount.ToString("n0");
    }

    public void ClearRow() {
      resourceName.text = "";
      resourceIcon.sprite = null;
      resourceIcon.color = new Color(255, 255, 255, 0);
      countText.text = "";
    }
  }
}