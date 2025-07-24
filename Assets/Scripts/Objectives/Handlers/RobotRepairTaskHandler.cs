using Objectives.Data;

namespace Objectives.Handlers {
  public class RobotRepairTaskHandler : IObjectiveTaskHandler {
    public ObjectiveTaskType Type => ObjectiveTaskType.RobotRepair;

    public bool IsTaskSatisfied(ObjectiveTaskData taskData, object context, int currentProgress,
      out int progressToAdd) {
      progressToAdd = 0;

      return taskData is RobotRepairTaskData;
    }
  }
}