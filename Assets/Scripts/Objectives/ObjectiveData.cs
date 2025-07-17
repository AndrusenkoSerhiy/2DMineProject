using System;

namespace Objectives {
  [Serializable]
  public class ObjectiveData {
    public string id;
    public string title;
    public ObjectiveTaskType type;
    public string targetId;
    public int requiredCount;
    public ObjectiveRewardData reward;
  }
}