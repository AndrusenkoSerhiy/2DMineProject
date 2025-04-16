using Actors;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Conditions.Actors {
  [Category("Project12/Actors")]
  public class CheckDistance : ConditionTask<ActorEnemy> {
    public BBParameter<float> distanceToSee;
    public BBParameter<ActorBase> target;
    public Structures.EqualityE EqualityE;
    protected override bool OnCheck() {
      float distance = Vector3.Distance(agent.transform.position, target.value.transform.position);
      return Structures.Compare(distance, distanceToSee.value, EqualityE);
    }
  }
}