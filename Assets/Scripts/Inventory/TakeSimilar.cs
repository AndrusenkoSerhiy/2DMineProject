using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  public class TakeSimilar : MonoBehaviour {
    [SerializeField] private UserInterface fromInventoryUI;
    [SerializeField] private UserInterface toInventoryUI;
    [SerializeField] private Button button;

    private Inventory fromInventory;
    private Inventory toInventory;

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
      fromInventory.TakeSimilar(toInventory);
    }
  }
}