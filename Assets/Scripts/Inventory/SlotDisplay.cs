using System.Collections.Generic;
using Scriptables.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  public class SlotDisplay : MonoBehaviour {
    [SerializeField] private GameObject outline;
    [SerializeField] private Image background;
    [SerializeField] private Image typeIcon;
    [SerializeField] private TextMeshProUGUI text;
    public Sprite EmptySlotIcon;
    public List<ItemType> AllowedTypes;
    public List<ItemObject> AllowedItems;
    public int MaxAllowedAmount = -1;
    public Item currentItem;
    public InventoryType type;

    public TextMeshProUGUI Text => text;
    public Image Background => background;
    public Image TypeIcon => typeIcon;

    public virtual void UpdateUI(InventorySlot slot) {
      currentItem = slot.Item;
      type = slot.InventoryType;

      if (!currentItem.info || slot.amount <= 0) {
        ClearText();
        ClearBackground();
      }
      else {
        var newText = slot.amount == 1 ? string.Empty : slot.amount.ToString("n0");
        SetBackground(currentItem.info.UiDisplay);
        SetText(newText);
      }
    }

    public bool IsAllowedItem(ItemObject item) {
      if (item == null) {
        return true;
      }

      if (AllowedTypes is { Count: > 0 } && !AllowedTypes.Contains(item.Type)) {
        return false;
      }

      if (AllowedItems == null || AllowedItems.Count == 0) {
        return true;
      }

      foreach (var allowedItem in AllowedItems) {
        if (allowedItem == item) {
          return true;
        }
      }

      return false;
    }

    public bool IsItemInAllowed(ItemObject item) {
      if (AllowedItems == null || AllowedItems.Count == 0) {
        return false;
      }

      return AllowedItems.Contains(item);
    }

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
      if (EmptySlotIcon != null) {
        SetBackgroundGhost(EmptySlotIcon);
        return;
      }

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