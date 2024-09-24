using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.Items {
  public abstract class ItemObject : ScriptableObject {
    public ItemType Type;
    public Sprite UiDisplay;
    public GameObject CharacterDisplay;
    public bool Stackable;
    public int MaxStackSize = 10;
    [TextArea(15, 20)] public string Description;
    public Item data = new Item();

    public Item CreateItem() {
      Item newItem = new Item(this);
      return newItem;
    }
  }
}