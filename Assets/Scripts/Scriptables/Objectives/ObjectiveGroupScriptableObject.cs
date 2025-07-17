using System.Collections.Generic;
using Objectives;
using UnityEngine;

namespace Scriptables.Objectives {
  [CreateAssetMenu(menuName = "Objectives/Group")]
  public class ObjectiveGroupScriptableObject : BaseScriptableObject {
    public string groupTitle;
    public List<ObjectiveData> objectives = new();
    public ObjectiveGroupScriptableObject nextGroup;
  }
}