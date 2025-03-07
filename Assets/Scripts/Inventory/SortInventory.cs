using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  public class SortInventory : MonoBehaviour {
    [SerializeField] private UserInterface inventoryUI;
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private Sprite ascImg;
    [SerializeField] private Sprite descImg;
    [SerializeField] private bool defaultSortAsc;

    private bool ascending;
    private InventoryObject inventory;

    private void Start() {
      inventory = inventoryUI.Inventory;
    }

    private void OnEnable() {
      ascending = defaultSortAsc;
      UpdateImage();
      button.onClick.AddListener(Run);
    }

    private void OnDisable() {
      button.onClick.RemoveAllListeners();
    }

    private void Run() {
      inventory.SortInventory(ascending);
      ascending = !ascending;
      UpdateImage();
    }

    private void UpdateImage() {
      image.sprite = ascending ? ascImg : descImg;
    }
  }
}