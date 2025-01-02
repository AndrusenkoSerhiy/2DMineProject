using Scriptables.Inventory;
using Scriptables.Items;
using Settings;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
  public InventoryObject inventory;
  public GameObject inventoryPrefab;
  // public GameObject equipmentPrefab;
  public InventoryObject equipment;
  private int defaultItemId = 0;

  private void Start() {
    inventory.Load();
    equipment.Load();

    // TODO Add default item to inventory if it's not already there
    Item defaultItem = new Item(inventory.database.ItemObjects[defaultItemId]);
    if (!inventory.IsItemInInventory(inventory.database.ItemObjects[defaultItemId])
        && !equipment.IsItemInInventory(inventory.database.ItemObjects[defaultItemId])) {
      Debug.Log("Adding default item to inventory.");
      inventory.AddItem(defaultItem, 1, null, null);
    }

    //hide the inventory UI at the start
    if (inventoryPrefab != null) {
      inventoryPrefab.SetActive(false);
    }
    GameManager.instance.PlayerInventory = this;
  }

  // public void OnTriggerEnter2D(Collider2D other) {
  //   var item = other.GetComponent<GroundItem>();
  //   // Debug.Log("Picked up " + item);
  //   if (item && !item.IsPicked) {
  //     if (inventory.AddItem(new Item(item.item), item.Count, null, item)) {
  //       item.IsPicked = true;
  //       //Debug.LogError($"Destroy {other.name}");
  //       Destroy(other.gameObject);
  //     }
  //   }
  // }

  public void AddItemToInventory(ItemObject item, int count){
    inventory.AddItem(new Item(item), count, item, null);
    AddAdditionalItem(item);
  }
  
  //get bonus resource when we are mining
  private void AddAdditionalItem(ItemObject item) {
    var resource = item as Resource;
    if (resource == null) 
      return;
    
    var list = resource.GetBonusResources;
    for (int i = 0; i < list.Count; i++) {
      if (Random.value > list[i].chance)
        return;
      
      var count = Random.Range((int)list[i].rndCount.x, (int)list[i].rndCount.y);
      //Debug.LogError($"spawn {list[i].item.name} | count {count} ");
      inventory.AddItem(new Item(list[i].item), count, list[i].item, null); 
    }
  }

  private void Update() {
    //show/hide inventory
    if (UserInput.instance.controls.UI.Inventory.triggered && inventoryPrefab != null) {
      UserInput.instance.EnableUIControls(!inventoryPrefab.activeSelf);
      inventoryPrefab.SetActive(!inventoryPrefab.activeSelf);
    }
  }

  public void OnApplicationQuit() {
    inventory.Save();
    equipment.Save();
    inventory.Clear();
    equipment.Clear();
  }
}