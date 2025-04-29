using UnityEngine;

namespace Scriptables.Siege {
  [CreateAssetMenu(fileName = "ZombieDifficultyProfile", menuName = "Siege/Zombie Difficulty Profile", order = 0)]
  public class ZombieDifficultyProfile : ScriptableObject {
    public float healthMultiplier = 1f;
    public float speedMultiplier = 1f;
    public float attackMultiplier = 1f;
    public float armorMultiplier = 1f;
  }
}