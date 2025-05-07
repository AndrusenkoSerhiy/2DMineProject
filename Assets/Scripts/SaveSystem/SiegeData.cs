using System;
using System.Collections.Generic;
using Siege;

namespace SaveSystem {
  [Serializable]
  public class SiegeData {
    public bool IsSet;
    public int CurrentSiegeCycle;
    public int CurrentSiegeIndex;
    public float DurationTimer;
    public float SiegeCycleElapsedTime;
    public bool SiegesStarted;
    public bool IsPaused;
    public float TotalCycleTime;
    public bool IsSiegeInProgress;
    public float TimeToNextSegment;
    public List<ActiveSiegeTemplate> SiegeQueue;
  }
}