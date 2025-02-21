using Windows;
using Scriptables.Inventory;
using Scriptables.Items;
using Settings;
using UnityEngine;
using Pool;

namespace Inventory {
  public class PlayerInventory : MonoBehaviour, IPlayerInventory {
    public InventoryObject inventory;
    public InventoryObject quickSlots;

    [SerializeField] private ItemObject defaultItem;

    // public Action OnQuickSlotLoaded;

    private PlayerInventoryWindow inventoryWindow;

    public void Start() {
      inventoryWindow = GameManager.instance.WindowsController.GetWindow<PlayerInventoryWindow>();
      /*inventory.Load();

      quickSlots.Load();
      OnQuickSlotLoaded?.Invoke();*/
    }

    public void Update() {
      if (UserInput.instance.controls.UI.Inventory.triggered) {
        // InitInventoryWindow();
        // UserInput.instance.EnableUIControls(!inventoryWindow.IsShow);
        if (inventoryWindow.IsShow) {
          inventoryWindow.Hide();
        }
        else {
          inventoryWindow.Show();
        }
      }
    }

    /*public void OnApplicationQuit() {
      inventory.Save();
      quickSlots.Save();

      inventory.Clear();
      quickSlots.Clear();
    }*/

    public void AddItemToInventory(ItemObject item, int count, Vector3 cellPos) {
      inventory.AddItem(new Item(item), count);
      ObjectPooler.Instance.SpawnFlyEffect(item, cellPos);
      AddAdditionalItem(item, cellPos);
    }

    //get bonus resource when we are mining
    private void AddAdditionalItem(ItemObject item, Vector3 cellPos) {
      var resource = item as Resource;
      if (resource == null)
        return;

      var list = resource.GetBonusResources;
      for (int i = 0; i < list.Count; i++) {
        if (UnityEngine.Random.value > list[i].chance)
          return;

        var count = UnityEngine.Random.Range((int)list[i].rndCount.x, (int)list[i].rndCount.y);
        //Debug.LogError($"spawn {list[i].item.name} | count {count} ");
        inventory.AddItem(new Item(list[i].item), count);
        ObjectPooler.Instance.SpawnFlyEffect(list[i].item, cellPos);
      }
    }
  }
}