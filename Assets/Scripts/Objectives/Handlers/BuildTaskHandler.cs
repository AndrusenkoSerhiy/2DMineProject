using Objectives.Data;
using Scriptables.Items;

namespace Objectives.Handlers {
  public class BuildTaskHandler : IObjectiveTaskHandler {
    public ObjectiveTaskType Type => ObjectiveTaskType.Build;

    public bool IsTaskSatisfied(ObjectiveTaskData taskData, object context, int currentProgress,
      out int progressToAdd) {
      progressToAdd = 0;

      if (taskData is not BuildTaskData build || context is not (BuildingBlock block, int amount)) {
        return false;
      }

      if (build.buildingBlock != block) {
        return false;
      }

      progressToAdd = amount;
      return currentProgress + amount >= build.amount;
    }
  }
}