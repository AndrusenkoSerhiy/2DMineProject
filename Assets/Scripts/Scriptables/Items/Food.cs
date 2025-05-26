using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(menuName = "Inventory System/Items/Food", fileName = "Food")]
  public class Food : ItemObject, IConsumableItem {
    public AudioData consumeSound;
    public AudioData ConsumeSound => consumeSound;
  }
}