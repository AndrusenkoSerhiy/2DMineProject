using Actors;
using UnityEngine;

namespace Player {
  public class ObjectHighlightActor : ObjectHighlight {
    [SerializeField] ActorEnemy actor;

    private void Start() {
      actor.OnEnemyDied += () => { SetHighlight(false); };
    }
    public override bool CanShowHighlight() {
      return !actor.IsDead;
    }
  }
}