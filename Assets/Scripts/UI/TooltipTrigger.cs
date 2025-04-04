using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
  public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public string content;
    public string header;

    private bool show;

    public void UpdateText() {
      if (!show) {
        return;
      }

      GameManager.Instance.TooltipManager.UpdateTooltip(content, header);
    }

    public void OnPointerEnter(PointerEventData eventData) {
      if (string.IsNullOrEmpty(content)) {
        return;
      }

      show = true;

      GameManager.Instance.TooltipManager.Show(content, header);
    }

    public void OnPointerExit(PointerEventData eventData) {
      GameManager.Instance.TooltipManager.Hide();
      show = false;
    }
  }
}