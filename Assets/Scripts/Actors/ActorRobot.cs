using System;
using Scriptables.Repair;
using Random = UnityEngine.Random;

namespace Actors {
  public class ActorRobot : ActorBase {
    public RobotObject robotObject;
    public static event Action OnRobotBroked;
    public static event Action OnRobotRepaired;

    protected override void Awake() {
      base.Awake();
      DamageableType = DamageableType.Robot;
    }

    public override void Damage(float damage, bool isPlayer) {
      base.Damage(damage, isPlayer);

      DamageAudio();

      if (IsDead) {
        OnRobotBroked?.Invoke();
      }
    }

    public override void Respawn() {
      base.Respawn();
      hasTakenDamage = false;
      OnRobotRepaired?.Invoke();
    }

    private void DamageAudio() {
      if (IsDead || !robotObject || robotObject.damagedAudioData.Count == 0) {
        return;
      }

      GameManager.Instance.AudioController.PlayAudio(
        robotObject.damagedAudioData[Random.Range(0, robotObject.damagedAudioData.Count)]);
    }
  }
}