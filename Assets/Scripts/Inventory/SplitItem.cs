using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Math = System.Math;

namespace Inventory {
  public class SplitItem : MonoBehaviour {
    [SerializeField] private GameObject splitWindow;
    [SerializeField] private TMP_InputField countInput;
    [SerializeField] private Button incrementButton;
    [SerializeField] private Button decrementButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button okButton;
    [SerializeField] private GameObject tempDragItemObject;
    [SerializeField] private TempDragItem tempDragItem;

    private Color buttonsActiveColor;
    private Color buttonsDisabledColor;
    private Transform tempDragParent;

    private InventorySlot slot;
    private int minCount = 1;
    private int maxCount;
    private int currentCount;
    private bool active;

    public bool Active => active;
    public InventorySlot Slot => slot;

    public void Awake() {
      var uiSettings = GameManager.Instance.UISettings;
      buttonsActiveColor = uiSettings.buttonsActiveColor;
      buttonsDisabledColor = uiSettings.buttonsDisabledColor;
    }

    public void OnEnable() {
      AddEvents();
    }

    public void OnDisable() {
      RemoveEvents();
    }

    public void Update() {
      if (active) {
        var mousePos = GameManager.Instance.UserInput.GetMousePosition();
        tempDragItem.UpdatePosition(mousePos);
      }
      else {
        if (GameManager.Instance.UserInput.controls.UI.Cancel.triggered
            || Input.GetMouseButtonDown(0) && !IsPointerOverUIObject()) {
          Cancel();
        }
      }
    }

    public void Show(InventorySlot slot, GameObject obj, Transform tempDragParent) {
      if (slot.amount <= 1) {
        return;
      }

      this.slot = slot;
      this.tempDragParent = tempDragParent;

      maxCount = slot.amount;
      SetCurrentCount(CalculateCurrentCount());
      PrintInputCount();

      SetPosition(obj);
      splitWindow.SetActive(true);
      gameObject.SetActive(true);
      EnableButtons();
    }

    public void End(bool success) {
      if (!success) {
        slot.AddAmount(currentCount);
      }

      Disable();
    }

    private void HideWindow() {
      splitWindow.SetActive(false);
    }

    private void Disable() {
      gameObject.SetActive(false);
      slot = null;
      tempDragParent = null;
      active = false;
    }

    private void OnOkButtonClickHandler() {
      GameManager.Instance.AudioController.PlayUIClick();

      active = true;
      tempDragItem.Enable(slot.Item, slot.amount, tempDragParent);
      tempDragItem.SetAmount(currentCount);

      if (currentCount == slot.amount) {
        var tmp = slot.Clone();
        slot.RemoveItem();
        slot = tmp;
      }
      else {
        slot.RemoveAmount(currentCount);
      }


      HideWindow();
    }

    private void OnCancelButtonClickHandler() {
      GameManager.Instance.AudioController.PlayUIClick();
      Cancel();
    }

    private void Cancel() {
      HideWindow();
      Disable();
    }

    private void SetPosition(GameObject obj) {
      var slotRect = obj.GetComponent<RectTransform>();
      var splitRect = GetComponent<RectTransform>();

      var slotPosition = slotRect.position;
      splitRect.position = new Vector3(slotPosition.x, slotPosition.y + slotRect.rect.height, slotPosition.z);
    }

    private bool IsPointerOverUIObject() {
      var eventData = new PointerEventData(EventSystem.current);
      eventData.position = Input.mousePosition;

      var results = new System.Collections.Generic.List<RaycastResult>();
      EventSystem.current.RaycastAll(eventData, results);

      foreach (var result in results) {
        if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform)) {
          return true;
        }
      }

      return false;
    }

    private int CalculateCurrentCount() {
      return maxCount > 1 ? maxCount / 2 : maxCount;
    }

    private void SetCurrentCount(int count) {
      currentCount = count;
    }

    private void PrintInputCount() {
      countInput.text = currentCount.ToString();
    }

    private void EnableButtons() {
      EnableButton(decrementButton, currentCount > minCount);
      EnableButton(incrementButton, currentCount < maxCount);
    }

    private void EnableButton(Button button, bool state) {
      var color = state ? buttonsActiveColor : buttonsDisabledColor;
      button.enabled = state;
      button.image.color = color;
    }

    private void AddEvents() {
      countInput.onValueChanged.AddListener(OnCountInputChangeHandler);
      incrementButton.onClick.AddListener(OnIncrementClickHandler);
      decrementButton.onClick.AddListener(OnDecrementClickHandler);
      cancelButton.onClick.AddListener(OnCancelButtonClickHandler);
      okButton.onClick.AddListener(OnOkButtonClickHandler);
    }

    private void RemoveEvents() {
      countInput.onValueChanged.RemoveAllListeners();
      incrementButton.onClick.RemoveAllListeners();
      decrementButton.onClick.RemoveAllListeners();
      cancelButton.onClick.RemoveAllListeners();
      okButton.onClick.RemoveAllListeners();
    }

    private void OnCountInputChangeHandler(string value) {
      if (value == string.Empty) {
        return;
      }

      var count = int.Parse(value);

      count = Math.Clamp(count, minCount, maxCount);

      SetCurrentCount(count);
      PrintInputCount();
      EnableButtons();
    }

    private void OnIncrementClickHandler() {
      if (currentCount >= maxCount) {
        return;
      }

      GameManager.Instance.AudioController.PlayUIClick();
      SetCurrentCount(currentCount + 1);
      PrintInputCount();
    }

    private void OnDecrementClickHandler() {
      if (currentCount <= minCount) {
        return;
      }

      GameManager.Instance.AudioController.PlayUIClick();
      SetCurrentCount(currentCount - 1);
      PrintInputCount();
    }
  }
}