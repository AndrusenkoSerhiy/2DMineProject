using System.Collections.Generic;
using Scriptables.Items;
using UnityEngine;

namespace Scriptables{
  [CreateAssetMenu(menuName = "Create ResourceData", fileName = "ResourceData", order = 0)]
  public class ResourceData : ScriptableObject {
    public int Durability;
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private int sortingOrder;
    [SerializeField] private ItemObject itemData;
    [SerializeField] private Color mapColor;
    
    
    public Sprite Sprite(int index) => sprites[index];
    public int SortingOrder => sortingOrder;
    public ItemObject ItemData => itemData;
    public Color Color => mapColor;
  }
}