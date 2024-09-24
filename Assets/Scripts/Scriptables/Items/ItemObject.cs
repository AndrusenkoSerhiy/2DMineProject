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

    // public List<string> boneNames = new List<string>();

    public Item CreateItem() {
      Item newItem = new Item(this);
      return newItem;
    }

    // private void OnValidate() {
    //   boneNames.Clear();
    //   if (CharacterDisplay == null) {
    //     return;
    //   }
    //   if (!CharacterDisplay.GetComponent<SkinnedMeshRenderer>()) {
    //     return;
    //   }

    //   var renderer = CharacterDisplay.GetComponent<SkinnedMeshRenderer>();
    //   var bones = renderer.bones;

    //   foreach (var t in bones) {
    //     boneNames.Add(t.name);
    //   }
    // }
  }
}