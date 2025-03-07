using Inventory;
using UnityEngine;

namespace Windows {
  public class StorageWindow : WindowBase {
    [SerializeField] private UserInterface storageUI;
    [SerializeField] private UserInterface inventoryUI;
    public UserInterface StorageUI => storageUI;
    public UserInterface InventoryUI => inventoryUI;
  }
}