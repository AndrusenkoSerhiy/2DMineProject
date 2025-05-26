using Inventory;
using Scriptables.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class Ammo : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image image;
    [SerializeField] private Image reloadMask;

    private int total;
    private int magazineSize;
    private int currentAmount;
    private string ammoId;
    private PlayerInventory playerInventory;

    public void Show(ItemObject item, int amount, int magSize) {
      if (!item) {
        return;
      }

      ammoId = item.Id;
      magazineSize = magSize;
      total = GetPlayerInventory().InventoriesPool.GetResourceTotalAmount(ammoId);
      currentAmount = amount;

      UpdateText();
      UpdateReloadMask();
      image.sprite = item.UiDisplay;
      gameObject.SetActive(true);
      SubscribeToEvents();
    }

    public void UpdateCount(int amount, int? totalCount = null) {
      currentAmount = amount;
      total = totalCount ?? total;
      UpdateText();
      UpdateReloadMask();
    }

    public void Hide() {
      if (!gameObject.activeSelf) {
        return;
      }

      gameObject.SetActive(false);
      UnSubscribeToEvents();
    }

    private void UpdateText() {
      ammoText.text = $"{currentAmount}/{total}";
    }

    private void SubscribeToEvents() {
      foreach (var inventory in GetPlayerInventory().InventoriesPool.Inventories) {
        foreach (var slot in inventory.Slots) {
          slot.OnAfterUpdated += UpdateTotal;
        }
      }
    }

    private void UnSubscribeToEvents() {
      var pInventory = GetPlayerInventory();
      if (!pInventory || pInventory.InventoriesPool == null) {
        return;
      }

      foreach (var inventory in pInventory.InventoriesPool.Inventories) {
        foreach (var slot in inventory.Slots) {
          slot.OnAfterUpdated -= UpdateTotal;
        }
      }
    }

    private void UpdateTotal(SlotUpdateEventData slotUpdateEventData) {
      var slot = slotUpdateEventData.after;
      if (slot.isEmpty || slot.Item.id != ammoId) {
        return;
      }

      total = GetPlayerInventory().InventoriesPool.GetResourceTotalAmount(ammoId);
      UpdateText();
    }

    private PlayerInventory GetPlayerInventory() {
      if (!playerInventory) {
        playerInventory = GameManager.Instance.PlayerInventory;
      }

      return playerInventory;
    }
    
    private void UpdateReloadMask() {
      reloadMask.fillAmount = (float) currentAmount / magazineSize;
    }
  }
}