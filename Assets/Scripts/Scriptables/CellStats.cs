using UnityEngine;

namespace World {
  [CreateAssetMenu]
  public class CellStats : ScriptableObject {
    [Header("ATTACK SHAKE")]
    public float minShakeDuration = 0.2f;
    public float maxShakeDuration = 0.5f;
    public float minShakeIntensity = 0.05f;
    public float maxShakeIntensity = 0.2f;
    public int vibrato = 10;
    public float randomness = 90;
  }
}