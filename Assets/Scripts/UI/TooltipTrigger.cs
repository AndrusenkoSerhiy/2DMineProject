using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
  public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public string content;
    public string header;

    public void OnPointerEnter(PointerEventData eventData) {
      if (string.IsNullOrEmpty(content)) {
        return;
      }

      GameManager.Instance.TooltipManager.Show(content, header);
    }

    public void OnPointerExit(PointerEventData eventData) {
      GameManager.Instance.TooltipManager.Hide();
    }
  }
}