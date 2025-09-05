using System;
using Analytics;
using SaveSystem;
using UnityEngine;

namespace Actors {
  public class ActorPlayer : ActorBase, ISaveLoad {
    public static event Action OnPlayerDeath;
    public static event Action OnPlayerRespawn;

    protected override void Awake() {
      base.Awake();
      DamageableType = DamageableType.Player;
    }

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

    public override void Damage(float damage, bool isPlayer) {
      if (stats.Health > 0) {
        GameManager.Instance.AudioController.PlayPlayerDamaged();
      }

      base.Damage(damage, isPlayer);

      if (stats.Health <= 0 && !IsDead) {
        OnPlayerDeath?.Invoke();
      }
    }

    public override void Respawn() {
      base.Respawn();
      OnPlayerRespawn?.Invoke();
    }
  }
}