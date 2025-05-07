using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Siege {
  [Serializable]
  public class ActiveSiegeTemplate {
    [SerializeField] private float duration;
    [SerializeField] private int zombieCount;
    [SerializeField] private int wavesOfZombies;
    [SerializeField] private float timeBeforeSiege;

    public float Duration => duration;
    public int ZombieCount => zombieCount;
    public int WavesOfZombies => wavesOfZombies;
    public float TimeBeforeSiege => timeBeforeSiege;

    public ActiveSiegeTemplate(SiegeTemplate baseTemplate) {
      duration = Random.Range(baseTemplate.Duration.x, baseTemplate.Duration.y + 1);
      zombieCount = Random.Range((int)baseTemplate.ZombieCount.x, (int)baseTemplate.ZombieCount.y + 1);
      wavesOfZombies = Random.Range((int)baseTemplate.WavesOfZombies.x, (int)baseTemplate.WavesOfZombies.y + 1);
      timeBeforeSiege = Random.Range(baseTemplate.TimeBeforeSiege.x, baseTemplate.TimeBeforeSiege.y + 1);
    }

    public ActiveSiegeTemplate(SiegeTemplate baseTemplate, float weight, int circle) {
      var weight01 = Mathf.Clamp01(weight / 100f);

      duration = LerpByWeight(baseTemplate.Duration.x, baseTemplate.Duration.y, weight01, circle);
      zombieCount =
        Mathf.RoundToInt(LerpByWeight(baseTemplate.ZombieCount.x, baseTemplate.ZombieCount.y, weight01, circle));
      wavesOfZombies = Mathf.RoundToInt(LerpByWeight(baseTemplate.WavesOfZombies.x, baseTemplate.WavesOfZombies.y,
        weight01, circle));
      timeBeforeSiege = LerpByWeight(baseTemplate.TimeBeforeSiege.x, baseTemplate.TimeBeforeSiege.y, 1f - weight01,
        circle); // чим складніше — тим менше часу
    }

    private float LerpByWeight(float min, float max, float weight01, int circle) {
      // Збільшуємо прогрес залежно від кола, щоб осади з кожним циклом ставали складнішими
      var scaledWeight = Mathf.Clamp01(weight01 + 0.05f * (circle - 1));
      var value = Mathf.Lerp(min, max, scaledWeight);
      return Random.Range(value * 0.9f, value * 1.1f); // Невеликий розкид
    }
  }
}