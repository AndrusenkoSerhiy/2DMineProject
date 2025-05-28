using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class LocatorPoint : MonoBehaviour {
    [SerializeField] private Transform arrowTransform;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image arrowImage;

    public void SetPoint(Sprite targetSprite, Color color) {
      iconImage.sprite = targetSprite;
      arrowImage.color = color;
    }

    public void UpdateArrow(Vector3 directionToTarget) {
      arrowTransform.up = directionToTarget;
      iconImage.rectTransform.rotation = Quaternion.identity;
    }

    public void Show() {
      gameObject.SetActive(true);
    }

    public void Hide() {
      gameObject.SetActive(false);
    }
  }
}