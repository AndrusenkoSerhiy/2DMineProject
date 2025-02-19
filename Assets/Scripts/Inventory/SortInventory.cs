using Scriptables.Inventory;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace Inventory {
  public class SortInventory : MonoBehaviour {
    [SerializeField] private InventoryObject inventory;
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private Sprite ascImg;
    [SerializeField] private Sprite descImg;
    [SerializeField] private bool defaultSortAsc;

    private bool ascending;

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