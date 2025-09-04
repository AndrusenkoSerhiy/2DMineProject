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
      var xPos = -500;
      int checkDistance = distance;
      
      while (true){
        xPos = checkDistance;
        if (GameManager.Instance.ChunkController.ChunkData.GetCellFill(agentPos.X + xPos, agentPos.Y) != 1) {
          agentPos.X += xPos;
          position.value = agentPos;
          /*Debug.DrawRay(CoordsTransformer.GridToWorld(agentPos.X, agentPos.Y),
            Vector3.up * 10f, Color.red, 3f);*/
          EndAction(true);
          return;
        }
        if (GameManager.Instance.ChunkController.ChunkData.GetCellFill(agentPos.X + xPos, agentPos.Y - 1) != 1) {
          agentPos.X += xPos;
          agentPos.Y -= 1;
          position.value = agentPos;
          /*Debug.DrawRay(CoordsTransformer.GridToWorld(agentPos.X, agentPos.Y),
            Vector3.up * 10f, Color.green, 3f);*/
          EndAction(true);
          return;
        }

        //when zombie fall down and sit in cells
        if (checkDistance == 0) {
          EndAction(true);
          return;
        }

        if (checkDistance > 0)
          checkDistance--;
        else
          checkDistance++;
      }
    }
  }
}