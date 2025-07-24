using System;
using Scriptables.Items;

namespace Objectives.Data {
  [Serializable]
  public class BuildTaskData : ObjectiveTaskData {
    public BuildingBlock buildingBlock;
    public int amount;

    public override ObjectiveTaskType TaskType => ObjectiveTaskType.Build;
  }
}