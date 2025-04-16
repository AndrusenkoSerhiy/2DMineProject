using Actors;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Actions.Movement {
  [Category("Project12/Movement")]
  public class MoveToPlayer : ActionTask<ActorEnemy> {
    public BBParameter<ActorBase> target;
    protected override void OnExecute() {
      agent.SetTargetTransform(target.value.transform);
    }
    
    protected override void OnUpdate() {
      if(agent.HasArrived()) EndAction(true);
      if(agent.IsDead) EndAction(false);
    }

    protected override void OnStop() {
      agent.SetTargetTransform(null);
    }
  }
}