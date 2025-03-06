using System.Collections.Generic;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Scriptables {
  [CreateAssetMenu(menuName = "Create ResourceData", fileName = "ResourceData", order = 0)]
  public class ResourceData : ScriptableObject {
    public int Durability;
    [SerializeField] private List<TileData> tileDatas;
    [SerializeField] private int sortingOrder;
    [SerializeField] private ItemObject itemData;
    [SerializeField] private Color mapColor;
    [SerializeField] private Vector2 colliderOffset = Vector2.zero;
    [SerializeField] private Vector2 colliderSize = new Vector2(3.44f, 3.44f);
    [SerializeField] private bool isBuilding;

    public Sprite Sprite(int index) => tileDatas[index].Sprite;
    public int SortingOrder(int index) => sortingOrder + tileDatas[index].OffsetSorting;
    public int SortingOrder() => sortingOrder;

    public Vector3 ColSize() => colliderSize;
    public Vector3 ColOffset() => colliderOffset;

    public ItemObject ItemData => itemData;
    public Color Color => mapColor;
    public bool IsBuilding => isBuilding;
  }
}