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
    [SerializeField] private Button cancel;

    private Recipe recipe;
    private int countLeft;
    private int position;
    private RectTransform rectTransform;

    public event Action<Recipe, int> OnItemCrafted;
    public event Action OnInputAllCrafted;
    public event Action<InputItem> OnCanceled;
    public Timer Timer => timer;
    public Recipe Recipe => recipe;
    public int CountLeft => countLeft;
    public int Position => position;

    public void Init(int count, Recipe recipe, int position, DateTime? start = null) {
      countLeft = count;
      this.recipe = recipe;
      this.position = position;
      rectTransform = GetComponent<RectTransform>();

      SetupUI();
      PrintCount();

      cancel.onClick.AddListener(CancelHandler);

      timer.enabled = true;
      timer.onTimerStop += OnTimerStopHandler;
      timer.onItemTimerEnd += OnItemTimerEndHandler;
      timer.InitTimer(count, recipe.CraftingTime, start);

      if (position == 0) {
        StartCrafting();
      }
    }

    public void UpdatePosition(int position) {
      this.position = position;
    }

    public void StartCrafting() {
      timer.StartTimer();
    }

    public void UpdateTransformPosition(Vector3 position) {
      rectTransform.position = position;
    }

    public Vector3 GetTransformPosition() {
      return rectTransform.position;
    }

    private void CancelHandler() {
      OnCanceled?.Invoke(this);
      ResetInput();
    }

    private void SetupUI() {
      icon.sprite = recipe.Result.UiDisplay;
      icon.color = new Color(1, 1, 1, 255);
      timeIcon.gameObject.SetActive(true);
      fade.gameObject.SetActive(true);
      cancel.gameObject.SetActive(true);
    }

    private void PrintCount() {
      countText.text = countLeft.ToString();
    }

    private void OnTimerStopHandler() {
      ResetInput();

      OnInputAllCrafted?.Invoke();
    }

    private void OnItemTimerEndHandler(int count) {
      OnItemCrafted?.Invoke(recipe, count);

      countLeft -= count;
      PrintCount();
    }

    private void ResetInput() {
      cancel.onClick.RemoveAllListeners();

      timer.onTimerStop -= OnTimerStopHandler;
      timer.onItemTimerEnd -= OnItemTimerEndHandler;
      timer.enabled = false;
      timer.Reset();

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