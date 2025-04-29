using System;

namespace Siege {
  [Serializable]
  public class SiegeTemplate {
    public float DurationMin;
    public float DurationMax;
    public int ZombieCountMin;
    public int ZombieCountMax;
    public float SpawnIntervalMin;
    public float SpawnIntervalMax;
    public float TimeBetweenSiegeMin;
    public float TimeBetweenSiegeMax;
  }
}