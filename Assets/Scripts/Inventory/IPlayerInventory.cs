using Scriptables.Items;
using UnityEngine;

namespace Inventory {
  public interface IPlayerInventory {
    void AddItemToInventory(ItemObject item, int count, Vector3 tr);
  }
}