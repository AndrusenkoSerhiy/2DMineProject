using System;

namespace Objectives.Data {
  [Serializable]
  public class SurviveSiegeTaskData : ObjectiveTaskData {
    public override ObjectiveTaskType TaskType => ObjectiveTaskType.SurviveSiege;
  }
}