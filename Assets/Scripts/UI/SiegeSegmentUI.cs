using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class SiegeSegmentUI : MonoBehaviour {
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image image;

    public RectTransform GetRectTransform() {
      if (rectTransform == null) {
        rectTransform = GetComponent<RectTransform>();
      }

      return rectTransform;
    }

    public Image GetImage() {
      if (image == null) {
        image = GetComponent<Image>();
      }

      return image;
    }
  }
}