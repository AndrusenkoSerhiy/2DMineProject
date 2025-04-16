using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  public class SlotDisplayWithDurability : SlotDisplay {
    [SerializeField] private Slider slider;

    [Tooltip("Durability values for each color")] [SerializeField]
    private List<int> durabilityValues;

    [SerializeField] private List<Color> colors;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image bg;
    [SerializeField] private Color bgDefaultColor;
    [SerializeField] private Color bgBrokenColor;

    private Color currentColor;

    public override void UpdateUI(InventorySlot slot) {
      RemoveItemEvents();

      base.UpdateUI(slot);

      if (currentItem.DurabilityNotFull()) {
        ShowSlider();

        UpdateSliderMaxValue(currentItem.MaxDurability);
        UpdateSliderValue(currentItem.Durability);
      }
      else {
        HideSlider();
      }

      UpdateBgColor();

      AddItemEvents();
    }

    private void RemoveItemEvents() {
      if (currentItem == null || type != InventoryType.QuickSlots) {
        return;
      }

      currentItem.OnDurabilityChanged -= OnDurabilityChangedHandler;
    }

    private void AddItemEvents() {
      if (currentItem == null || type != InventoryType.QuickSlots) {
        return;
      }

      currentItem.OnDurabilityChanged += OnDurabilityChangedHandler;
    }

    private void OnDurabilityChangedHandler(float before, float after) {
      var isFullDurability = Mathf.Approximately(after, currentItem.MaxDurability);
      var isSliderVisible = IsSliderShown();

      if (isFullDurability && isSliderVisible) {
        HideSlider();
        return;
      }

      if (!isFullDurability && !isSliderVisible) {
        ShowSlider();
        UpdateSliderMaxValue(currentItem.MaxDurability);
      }

      UpdateSliderValue(after);
    }

    private void ShowSlider() {
      if (IsSliderShown()) {
        return;
      }

      slider.gameObject.SetActive(true);
    }

    private void HideSlider() {
      if (!IsSliderShown()) {
        return;
      }

      slider.gameObject.SetActive(false);
    }

    private bool IsSliderShown() {
      return slider.gameObject.activeSelf;
    }

    private void UpdateSliderValue(float value) {
      slider.value = value;

      SetSliderColor(value);
      UpdateBgColor();
    }

    private void UpdateSliderMaxValue(float value) {
      slider.maxValue = value;
    }

    private void UpdateBgColor() {
      var color = currentItem.IsBroken ? bgBrokenColor : bgDefaultColor;
      if (bg.color == color) {
        return;
      }

      bg.color = color;
    }

    private void SetSliderColor(float value) {
      for (var i = durabilityValues.Count - 1; i >= 0; i--) {
        var durabilityValue = durabilityValues[i];
        if (!(value >= durabilityValue)) {
          continue;
        }

        var color = colors[durabilityValues.IndexOf(durabilityValue)];
        if (currentColor == color) {
          return;
        }

        fillImage.color = color;
        currentColor = color;
        return;
      }
    }
  }
}