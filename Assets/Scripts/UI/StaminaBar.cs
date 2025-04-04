using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class StaminaBar : MonoBehaviour {

    [SerializeField] private Slider staminaSlider;

    public void SetStamina(float val) {
      staminaSlider.value = val;
    }

    public void SetMaxStamina(float val) {
      staminaSlider.maxValue = val;
    }
  }
}