using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Craft {
  public class RecipeDetailRow : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI resourceName;
    [SerializeField] private Image resourceIcon;
    [SerializeField] private TextMeshProUGUI totalAmountText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Color textColor;
    [SerializeField] private Color insufficientTotalAmountColor;

    public void SetRow(Recipe.CraftingMaterial craftingMaterial) {
      var totalAmount = GameManager.instance.PlayerInventory.GetResourceTotalAmount(craftingMaterial.Material.data.Id);

      SetName(craftingMaterial.Material.data.Name);
      SetSprite(craftingMaterial.Material.UiDisplay, new Color(255, 255, 255, 255));
      SetCount(craftingMaterial.Amount);
      SetTotalAmount(totalAmount, craftingMaterial.Amount);
    }

    private void SetSprite(Sprite sprite, Color color) {
      resourceIcon.sprite = sprite;
      resourceIcon.color = color;
    }

    private void SetName(string name) {
      resourceName.text = name;
    }

    private void SetCount(int amount) {
      countText.text = $"/{amount}";
      countText.color = textColor;
    }

    private void SetTotalAmount(int totalAmount, int amount) {
      totalAmountText.text = $"{totalAmount}";
      totalAmountText.color = amount > totalAmount ? insufficientTotalAmountColor : textColor;
    }

    public void ClearRow() {
      ClearName();
      ClearSprite();
      ClearCount();
      ClearTotalAmount();
    }

    private void ClearName() {
      resourceName.text = "";
    }

    private void ClearSprite() {
      resourceIcon.sprite = null;
      resourceIcon.color = new Color(255, 255, 255, 0);
    }

    private void ClearCount() {
      countText.text = "";
    }

    private void ClearTotalAmount() {
      totalAmountText.text = "";
    }
  }
}