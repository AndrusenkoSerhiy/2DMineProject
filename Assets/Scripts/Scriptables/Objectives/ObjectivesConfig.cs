using System.Collections.Generic;
using Objectives;
using UnityEngine;

namespace Scriptables.Objectives {
  [CreateAssetMenu(menuName = "Objectives/Objectives Config")]
  public class ObjectivesConfig : ScriptableObject {
    public string id;
    public Sprite titleIcon;
    public Color titleColor;

    public Sprite taskIconIncomplete;
    public Sprite taskIconCompleted;
    public Color taskColorIncomplete;
    public Color taskColorCompleted;

    public List<ObjectiveGroup> groups = new();
  }
}