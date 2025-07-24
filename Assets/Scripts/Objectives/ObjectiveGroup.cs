using System;
using System.Collections.Generic;
using Objectives.Data;

namespace Objectives {
  [Serializable]
  public class ObjectiveGroup {
    public string id;
    public string groupTitle;
    public List<ObjectiveData> objectives = new();
    public ObjectiveRewardData reward;
  }
}