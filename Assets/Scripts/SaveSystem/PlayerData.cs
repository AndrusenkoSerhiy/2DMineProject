using System;
using System.Collections.Generic;
using Stats;
using UnityEngine;

namespace SaveSystem {
  [Serializable]
  public class PlayerData {
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public bool IsSet;
    public bool IsDead;
    public PlayerStatsData PlayerStatsData;
  }

  [Serializable]
  public class PlayerStatsData {
    public float Health;
    public float Stamina;
    public List<StatModifierData> StatModifiersData;
  }

  [Serializable]
  public class StatModifierData {
    public StatType Type;
    public ApplyType ApplyType;
    public OperatorType OperatorType;
    public float Value;
    public float Duration;
    public float TimeLeft;
    public string ItemId;
    public string ModifierDisplayObjectId;
  }
}