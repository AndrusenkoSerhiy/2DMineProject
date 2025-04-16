using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class DurabilitySlider : MonoBehaviour {
    [SerializeField] private Slider slider;

    [Tooltip("Durability values for each color")] [SerializeField]
    private List<int> durabilityValues;

    [SerializeField] private List<Color> colors;
    [SerializeField] private Image fillImage;

    private Color currentColor;
    
    private void OnEnable() {
      Debug.Log("DurabilitySlider OnEnable");
    }

    public void Show() {
      slider.gameObject.SetActive(true);
    }

    public void Hide() {
      slider.gameObject.SetActive(false);
    }

    public bool IsShown() {
      return slider.gameObject.activeSelf;
    }

    public void UpdateValue(float value) {
      slider.value = value;

      SetColor(value);
    }

    public void UpdateMaxValue(float value) {
      slider.maxValue = value;
    }

    private void SetColor(float value) {
      foreach (var durabilityValue in durabilityValues) {
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