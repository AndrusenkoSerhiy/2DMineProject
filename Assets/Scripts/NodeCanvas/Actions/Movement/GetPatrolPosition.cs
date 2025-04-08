using Actors;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Utils;
using World;

namespace NodeCanvas.Actions.Movement {
  [Category("Project12/Movement")]
  public class GetPatrolPosition : ActionTask<ActorEnemy> {
    public BBParameter<Coords> position;
    public int distance = 1;
    protected override void OnExecute() {
      var agentPos = agent.GetCoords;
      agentPos.X += distance;
      position.value = agentPos;
      /*Debug.LogError($"Get Patrol Position {agentPos.X} | {agentPos.Y}");
      Debug.DrawRay(CoordsTransformer.GridToWorld(agentPos.X, agentPos.Y),
        Vector3.up * 10f, Color.red, 5f);*/
      EndAction(true);
    }
  }
}