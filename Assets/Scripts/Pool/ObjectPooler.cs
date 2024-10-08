using System.Collections.Generic;
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
    [SerializeField] private Dictionary<string, Queue<GameObject>> poolDictionary;

    // Singleton pattern to access the pooler from other scripts
    public static ObjectPooler Instance;

    private void Awake() {
      Instance = this;
    }

    void Start() {
      poolDictionary = new Dictionary<string, Queue<GameObject>>();

      foreach (Pool pool in pools) {
        Queue<GameObject> objectPool = new Queue<GameObject>();

        for (int i = 0; i < pool.size; i++) {
          GameObject obj = Instantiate(pool.prefab, transform);
          obj.SetActive(false);
          objectPool.Enqueue(obj);
        }

        poolDictionary.Add(pool.tag, objectPool);
      }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation) {
      if (!poolDictionary.ContainsKey(tag)) {
        Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
        return null;
      }

      GameObject objectToSpawn = poolDictionary[tag].Dequeue();

      objectToSpawn.SetActive(true);
      objectToSpawn.transform.position = position;
      objectToSpawn.transform.rotation = rotation;

      poolDictionary[tag].Enqueue(objectToSpawn);

      return objectToSpawn;
    }

    public void ReturnToPool() {
      
    }
  }
}