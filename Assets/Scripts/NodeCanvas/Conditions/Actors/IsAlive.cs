using Actors;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Conditions.Actors {
  [Category("Project12/Actors")]
  
  public class IsAlive : ConditionTask<ActorBase> {
    public BBParameter<bool> isSelf;
    public BBParameter<ActorBase> actor;
    protected override bool OnCheck() {
      if (isSelf.value) {
        return !agent.IsDead;
      }
      
      return actor.value != null && !actor.value.IsDead;
    }
  }
}