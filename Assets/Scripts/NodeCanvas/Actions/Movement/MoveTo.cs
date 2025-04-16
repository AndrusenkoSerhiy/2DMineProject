using Actors;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Utils;
using World;

namespace NodeCanvas.Actions.Movement {
  [Category("Project12/Movement")]
  public class MoveTo : ActionTask<ActorEnemy> {
    public BBParameter<Coords> target;
    protected override void OnExecute() {
      agent.SetPatrolPosition(CoordsTransformer.GridToWorld(target.value.X, target.value.Y));
    }

    protected override void OnUpdate() {
      if (agent.HasArrived()) {
        EndAction(true);
      }

      if (agent.IsDead) {
        EndAction(false);
      }
    }

    protected override void OnStop() {
      agent.SetPatrolPosition(Vector3.zero);
    }
  }
}