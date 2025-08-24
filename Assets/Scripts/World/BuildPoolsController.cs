using System.Collections.Generic;
using Scriptables;
using UnityEngine;

namespace World {
  public class BuildPoolsController : MonoBehaviour {
    [SerializeField] private List<BuildObjectPool> objectsToSpawn = new();

    public BuildingDataObject Get(Building type, Vector3 position) {
      var pool = FindPool(type);
      if (!pool) {
        return null;
      }

      return pool.Get(position);
    }

    public void ReturnObject(BuildingDataObject obj) {
      var pool = FindPool(obj.Building);
      if (!pool) return;
      pool.ReturnObject(obj);
    }

    private BuildObjectPool FindPool(Building type) {
      BuildObjectPool pool = null;
      for (int i = 0; i < objectsToSpawn.Count; i++) {
        if (objectsToSpawn[i].BuildingRef == type) {
          pool = objectsToSpawn[i];
          break;
        }
      }

      return pool;
    }
  }
}