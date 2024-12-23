using System.Collections.Generic;
using Scriptables.Items;
using UnityEngine;

namespace Scriptables{
  [CreateAssetMenu(menuName = "Create ResourceData", fileName = "ResourceData", order = 0)]
  public class ResourceData : ScriptableObject {
    public int Durability;
    [SerializeField] private Sprite sprite;
    [SerializeField] private ItemObject itemData;
    [SerializeField] private Color mapColor;

    public Sprite Sprite => sprite;
    public ItemObject ItemData => itemData;
    public Color Color => mapColor;
  }
}