using System;

namespace Objectives.Data {
  [Serializable]
  public class RobotRepairTaskData : ObjectiveTaskData {
    public override ObjectiveTaskType TaskType => ObjectiveTaskType.RobotRepair;
  }
}