using System;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public struct ItemCanceledEventData {
    public Recipe Recipe;
    public int Position;
    public int CountLeft;
  }

  public class InputItem : MonoBehaviour {
    [SerializeField] protected Image icon;
    [SerializeField] protected TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] protected Image timeIcon;
    [SerializeField] protected Image fade;
    [SerializeField] protected Button cancel;

    protected Workstation station;
    protected Recipe recipe;
    protected int countLeft;
    protected int position;
    protected RectTransform rectTransform;

    public event Action<ItemCanceledEventData> OnCanceled;
    public Recipe Recipe => recipe;
    public int CountLeft => countLeft;
    public int Position => position;

    public void Awake() {
      rectTransform = GetComponent<RectTransform>();
      station = ServiceLocator.For(this).Get<Workstation>();
    }

    public void SetPosition(int position) {
      this.position = position;
    }

    public virtual void Init(int count, Recipe recipe) {
      this.recipe = recipe;
      countLeft = count;

      SetupUI();
      PrintCount();
      PrintTime();

      cancel.onClick.AddListener(CancelHandler);
    }

    private void PrintTime() {
      timerText.text = Helper.SecondsToTimeString(countLeft * recipe.CraftingTime);
    }

    private void CancelHandler() {
      if (!station.CanCancelCraft(recipe, countLeft)) {
        GameManager.Instance.MessagesManager.ShowSimpleMessage(
          "You can't cancel craft. Not enough space in inventory.");
        return;
      }

      var data = new ItemCanceledEventData { Recipe = Recipe, Position = Position, CountLeft = countLeft };
      ResetInput();
      OnCanceled?.Invoke(data);
    }

    private void SetupUI() {
      icon.sprite = recipe.Result.UiDisplay;
      icon.color = new Color(1, 1, 1, 255);
      timeIcon.gameObject.SetActive(true);
      fade.gameObject.SetActive(true);
      cancel.gameObject.SetActive(true);
    }

    protected void PrintCount() {
      countText.text = countLeft.ToString();
    }

    public virtual void ResetInput() {
      cancel.onClick.RemoveAllListeners();

      countLeft = 0;
      recipe = null;

      icon.sprite = null;
      icon.color = new Color(1, 1, 1, 0);
      timeIcon.gameObject.SetActive(false);
      fade.gameObject.SetActive(false);
      cancel.gameObject.SetActive(false);
      countText.text = string.Empty;
    }
  }
}