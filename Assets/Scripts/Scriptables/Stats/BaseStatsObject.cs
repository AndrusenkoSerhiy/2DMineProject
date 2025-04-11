using UnityEngine;

namespace Scriptables.Stats {
  [CreateAssetMenu(fileName = "BaseStats", menuName = "Stats/BaseStats")]
  public class BaseStatsObject : ScriptableObject {
    [Header("Health")] 
    public float health = 100f;
    public float maxHealth = 100f;
    public float healthRegen = 0.5f;
    public float healthMaxPossibleValue = 200f;

    [Header("Armor")] 
    public float armor = 1f;
    public float armorMaxPossibleValue = 20f;
  }
}