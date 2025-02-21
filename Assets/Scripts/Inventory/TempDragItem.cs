using Scriptables.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  public class TempDragItem : MonoBehaviour {
    [SerializeField] private Image image;
    [SerializeField] private RectTransform rectTransform;
    private bool isDrag;
    private bool dragFull;
    private int amount;

    public bool IsDrag => isDrag;
    public bool DragFull => dragFull;
    public int Amount => amount;

    public void Enable(Item item, int amount, Transform tempDragParent) {
      dragFull = true;
      this.amount = amount;
      image.sprite = item.info.UiDisplay;
      transform.SetParent(tempDragParent);
      isDrag = true;
      gameObject.SetActive(true);
    }

    public void SetAmount(int amount) {
      if (amount < this.amount) {
        dragFull = false;
      }

      this.amount = amount;
    }

    public void UpdatePosition(Vector3 position) {
      rectTransform.position = position;
    }

    public void Disable() {
      image.sprite = null;
      transform.SetParent(null);
      isDrag = false;
      gameObject.SetActive(false);
    }
  }
}