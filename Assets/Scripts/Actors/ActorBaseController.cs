using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using SaveSystem;
using Scriptables.Siege;
using Siege;
using Stats;
using UnityEngine;
using Utils;
using World;

namespace Actors {
  public class ActorBaseController : MonoBehaviour, ISaveLoad {
    [SerializeField] private ActorBase actor;
    [SerializeField] private BehaviourTree patrolBehaviour;
    [SerializeField] private BehaviourTree siegeBehaviour;
    [SerializeField] private List<ActorEnemy> enemies = new();
    private SiegeManager siegeManager;
    private int areaWidth;
    private int areaHeight;

    private void Start() {
      SaveLoadSystem.Instance.Register(this);
      siegeManager = GameManager.Instance.SiegeManager;
      GameManager.Instance.SiegeManager.OnZombieSpawn += SpawnSiegeZombie;
      areaWidth = GameManager.Instance.GameConfig.PlayerAreaWidth;
      areaHeight = GameManager.Instance.GameConfig.PlayerAreaHeight;
    }

    #region Save/Load

    public int Priority => LoadPriority.ENEMIES;

    public void Save() {
      var data = SaveLoadSystem.Instance.gameData.Zombies;

      foreach (var enemy in enemies) {
        var enemyData = new ZombiesData {
          ProfileId = enemy.Difficulty.Id,
          Position = enemy.transform.position,
          Rotation = enemy.transform.rotation,
          Scale = enemy.transform.localScale,
          PlayerStatsData = enemy.GetStats().PrepareSaveData()
        };
        data.Add(enemyData);
      }
    }

    public void Load() {
      if (SaveLoadSystem.Instance.IsNewGame()) {
        return;
      }

      var data = SaveLoadSystem.Instance.gameData.Zombies;
      foreach (var zombieData in data) {
        var profile = siegeManager.ZombieDifficultyDatabase.ItemsMap[zombieData.ProfileId];
        if (profile == null) {
          continue;
        }

        var zombie = (ActorEnemy)Instantiate(actor, zombieData.Position, zombieData.Rotation);
        zombie.transform.localScale = zombieData.Scale;
        zombie.SetBehaviour(siegeBehaviour);
        zombie.SetDifficulty(profile);
        zombie.GetStats().UpdateBaseValue(StatType.Health, zombieData.PlayerStatsData.Health);
        enemies.Add(zombie);
      }

      data.Clear();
    }

    public void Clear() {
      enemies.Clear();
    }

    #endregion

    private void SpawnSiegeZombie(ActiveSiegeTemplate siege) {
      //Debug.LogError($"difficulty list {GetDifficultyList().Count}");
      var difficultyList = GetDifficultyList();

      for (int i = 0; i < difficultyList.Count; i++) {
        var count = Mathf.RoundToInt(difficultyList[i].percentage * siege.ZombieCount / 100);
        Spawn(difficultyList[i].profile, count);
      }
    }

    private void Spawn(ZombieDifficultyProfile profile, int count) {
      for (int i = 0; i < count; i++) {
        //Debug.LogError($"spawn {profile}");
        var pos = GetPosition();
        var zombie = (ActorEnemy)Instantiate(actor, pos, Quaternion.identity);
        zombie.SetBehaviour(siegeBehaviour);
        zombie.SetDifficulty(profile);
        enemies.Add(zombie);
      }
    }

    private List<DifficultyProfile> GetDifficultyList() {
      return siegeManager.GetDifficultyProfilesByWeight().profiles;
    }

    private Vector3 GetPosition() {
      return TryGetFreeCell();
      //return UnityEngine.Random.Range(0, 2) == 0 ? GetLeftPos(false) : GetLeftPos();
    }

    private void Update() {
      if (Input.GetKeyDown(KeyCode.O)) {
        //TryGetFreeCell();
        GetUpPos();
      }
    }

    //try to get pos from left or right side from player (out of visible zone)
    private Vector3 GetLeftPos(bool left = true) {
      var playerPos = GameManager.Instance.PlayerController.PlayerCoords.GetCoords();

      var leftX = left ? playerPos.X - (areaWidth / 2) + 1 : playerPos.X + (areaWidth / 2) - 1;
      var rightX = left ? playerPos.X - areaWidth / 4 : playerPos.X + areaWidth / 4;

      var leftPos = CoordsTransformer.GridToWorld(leftX, playerPos.Y);
      var rightPos = CoordsTransformer.GridToWorld(rightX, playerPos.Y);
      var randomX = UnityEngine.Random.Range(leftPos.x, rightPos.x);
      return new Vector3(randomX, leftPos.y, 0);
    }

    //try get pos above the player visible zone
    private Vector3 GetUpPos() {
      var playerPos = GameManager.Instance.PlayerController.PlayerCoords.GetCoords();


      var leftX = playerPos.X - (areaWidth / 4) + 1;
      var rightX = playerPos.X + (areaWidth / 4) - 1;

      var upY = playerPos.Y - (areaHeight / 4);
      //var upY = playerPos.Y - (areaHeight / 2) + 1;
      var upDownY = playerPos.Y - (areaHeight / 6);
      //var upDownY = playerPos.Y - (areaHeight / 4);

      var leftPos = CoordsTransformer.GridToWorld(leftX, playerPos.Y);
      var rightPos = CoordsTransformer.GridToWorld(rightX, playerPos.Y);
      var upHighPos = CoordsTransformer.GridToWorld(playerPos.X, upY);
      var upLowPos = CoordsTransformer.GridToWorld(playerPos.X, upDownY);
      var randomX = UnityEngine.Random.Range(leftPos.x, rightPos.x);
      var randomY = UnityEngine.Random.Range(upLowPos.y, upHighPos.y);
      //Debug.DrawRay(new Vector3(randomX, randomY, 0), Vector3.up, Color.red, 3f);
      return new Vector3(randomX, randomY, 0);
    }

    private Vector3 TryGetFreeCell() {
      var rndPos = UnityEngine.Random.Range(0, 2) == 0 ? GetLeftPos(false) : GetLeftPos();
      var coords = CoordsTransformer.MouseToGridPosition(rndPos);
      var chunkData = GameManager.Instance.ChunkController.ChunkData;

      var free = Vector3.zero;
      if (chunkData.GetCellFill(coords.X, coords.Y) == 1) {
        //Debug.LogError("need to find empty cell");
        free = FindAboveCell(coords);

        if (free == Vector3.zero)
          free = FindUnderCell(coords);
      }
      else {
        free = rndPos;
      }

      //if player on the top of grid
      if (coords.Y < 0) {
        free = rndPos;
      }

      if (free == Vector3.zero) {
        rndPos = GetUpPos();
        coords = CoordsTransformer.MouseToGridPosition(rndPos);
        if (chunkData.GetCellFill(coords.X, coords.Y) == 1) {
          free = FindLeftCell(coords);

          if (free == Vector3.zero)
            free = FindRightCell(coords);
        }
        else {
          free = rndPos;
        }
      }

      if (free != Vector3.zero) Debug.DrawRay(free, Vector3.up, Color.green, 3f);
      else Debug.DrawRay(rndPos, Vector3.up, Color.red, 3f);
      return free;
    }

    private Vector3 FindAboveCell(Coords coords) {
      var chunkData = GameManager.Instance.ChunkController.ChunkData;
      for (int i = 1; i < 5; i++) {
        Debug.DrawRay(CoordsTransformer.GridToWorld(coords.X, coords.Y - i), Vector3.up * 2, Color.blue, 2f);

        if (chunkData.GetCellFill(coords.X, coords.Y - i) == 0) {
          return CoordsTransformer.GridToWorld(coords.X, coords.Y - i);
        }
      }

      return Vector3.zero;
    }

    private Vector3 FindUnderCell(Coords coords) {
      var chunkData = GameManager.Instance.ChunkController.ChunkData;
      for (int i = 1; i < 5; i++) {
        Debug.DrawRay(CoordsTransformer.GridToWorld(coords.X, coords.Y + i), Vector3.up * 2, Color.blue, 2f);

        if (chunkData.GetCellFill(coords.X, coords.Y + i) == 0) {
          return CoordsTransformer.GridToWorld(coords.X, coords.Y + i);
        }
      }

      return Vector3.zero;
    }

    private Vector3 FindLeftCell(Coords coords) {
      var chunkData = GameManager.Instance.ChunkController.ChunkData;
      for (int i = 1; i < 5; i++) {
        Debug.DrawRay(CoordsTransformer.GridToWorld(coords.X - i, coords.Y), Vector3.up * 2, Color.blue, 2f);

        if (chunkData.GetCellFill(coords.X - i, coords.Y) == 0) {
          return CoordsTransformer.GridToWorld(coords.X - i, coords.Y);
        }
      }

      return Vector3.zero;
    }

    private Vector3 FindRightCell(Coords coords) {
      var chunkData = GameManager.Instance.ChunkController.ChunkData;
      for (int i = 1; i < 5; i++) {
        Debug.DrawRay(CoordsTransformer.GridToWorld(coords.X + i, coords.Y), Vector3.up * 2, Color.blue, 2f);

        if (chunkData.GetCellFill(coords.X + i, coords.Y) == 0) {
          return CoordsTransformer.GridToWorld(coords.X + i, coords.Y);
        }
      }

      return Vector3.zero;
    }
  }
}