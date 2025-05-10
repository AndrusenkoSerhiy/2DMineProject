using System;
using SaveSystem;

namespace Actors {
  public class ActorPlayer : ActorBase, ISaveLoad {
    public static event Action OnPlayerDeath;

    protected override void Start() {
      SaveLoadSystem.Instance.Register(this);
      base.Start();
    }

    #region Save/Load

    public int Priority => LoadPriority.ACTORS;

    public void Clear() {
    }

    public void Save() {
      SaveLoadSystem.Instance.gameData.PlayerData.IsDead = IsDead;
    }

    public void Load() {
      var isDead = SaveLoadSystem.Instance.gameData.PlayerData.IsDead;

      if (!isDead) {
        return;
      }

      DeathActions();
      OnPlayerDeath?.Invoke();
    }

    #endregion

    public override void Damage(float damage) {
      base.Damage(damage);
      if (stats.Health <= 0) {
        OnPlayerDeath?.Invoke();
      }
    }
  }
}