using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  public class TakeAll : MonoBehaviour {
    [SerializeField] private UserInterface fromInventoryUI;
    [SerializeField] private UserInterface toInventoryUI;
    [SerializeField] private Button button;

    private InventoryObject fromInventory;
    private InventoryObject toInventory;

    private void Start() {
      fromInventory = fromInventoryUI.Inventory;
      toInventory = toInventoryUI.Inventory;
    }

    private void OnEnable() {
      button.onClick.AddListener(Run);
    }

    private void OnDisable() {
      button.onClick.RemoveAllListeners();
    }

    private void Run() {
      fromInventory.MoveAllItemsTo(toInventory);
    }
  }
}