using UnityEngine;

namespace Siege {
  public class ActiveSiegeTemplate {
    private float duration;
    private int zombieCount;
    private float spawnInterval;
    private float timeBetweenSiege;

    public float Duration => duration;
    public int ZombieCount => zombieCount;
    public float SpawnInterval => spawnInterval;
    public float TimeBetweenSiege => timeBetweenSiege;

    //TODO calculate with weight and previous sieges circles
    public ActiveSiegeTemplate(SiegeTemplate baseTemplate) {
      duration = Random.Range(baseTemplate.DurationMin, baseTemplate.DurationMax + 1);
      zombieCount = Random.Range(baseTemplate.ZombieCountMin, baseTemplate.ZombieCountMax + 1);
      spawnInterval = Random.Range(baseTemplate.SpawnIntervalMin, baseTemplate.SpawnIntervalMax + 1);
      timeBetweenSiege = Random.Range(baseTemplate.TimeBetweenSiegeMin, baseTemplate.TimeBetweenSiegeMax + 1);
    }
  }
}