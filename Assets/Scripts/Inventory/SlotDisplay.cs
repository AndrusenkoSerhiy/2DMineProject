using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  public class SlotDisplay : MonoBehaviour {
    [SerializeField] private GameObject outline;
    [SerializeField] private Image background;
    [SerializeField] private Image typeIcon;
    [SerializeField] private TextMeshProUGUI text;

    public TextMeshProUGUI Text => text;
    public Image Background => background;
    public Image TypeIcon => typeIcon;

    public void ActivateOutline() {
      outline.SetActive(true);
    }

    public void DeactivateOutline() {
      outline.SetActive(false);
    }

    public void SetText(string value) {
      text.text = value;
    }

    public void ClearText() {
      text.text = string.Empty;
    }

    public void SetBackground(Sprite sprite) {
      background.sprite = sprite;
      background.color = new Color(1, 1, 1, 1f);
    }

    public void SetBackgroundGhost(Sprite sprite) {
      background.sprite = sprite;
      background.color = new Color(1, 1, 1, 0.3f);
    }

    public void ClearBackground() {
      background.sprite = null;
      background.color = new Color(1, 1, 1, 0);
    }

    public void SetTypeIcon(Sprite sprite) {
      typeIcon.sprite = sprite;
      typeIcon.color = new Color(1, 1, 1, 1f);
    }

    public void ClearTypeIcon() {
      typeIcon.sprite = null;
      typeIcon.color = new Color(1, 1, 1, 0);
    }

    public void Clear() {
      ClearText();
      ClearBackground();
      ClearTypeIcon();
    }

    public void Disable(Color disabledSlotColor) {
      Clear();
      background.color = disabledSlotColor;
    }

    public void Disable() {
      Clear();
      gameObject.SetActive(false);
    }
  }
}