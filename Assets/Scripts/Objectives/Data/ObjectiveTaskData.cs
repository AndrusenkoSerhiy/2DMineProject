using System;

namespace Objectives.Data {
  [Serializable]
  public abstract class ObjectiveTaskData {
    public abstract ObjectiveTaskType TaskType { get; }
  }
}