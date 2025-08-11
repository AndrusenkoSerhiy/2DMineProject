using System;
using Scriptables.Items;

namespace Objectives.Data {
  [Serializable]
  public class CraftItemTaskData : ObjectiveTaskData {
    public ItemObject itemObject;
    public int amount;

    public override ObjectiveTaskType TaskType => ObjectiveTaskType.CraftItem;
  }
}