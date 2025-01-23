using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Craft {
  public class RecipeListItem : MonoBehaviour {
    [SerializeField] private Image background;
    [SerializeField] private Image recipeIcon;
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Recipe recipe;
    [SerializeField] private Color bgColor;
    [SerializeField] private Color activeBgColor;
    [SerializeField] private Color textColor;
    [SerializeField] private Color activeTextColor;

    public Recipe Recipe => recipe;

    public void SetRecipeDetails(string name, Sprite icon, Recipe recipe) {
      recipeNameText.text = name;
      recipeIcon.sprite = icon;
      this.recipe = recipe;
    }

    public void SetActiveStyles() {
      background.color = activeBgColor;
      recipeNameText.color = activeTextColor;
    }

    public void ResetStyles() {
      recipeNameText.color = textColor;
      background.color = bgColor;
    }
  }
}