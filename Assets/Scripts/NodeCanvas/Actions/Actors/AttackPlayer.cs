using Actors;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Actions.Actors {
  [Category("Project12/Actors")]
  public class AttackPlayer : ActionTask<ActorEnemy>{
    protected override void OnExecute() {
      agent.AttackPlayer();
      EndAction(true);
    }
  }
}