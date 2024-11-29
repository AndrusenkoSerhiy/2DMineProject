using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace World {
  public class CellObjectsPool : MonoBehaviour {
    [SerializeField] private CellObject cellObjectPrefab; // The prefab for CellObject
    public int initialPoolSize = 2500; // Maximum size of the pool
    private CellObject[] cellObjects;
    [SerializeField] private List<CellObject> cellObjectsList = new();

    public void Init() {
      cellObjects = new CellObject[initialPoolSize];
      for (int i = 0; i < initialPoolSize; i++) {
        cellObjects[i] = cellObjectsList[i];
      }
    }

    [ContextMenu("Fill CellObject Pool")]
    public void CreatePool() {
      for (int i = 0; i < initialPoolSize; i++) {
        cellObjectsList.Add(CreateNewObject());
      }
    }

    private CellObject CreateNewObject() {
      var newObj = Instantiate(cellObjectPrefab, transform);
      newObj.gameObject.SetActive(false); // Start inactive
      newObj.transform.SetParent(transform);
      return newObj;
    }

    // Method called when an object is taken from the pool
    private void OnTakeFromPool(CellObject obj, Vector3 pos) {
      obj.transform.position = pos;
      obj.gameObject.SetActive(true);
    }

    public CellObject Get(Vector3 pos) {
      CellObject target = null;
      for (int i = 0; i < initialPoolSize; i++) {
        if (cellObjects[i].gameObject.activeInHierarchy) continue;
        target = cellObjects[i];
        break;
      }

      if (!target) return null;
      OnTakeFromPool(target, pos);
      return target;
    }

    // Method called when an object is returned to the pool
    private void OnReturnedToPool(CellObject obj) {
      obj.transform.position = Vector3.zero;
      obj.gameObject.SetActive(false);
    }

    // Method to return an object to the pool
    public void ReturnObject(CellObject obj) {
      OnReturnedToPool(obj);
    }
  }
}