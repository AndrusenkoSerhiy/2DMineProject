using System.Collections.Generic;
using Scriptables.Items;
using UnityEngine;

namespace Scriptables{
  [CreateAssetMenu(menuName = "Create ResourceData", fileName = "ResourceData", order = 0)]
  public class ResourceData : ScriptableObject {
    public int Durability;
    [SerializeField] private List<Sprite> SpriteVariants = new();
    [SerializeField] private Sprite darkSprite;
    [SerializeField] private ItemObject itemData;

    public Sprite Sprite => SpriteVariants[Random.Range(0, SpriteVariants.Count)];
    public Sprite DarkSprite => darkSprite;

    public ItemObject ItemData => itemData;
  }
}