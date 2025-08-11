using Objectives.Data;
using Scriptables.Items;

namespace Objectives.Handlers {
  public class CraftItemTaskHandler : IObjectiveTaskHandler {
    public ObjectiveTaskType Type => ObjectiveTaskType.CraftItem;

    public bool IsTaskSatisfied(ObjectiveTaskData taskData, object context, int currentProgress,
      out int progressToAdd) {
      progressToAdd = 0;

      if (taskData is not CraftItemTaskData data || context is not (ItemObject item, int amount)) {
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