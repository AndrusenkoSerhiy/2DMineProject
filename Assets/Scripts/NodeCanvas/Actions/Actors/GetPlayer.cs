using Actors;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Actions.Actors {
  [Category("Project12/Actors")]
  public class GetPlayer : ActionTask {
    public BBParameter<ActorBase> player; 
    protected override void OnExecute() {
      player.value = GameManager.Instance.PlayerController.Actor;
      EndAction(true);
    }
  }
}