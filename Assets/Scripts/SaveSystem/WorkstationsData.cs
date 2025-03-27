using System;
using System.Collections.Generic;
using Craft;
using UnityEngine;

namespace SaveSystem {
  [Serializable]
  public class WorkstationsData {
    [field: SerializeField] public string Id { get; set; }
    public List<CraftInputData> Inputs;
    public CurrentProgress CurrentProgress;
    public string WorkStationObjectId;
  }
}