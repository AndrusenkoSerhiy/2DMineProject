using System.Collections.Generic;
using Scriptables.Items;
using UnityEngine;

namespace Pool {
  public class ObjectPooler : MonoBehaviour {
    [System.Serializable]
    public class Pool {
      public string tag;
      public GameObject prefab;
      public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<PoolObjectBase>> poolDictionary;

    public static ObjectPooler Instance;

    private void Awake() {
      Instance = this;
    }

    void Start() {
      poolDictionary = new Dictionary<string, Queue<PoolObjectBase>>();

      foreach (Pool pool in pools) {
        Queue<PoolObjectBase> objectPool = new Queue<PoolObjectBase>();

        for (int i = 0; i < pool.size; i++) {
          GameObject obj = Instantiate(pool.prefab, transform);
          obj.SetActive(false);
          objectPool.Enqueue(obj.GetComponent<PoolObjectBase>());
        }

        poolDictionary.Add(pool.tag, objectPool);
      }
    }

    public PoolObjectBase SpawnFromPool(string tag, Vector3 position, Quaternion rotation) {
      if (!poolDictionary.ContainsKey(tag)) {
        Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
        return null;
      }

      Queue<PoolObjectBase> objectPool = poolDictionary[tag];
      PoolObjectBase objectToSpawn = null;

      for (int i = 0; i < objectPool.Count; i++) {
        PoolObjectBase obj = objectPool.Dequeue();

        if (!obj.gameObject.activeInHierarchy) {
          objectToSpawn = obj;
          break;
        }

        objectPool.Enqueue(obj);
      }

      if (objectToSpawn == null) {
        Pool poolConfig = pools.Find(p => p.tag == tag);

        if (poolConfig != null) {
          GameObject newObj = Instantiate(poolConfig.prefab, transform);
          objectToSpawn = newObj.GetComponent<PoolObjectBase>();
        }
      }

      objectToSpawn.gameObject.SetActive(true);
      objectToSpawn.transform.position = position;
      objectToSpawn.transform.rotation = rotation;

      poolDictionary[tag].Enqueue(objectToSpawn);

      return objectToSpawn;
    }

    //TODO
    public void SpawnFlyEffect(ItemObject item, Vector3 cellPos) {
      var fly = (PoolObjectFly)SpawnFromPool("WavyMove", cellPos, Quaternion.identity) as PoolObjectFly;
      if (fly != null) {
        fly.SetSprite(item.UiDisplay);
        fly.SetPosition(cellPos, GameManager.instance.PlayerController.transform);
      }
    }
  }
}