using System;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  public class SortInventory : MonoBehaviour {
    [SerializeField] private InventoryType inventoryType;
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private Sprite ascImg;
    [SerializeField] private Sprite descImg;
    [SerializeField] private bool defaultSortAsc;

    private InventoryObject inventory;
    private bool ascending;

    private void Awake() {
      inventory = GameManager.Instance.PlayerInventory.GetInventoryByType(inventoryType);
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