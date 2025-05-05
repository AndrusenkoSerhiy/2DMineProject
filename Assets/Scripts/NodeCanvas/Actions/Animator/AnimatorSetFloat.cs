using Actors;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Actions.Animator {
  [Category("Project12/Animator")]
  public class AnimatorSetFloat : ActionTask<ActorEnemy>{
    public BBParameter<float> param;
    protected override void OnExecute() {
      agent.SetAnimVelocityX(param.value);
      EndAction(true);
    }
  }
}