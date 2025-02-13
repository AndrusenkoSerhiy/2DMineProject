using System.Collections.Generic;
using Scriptables.Inventory;
using Scriptables.Items;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scriptables.Craft {
  [CreateAssetMenu(menuName = "Crafting System/Workstation", fileName = "New Workstation")]
  public class Workstation : ScriptableObject {
    public RecipeType RecipeType;
    public string Title;
    [TextArea(15, 20)] public string Description;
    public int OutputSlotsAmount;
    public List<Recipe> recipes = new List<Recipe>();
    public InventoryObject OutputInventory;

    public SerializedDictionary<ItemObject, int> CraftItemsTotal = new();
    public List<int> CraftInputsItemsIds = new();

    public void AddItemToCraftTotal(ItemObject item, int count) {
      if (!CraftItemsTotal.ContainsKey(item)) {
        CraftItemsTotal.Add(item, count);
      }
      else {
        CraftItemsTotal[item] += count;
      }
    }

    public void RemoveCountFromCraftTotal(ItemObject item, int count) {
      if (!CraftItemsTotal.ContainsKey(item)) {
        return;
      }

      var craftItemCount = CraftItemsTotal[item];
      craftItemCount -= count;

      if (craftItemCount <= 0) {
        CraftItemsTotal.Remove(item);
      }
      else {
        CraftItemsTotal[item] = craftItemCount;
      }
    }

    public void AddToCraftInputsItemsIds(int id) {
      CraftInputsItemsIds.Add(id);
    }

    public void RemoveFromCraftInputsItemsIds(int position = 0) {
      if (CraftInputsItemsIds.Count > 0) {
        CraftInputsItemsIds.RemoveAt(position);
      }
    }
  }
}