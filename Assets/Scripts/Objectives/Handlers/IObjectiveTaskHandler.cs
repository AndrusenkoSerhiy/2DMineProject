using Objectives.Data;

namespace Objectives.Handlers {
  public interface IObjectiveTaskHandler {
    ObjectiveTaskType Type { get; }
    bool IsTaskSatisfied(ObjectiveTaskData taskData, object context, int currentProgress, out int progressToAdd);
  }
}