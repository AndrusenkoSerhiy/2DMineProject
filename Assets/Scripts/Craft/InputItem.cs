using System;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Craft {
  public class InputItem : MonoBehaviour {
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image timeIcon;
    [SerializeField] private Image fade;
    [SerializeField] private Timer timer;

    public Action<Recipe, int> onItemCrafted;
    private Recipe recipe;
    private int countLeft;

    public void Init(int count, Recipe recipe) {
      Debug.Log("InputItem Init");
      countLeft = count;
      this.recipe = recipe;

      icon.sprite = recipe.Result.UiDisplay;
      icon.color = new Color(1, 1, 1, 255);
      timeIcon.gameObject.SetActive(true);
      fade.gameObject.SetActive(true);

      PrintCount();

      timer.enabled = true;
      timer.onTimerStop += OnTimerStopHandler;
      timer.onItemTimerEnd += OnItemTimerEndHandler;
      timer.StartTimer(count, recipe.CraftingTime);
    }

    private void PrintCount() {
      countText.text = countLeft.ToString();
    }

    private void OnTimerStopHandler() {
      Debug.Log("InputItem OnTimerStop");
      ResetInput();
    }

    private void OnItemTimerEndHandler(int count) {
      Debug.Log("InputItem OnItemTimerEndHandler: " + count);
      onItemCrafted?.Invoke(recipe, count);

      countLeft -= count;
      PrintCount();
    }

    private void ResetInput() {
      Debug.Log("InputItem ResetInput");
      timer.onTimerStop -= OnTimerStopHandler;
      timer.onItemTimerEnd -= OnItemTimerEndHandler;
      timer.enabled = false;

      countLeft = 0;
      recipe = null;

      icon.sprite = null;
      icon.color = new Color(1, 1, 1, 0);
      timeIcon.gameObject.SetActive(false);
      fade.gameObject.SetActive(false);
      countText.text = string.Empty;
    }
  }
}