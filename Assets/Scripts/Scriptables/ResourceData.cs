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


    public Sprite Sprite(int index) => tileDatas[index].Sprite;
    public int SortingOrder(int index) => sortingOrder + tileDatas[index].OffsetSorting;

    public Vector3 PosOffset(int index) => tileDatas[index].OffsetPosition;
    public Vector3 ColOffset(int index) => tileDatas[index].OffsetCollider;

    public ItemObject ItemData => itemData;
    public Color Color => mapColor;
  }
}