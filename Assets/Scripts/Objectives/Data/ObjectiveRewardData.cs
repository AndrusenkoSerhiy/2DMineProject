using System;
using Scriptables.Items;

namespace Objectives.Data {
  [Serializable]
  public class ObjectiveRewardData {
    public ItemObject item;
    public int amount;
  }
}