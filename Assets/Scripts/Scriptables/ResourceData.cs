using System.Collections.Generic;
using UnityEngine;

namespace Scriptables{
  [CreateAssetMenu(menuName = "Create ResourceData", fileName = "ResourceData", order = 0)]
  public class ResourceData : ScriptableObject {
    public int Durability;
    [SerializeField]
    private List<Sprite> SpriteVariants = new();

    public Sprite Sprite => SpriteVariants[Random.Range(0, SpriteVariants.Count)];
  }
}