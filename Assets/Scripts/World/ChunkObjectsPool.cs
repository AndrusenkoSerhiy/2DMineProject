using UnityEngine;
using UnityEngine.Pool;

namespace World {
  public class ChunkObjectsPool : MonoBehaviour {
    public ChunkObject chunkObjectPrefab; // The prefab for CellObject
    public int initialPoolSize = 10; // Initial size of the pool
    public int maxPoolSize = 100; // Maximum size of the pool
    private ObjectPool<ChunkObject> pool;

    public void Init() {
      // Initialize the pool
      pool = new ObjectPool<ChunkObject>(
        createFunc: CreateNewObject,
        actionOnGet: OnTakeFromPool,
        actionOnRelease: OnReturnedToPool,
        actionOnDestroy: OnDestroyObject,
        collectionCheck: true,
        defaultCapacity: initialPoolSize,
        maxSize: maxPoolSize);
    }

    private ChunkObject CreateNewObject() {
      ChunkObject newObj = Instantiate(chunkObjectPrefab, transform);
      newObj.gameObject.SetActive(false); // Start inactive
      return newObj;
    }

    // Method called when an object is taken from the pool
    private void OnTakeFromPool(ChunkObject obj) {
      obj.gameObject.SetActive(true);
    }

    // Method called when an object is returned to the pool
    private void OnReturnedToPool(ChunkObject obj) {
      obj.gameObject.SetActive(false);
    }

    // Method called when an object is destroyed
    private void OnDestroyObject(ChunkObject obj) {
      Destroy(obj.gameObject);
    }

    // Method to get an object from the pool
    public ChunkObject GetObject() {
      return pool.Get();
    }

    // Method to return an object to the pool
    public void ReturnObject(ChunkObject obj) {
      pool.Release(obj);
    }
  }
}