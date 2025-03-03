using System;

namespace Craft {
  [Serializable]
  public struct CraftingTask {
    public string ItemId;
    public string FinishTimeString;

    public DateTime FinishTime {
      get => Helper.ParseDate(FinishTimeString);
      set => FinishTimeString = Helper.FormatDate(value);
    }

    public CraftingTask(string id, DateTime endTime) {
      ItemId = id;
      FinishTimeString = Helper.FormatDate(endTime);
    }
  }
}