using System;
using System.Collections.Generic;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Scriptables {
  [CreateAssetMenu(menuName = "Create ResourceData", fileName = "ResourceData", order = 0)]
  public class ResourceData : ScriptableObject {
    public int Durability;
    public int DropCount;
    [SerializeField] private List<TileData> tileDatas;
    [SerializeField] private int sortingOrder;
    [SerializeField] private ItemObject itemData;
    [SerializeField] private Color mapColor;
    [SerializeField] private Color effectColor;
    [SerializeField] private Color previewColor;
    [SerializeField] private Color blockColor;
    [SerializeField] private Vector2 colliderOffset = Vector2.zero;
    [SerializeField] private Vector2 colliderSize = new Vector2(3.44f, 3.44f);
    //[SerializeField] private bool isBuilding;
    [SerializeField] private Vector2 cellSize = Vector3.one;
    
    [SerializeField] private List<BonusResource> bonusResources = new List<BonusResource>();

    public List<BonusResource> GetBonusResources => bonusResources;
    public AudioData OnTakeDamageAudioData;

    [Serializable]
    public struct BonusResource {
      public ItemObject item;
      [Range(0, 1)] public float chance;
      public Vector2 rndCount;
    }

    public Sprite Sprite(int index) => tileDatas[index].Sprite;
    public int SortingOrder(int index) => sortingOrder + tileDatas[index].OffsetSorting;
    public int SortingOrder() => sortingOrder;

    public Vector3 ColSize() => colliderSize;
    public Vector3 ColOffset() => colliderOffset;

    public ItemObject ItemData => itemData;
    public Color Color => mapColor;
    public Color EffectColor => effectColor;
    public Color PreviewColor => previewColor;
    public Color BlockColor => blockColor;

    //public bool IsBuilding => isBuilding;
    public Vector2 CellSize => cellSize;
    public bool debug;
    public bool CanTakeDamage;
  }
}