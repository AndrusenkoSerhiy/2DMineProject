using System;

namespace Craft {
  [Serializable]
  public struct CraftingTask {
    public string ItemId;
    public long FinishTimeMilliseconds;

    public CraftingTask(string id, long endTimeMillisecondsInMilliseconds) {
      ItemId = id;
      FinishTimeMilliseconds = endTimeMillisecondsInMilliseconds;
    }
  }
}