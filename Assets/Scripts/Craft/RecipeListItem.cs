using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Craft {
  public class RecipeListItem : MonoBehaviour {
    [SerializeField] private Image background;
    [SerializeField] private Image recipeIcon;
    [SerializeField] private Image isNewIcon;
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Recipe recipe;
    [SerializeField] private Color bgColor;
    [SerializeField] private Color activeBgColor;
    [SerializeField] private Color textColor;
    [SerializeField] private Color activeTextColor;

    private bool isNew;

    public Recipe Recipe => recipe;
    public bool IsNew => isNew;

    public void SetRecipeDetails(string name, Sprite icon, Recipe recipe, bool isNew = false) {
      recipeNameText.text = name;
      recipeIcon.sprite = icon;
      isNewIcon.gameObject.SetActive(isNew);
      this.isNew = isNew;
      this.recipe = recipe;
    }

    public void SetActiveStyles() {
      background.color = activeBgColor;
      recipeNameText.color = activeTextColor;
    }

    public void ResetStyles() {
      recipeNameText.color = textColor;
      background.color = bgColor;

      if (isNew) {
        MarkAsSeen();
      }
    }

    public void MarkAsSeen() {
      isNew = false;
      isNewIcon.gameObject.SetActive(false);
    }
  }
}