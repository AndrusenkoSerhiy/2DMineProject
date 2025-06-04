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
    private Inventory inventory;

    private void OnEnable() {
      inventory = inventoryUI.Inventory;
      ascending = defaultSortAsc;
      UpdateImage();
      button.onClick.AddListener(Run);
    }

    private void OnDisable() {
      button.onClick.RemoveAllListeners();
    }

    private void Run() {
      GameManager.Instance.AudioController.PlayUIClick();
      inventory.SortInventory(ascending);
      ascending = !ascending;
      UpdateImage();
    }

    private void UpdateImage() {
      image.sprite = ascending ? ascImg : descImg;
    }
  }
}