using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.Siege {
  [CreateAssetMenu(fileName = "ZombieDifficultyProfile", menuName = "Siege/Zombie Difficulty Profile", order = 0)]
  public class ZombieDifficultyProfile : BaseScriptableObject {
    public float healthMultiplier = 1f;
    public float speedMultiplier = 1f;
    public float attackMultiplier = 1f;
    public float armorMultiplier = 1f;
    
    public List<AudioData> OnTakeDamageAudioDatas;
    public List<AudioData> OnDeathAudioDatas;
    
    public AudioData GroanAudioData;
    public Vector2 GroanInterval;
  }
}