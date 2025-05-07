using System;
using UnityEngine;

namespace SaveSystem {
  [Serializable]
  public class RobotData {
    public string Id;
    public bool IsSet;
    public bool IsPlayerInside;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public PlayerStatsData PlayerStatsData;
  }
}