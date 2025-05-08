using Actors;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Tools;

namespace NodeCanvas.Actions.Movement {
  [Category("Project12/Movement")]
  public class MoveToPlayer : ActionTask<ActorEnemy> {
    public BBParameter<ActorBase> target;
    protected override void OnExecute() {
      SetTarget();
      //agent.SetTargetTransform(target.value.transform);
      Subscribe();
    }
    
    protected override void OnUpdate() {
      if (agent.HasArrived()) {
        UnSubscribe();
        EndAction(true);
      }

      if (agent.IsDead) {
        UnSubscribe();
        EndAction(false);
      }
    }

    protected override void OnStop() {
      agent.SetTargetTransform(null, 0);
    }

    private void SetTarget() {
      var newTarget = GameManager.Instance.CurrPlayerController.Actor;
      agent.SetTargetTransform(newTarget.transform, newTarget.ActorBoundsWidth);
      target.value = newTarget;
    }

    private void Subscribe() {
      MiningRobotTool.OnPlayerSitOnRobot += SetTarget;
      MiningRobotTool.OnPlayerExitFromRobot += SetTarget;
    }
    
    private void UnSubscribe() {
      MiningRobotTool.OnPlayerSitOnRobot -= SetTarget;
      MiningRobotTool.OnPlayerExitFromRobot -= SetTarget;
    }
  }
}