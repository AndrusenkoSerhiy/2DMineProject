using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;

namespace World {
  public class CellObjectsPool : MonoBehaviour {
    [SerializeField] private CellObject cellObjectPrefab; // The prefab for CellObject
    public int initialPoolSize = 2500; // Maximum size of the pool
    private CellObject[] cellObjects;
    [SerializeField] private List<CellObject> cellObjectsList = new();
    [SerializeField] private Vector3 initialPos;

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
    
    [ContextMenu("Clear CellObject Pool")]
    public void ClearPool() {
      for (int i = 0; i < initialPoolSize; i++) {
        DestroyImmediate(cellObjectsList[i].gameObject);
      }
      cellObjectsList.Clear();
    }

    private CellObject CreateNewObject() {
      var newObj = Instantiate(cellObjectPrefab, transform);
      newObj.transform.SetParent(transform);
      newObj.transform.position = initialPos;
      return newObj;
    }

    // Method called when an object is taken from the pool
    private void OnTakeFromPool(CellObject obj, Vector3 pos) {
      obj.IsActive = true;
      obj.transform.position = pos;
    }

    public CellObject Get(Vector3 pos) {
      CellObject target = null;
      for (int i = 0; i < initialPoolSize; i++) {
        if (cellObjects[i].IsActive) continue;
        target = cellObjects[i];
        break;
      }

      if (target == null) return null;
      OnTakeFromPool(target, pos);
      return target;
    }

    // Method called when an object is returned to the pool
    private void OnReturnedToPool(CellObject obj) {
      obj.transform.position = initialPos;
      obj.IsActive = false;
    }

    // Method to return an object to the pool
    public void ReturnObject(CellObject obj) {
      OnReturnedToPool(obj);
    }
  }
}