using Objectives.Data;
using Scriptables.Items;

namespace Objectives.Handlers {
  public class ItemUseTaskHandler : IObjectiveTaskHandler {
    public ObjectiveTaskType Type => ObjectiveTaskType.ItemUse;

    public bool IsTaskSatisfied(ObjectiveTaskData taskData, object context, int currentProgress,
      out int progressToAdd) {
      progressToAdd = 0;

      if (taskData is not ItemUseTaskData data || context is not (ItemObject item, int amount)) {
        return false;
      }

      if (data.itemObject != item) {
        return false;
      }

      progressToAdd = amount;
      return currentProgress + amount >= data.amount;
    }
  }
}