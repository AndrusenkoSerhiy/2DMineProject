using System.Collections.Generic;
using Scriptables;
using UnityEngine;
using Utils;

namespace World {
  public class ChunkSpecialPointsSpawner : MonoBehaviour {
    public ResourceData LootPointSpawnerData;
    public ResourceData ZombieSpawnerData;
    public List<Building> possibleBuildings = new();
    private ChunkController chunkController;

    public bool CheckData(ResourceData data, Coords coords, CellObject cellObject) {
      if (!chunkController) chunkController = GameManager.Instance.ChunkController;
      var loot = data == LootPointSpawnerData;
      var zombie = data == ZombieSpawnerData;
      if (!loot && !zombie) return false;
      cellObject.DestroySilent();
      if (loot)
        SpawnChest(coords);
      if (zombie)
        SpawnZombie(coords);
      return true;
    }

    private void SpawnChest(Coords coords) {
      var randBuilding = possibleBuildings[Random.Range(0, possibleBuildings.Count)];
      var convertedCoords = CoordsTransformer.GridToBuildingsGrid(coords);
      var build = chunkController.SpawnBuild(convertedCoords, randBuilding);
    }

    private void SpawnZombie(Coords coords) {
      var pos = CoordsTransformer.GridToWorld(coords.X, coords.Y);
      GameManager.Instance.ActorBaseController.SpawnPatrolZombie(pos);
    }
  }
}