using System;
using System.Collections.Generic;
using Scriptables.Items;

namespace Farm {
  [Serializable]
  public class ProcessingPlantBox {
    public string Coord;
    public bool HasGround;
    public bool HasSeeds;
    public bool StartGrowing;
    public bool HasRipened;

    public Seeds CurrSeed;
    public List<SeedHarvest> CurrHarvest = new List<SeedHarvest>();
    public int TimeToGrowth;
    public float CurrTime;
    public double LastUpdateTime;
  }
}