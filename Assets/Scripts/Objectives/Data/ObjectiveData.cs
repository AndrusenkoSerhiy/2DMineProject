using System;
using UnityEngine;

namespace Objectives.Data {
  [Serializable]
  public class ObjectiveData {
    public string id;
    public string title;
    [SerializeReference] public ObjectiveTaskData taskData;
    public ObjectiveRewardData reward;
  }
}