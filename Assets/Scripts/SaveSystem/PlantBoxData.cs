using System;
using Scriptables.Items;
using UnityEngine;

namespace SaveSystem {
  [Serializable]
  public class PlantBoxData {
    public Vector3 position;
    public bool HasGround;
    public bool HasSeeds;
    public bool StartGrowing;
    public bool HasRipened;

    public Seeds CurrSeed;
    public ItemObject CurrHarvest;
    public int TimeToGrowth;
    public float CurrTime;
  }
}