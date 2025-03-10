using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem {
  [Serializable]
  public class WorkstationsData : ISaveable {
    [field: SerializeField] public string Id { get; set; }
    public List<CraftInputData> Inputs;
    public long MillisecondsLeft;
    public string ResourcePath;
    public string WorkStationObjectId;
  }
}