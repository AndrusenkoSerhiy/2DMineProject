using System;
using Inventory;
using QuickSlots;
using UnityEngine;

namespace Scriptables.Items {
  [Serializable]
  public abstract class ItemObject : ScriptableObject, IUsableItem {
    [SerializeField, HideInInspector] private string id;
    public string Id => id;
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

    [TextArea(15, 20)] public string Description;

    private void OnValidate() {
      if (string.IsNullOrEmpty(id)) {
        id = Guid.NewGuid().ToString();
      }
    }

    public virtual void Use(InventorySlot slot) {
    }
  }
}