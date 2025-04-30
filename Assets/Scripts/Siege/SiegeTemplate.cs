using System;
using UnityEngine;

namespace Siege {
  [Serializable]
  public class SiegeTemplate {
    [Tooltip("Range in seconds")] 
    public Vector2 Duration;
    public Vector2 ZombieCount;
    [Tooltip("Range in seconds")] 
    public Vector2 WavesOfZombies;
    [Tooltip("Range in seconds")] 
    public Vector2 TimeBeforeSiege;
  }
}