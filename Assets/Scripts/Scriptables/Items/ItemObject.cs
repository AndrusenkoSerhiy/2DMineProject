using System;
using Inventory;
using QuickSlots;
using UnityEngine;

namespace Scriptables.Items {
  [Serializable]
  public abstract class ItemObject : BaseScriptableObject, IUsableItem {
    public ItemType Type;
    public string Name;
    public ParentType ParentType;
    public Sprite UiDisplay;
    public GameObject CharacterDisplay;
    public bool Stackable;
    public int MaxStackSize = 10;
    public Vector3 SpawnPosition;
    public Vector3 SpawnRotation;
    public GameObject spawnPrefab;
    public bool CanMoveToAnotherInventory = true;
    public bool CanDrop = true;

    [TextArea(15, 20)] public string Description;

    public virtual void Use(InventorySlot slot) {
    }
  }
}