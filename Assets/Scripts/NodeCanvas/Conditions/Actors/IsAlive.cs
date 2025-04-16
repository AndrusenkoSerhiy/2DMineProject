using Actors;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Conditions.Actors {
  [Category("Project12/Actors")]
  
  public class IsAlive : ConditionTask {
    public BBParameter<ActorBase> actor;
    protected override bool OnCheck() {
      return actor.value != null && !actor.value.IsDead;
    }
  }
}