using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class LocatorDistance : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private Image iconImage;

    public void SetPoint(Sprite targetSprite) {
      iconImage.sprite = targetSprite;
    }

    public void UpdateDistance(float distance) {
      distanceText.text = $"{distance:F1} m";
    }

    public void Show() {
      gameObject.SetActive(true);
    }

    public void Hide() {
      gameObject.SetActive(false);
    }
  }
}