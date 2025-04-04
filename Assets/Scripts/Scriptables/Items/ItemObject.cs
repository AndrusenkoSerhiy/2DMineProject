using System;
using System.Collections.Generic;
using UnityEngine;
using StatModifier = Scriptables.Stats.StatModifier;

namespace Scriptables.Items {
  [Serializable]
  public abstract class ItemObject : BaseScriptableObject {
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

    public List<StatModifier> statModifiers;
  }
}