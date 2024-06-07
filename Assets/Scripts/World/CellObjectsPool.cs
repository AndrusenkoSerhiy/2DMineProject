using UnityEngine;
using UnityEngine.Pool;

namespace World{
  public class CellObjectsPool : MonoBehaviour{
    public CellObject cellObjectPrefab; // The prefab for CellObject
    public int initialPoolSize = 10; // Initial size of the pool
    public int maxPoolSize = 100; // Maximum size of the pool
    private ObjectPool<CellObject> pool;

    public void Init(){
      // Initialize the pool
      pool = new ObjectPool<CellObject>(
        createFunc: CreateNewObject,
        actionOnGet: OnTakeFromPool,
        actionOnRelease: OnReturnedToPool,
        actionOnDestroy: OnDestroyObject,
        collectionCheck: true,
        defaultCapacity: initialPoolSize,
        maxSize: maxPoolSize);
    }

    private CellObject CreateNewObject(){
      CellObject newObj = Instantiate(cellObjectPrefab,transform);
      newObj.gameObject.SetActive(false); // Start inactive
      return newObj.GetComponent<CellObject>();
    }

    // Method called when an object is taken from the pool
    private void OnTakeFromPool(CellObject obj){
      obj.gameObject.SetActive(true);
    }

    // Method called when an object is returned to the pool
    private void OnReturnedToPool(CellObject obj){
      obj.gameObject.SetActive(false);
    }

    // Method called when an object is destroyed
    private void OnDestroyObject(CellObject obj){
      Destroy(obj.gameObject);
    }

    // Method to get an object from the pool
    public CellObject GetObject(){
      return pool.Get();
    }

    // Method to return an object to the pool
    public void ReturnObject(CellObject obj){
      pool.Release(obj);
    }
  }
}