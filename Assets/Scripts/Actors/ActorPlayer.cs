using System;
using SaveSystem;

namespace Actors {
  public class ActorPlayer : ActorBase, ISaveLoad {
    public static event Action OnPlayerDeath;

    protected override void Start() {
      SaveLoadSystem.Instance.Register(this);
      base.Start();
      Load();
    }

    public override void Damage(float damage, bool isPlayer) {
      base.Damage(damage, isPlayer);
      if (stats.Health <= 0) {
        OnPlayerDeath?.Invoke();
      }
    }

    public void Save() {
      SaveLoadSystem.Instance.gameData.PlayerData.IsDead = IsDead;
    }

    //TODO: fix
    public void Load() {
      var isDead = SaveLoadSystem.Instance.gameData.PlayerData.IsDead;

      if (!isDead) {
        return;
      }

      DeathActions();
      OnPlayerDeath?.Invoke();
    }
  }
}