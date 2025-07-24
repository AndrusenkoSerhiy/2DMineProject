using Objectives.Data;

namespace Objectives.Handlers {
  public class SurviveSiegeTaskHandler : IObjectiveTaskHandler {
    public ObjectiveTaskType Type => ObjectiveTaskType.SurviveSiege;

    public bool IsTaskSatisfied(ObjectiveTaskData taskData, object context, int currentProgress,
      out int progressToAdd) {
      progressToAdd = 0;

      return taskData is SurviveSiegeTaskData;
    }
  }
}