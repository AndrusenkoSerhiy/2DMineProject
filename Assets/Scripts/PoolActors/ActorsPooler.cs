using System.Collections.Generic;
using Actors;
using Pool;
using UnityEngine;

namespace PoolActors {
  public class ActorsPooler : MonoBehaviour {
    [System.Serializable]
    public class Pool {
      public string tag;
      public GameObject prefab;
      public int size;
    }

    public List<ObjectPooler.Pool> pools;
    private Dictionary<string, Queue<ActorBase>> poolDictionary;

    void Start() {
      poolDictionary = new Dictionary<string, Queue<ActorBase>>();

      foreach (ObjectPooler.Pool pool in pools) {
        Queue<ActorBase> objectPool = new Queue<ActorBase>();

        for (int i = 0; i < pool.size; i++) {
          GameObject obj = Instantiate(pool.prefab, transform);
          obj.SetActive(false);
          objectPool.Enqueue(obj.GetComponent<ActorBase>());
        }

        poolDictionary.Add(pool.tag, objectPool);
      }
    }

    public ActorBase SpawnFromPool(string tag, Vector3 position, Quaternion rotation) {
      if (!poolDictionary.ContainsKey(tag)) {
        Debug.LogError("Pool with tag " + tag + " doesn't exist.");
        return null;
      }

      Queue<ActorBase> objectPool = poolDictionary[tag];
      ActorBase objectToSpawn = null;

      for (int i = 0; i < objectPool.Count; i++) {
        ActorBase obj = objectPool.Dequeue();

        if (!obj.gameObject.activeInHierarchy) {
          objectToSpawn = obj;
          break;
        }

        objectPool.Enqueue(obj);
      }

      if (objectToSpawn == null) {
        ObjectPooler.Pool poolConfig = pools.Find(p => p.tag == tag);

        if (poolConfig != null) {
          GameObject newObj = Instantiate(poolConfig.prefab, transform);
          objectToSpawn = newObj.GetComponent<ActorBase>();
        }
      }

      objectToSpawn.gameObject.SetActive(true);
      objectToSpawn.transform.position = position;
      objectToSpawn.transform.rotation = rotation;

      poolDictionary[tag].Enqueue(objectToSpawn);

      return objectToSpawn;
    }
  }
}