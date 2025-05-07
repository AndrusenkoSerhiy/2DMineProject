using System;
using UnityEngine;

namespace SaveSystem {
  [Serializable]
  public class ZombiesData {
    public string ProfileId;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public PlayerStatsData PlayerStatsData;
  }
}