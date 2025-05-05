using System;
using NodeCanvas.BehaviourTrees;
using Siege;
using UnityEngine;

namespace Actors {
  public class ActorBaseController : MonoBehaviour {
    [SerializeField] private ActorBase actor;
    
    [SerializeField] private BehaviourTree patrolBehaviour;
    [SerializeField] private BehaviourTree siegeBehaviour;
    private void Start() {
      GameManager.Instance.SiegeManager.OnZombieSpawn += SpawnSiegeZombie;
    }
    
    private void SpawnSiegeZombie(ActiveSiegeTemplate siege) {
       Debug.LogError($"spawn {siege.ZombieCount} zombies ");
       for (int i = 0; i < siege.ZombieCount; i++) {
         Spawn();
       }
    }

    private void Spawn() {
      var pos = GetPosition();
      var zombie = (ActorEnemy)Instantiate(actor, pos, Quaternion.identity);
      zombie.SetBehaviour(siegeBehaviour);
    }

    private Vector3 GetPosition() {
      return GameManager.Instance.PlayerController.transform.position +
             new Vector3(UnityEngine.Random.Range(-10, 10), 0, 0);
    }
  }
}