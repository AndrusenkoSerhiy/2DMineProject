using UnityEngine;

namespace Tools {
  public class ToolBase : MonoBehaviour {
    public virtual void StartUse(){}
    public virtual void EndUse(){}
    public virtual void Activate(){}
  }
}