using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(menuName = "Inventory System/Items/Item", fileName = "New Item")]
  public class Resource : ItemObject {
    public GameObject spawnPrefab;
    [SerializeField] private List<BonusResource> bonusResources = new List<BonusResource>();

    public List<BonusResource> GetBonusResources => bonusResources;

    [Serializable]
    public struct BonusResource {
      public ItemObject item;
      [Range(0, 1)] public float chance;
      public Vector2 rndCount;
    }

    public void Awake() {
      Type = ItemType.Tool;
    }
  }
}