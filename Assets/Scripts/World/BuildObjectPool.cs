using System.Collections.Generic;
using Scriptables;
using UnityEngine;

namespace World {
  public class BuildObjectPool : MonoBehaviour {
    public Building BuildingRef;
    [SerializeField] private BuildingDataObject buildObjectPrefab;
    [SerializeField] private int initialPoolSize = 5;


    private BuildingDataObject[] buildObjects;
    [SerializeField] private List<BuildingDataObject> buildObjectsList = new();
    [SerializeField] private Vector3 initialPos;

    public void Awake() {
      buildObjects = new BuildingDataObject[initialPoolSize];
      for (int i = 0; i < initialPoolSize; i++) {
        buildObjects[i] = buildObjectsList[i];
      }
    }

    [ContextMenu("Fill BuildObjectPool Pool")]
    public void CreatePool() {
      for (int i = 0; i < initialPoolSize; i++) {
        buildObjectsList.Add(CreateNewObject());
      }
    }

    [ContextMenu("Clear BuildObjectPool Pool")]
    public void ClearPool() {
      for (int i = 0; i < initialPoolSize; i++) {
        DestroyImmediate(buildObjectsList[i].gameObject);
      }

      buildObjectsList.Clear();
    }

    private BuildingDataObject CreateNewObject() {
      var newObj = Instantiate(buildObjectPrefab, transform);
      newObj.transform.SetParent(transform);
      newObj.transform.position = initialPos;
      return newObj;
    }

    // Method called when an object is taken from the pool
    private void OnTakeFromPool(BuildingDataObject obj, Vector3 pos) {
      obj.IsActive = true;
      obj.transform.position = pos;
    }

    public BuildingDataObject Get(Vector3 pos) {
      BuildingDataObject target = null;
      for (int i = 0; i < initialPoolSize; i++) {
        if (buildObjects[i].IsActive) continue;
        target = buildObjects[i];
        break;
      }

      if (target == null) target = CreateNewObject();
      OnTakeFromPool(target, pos);
      return target;
    }

    // Method called when an object is returned to the pool
    private void OnReturnedToPool(BuildingDataObject obj) {
      obj.transform.position = initialPos;
      obj.IsActive = false;
    }

    // Method to return an object to the pool
    public void ReturnObject(BuildingDataObject obj) {
      OnReturnedToPool(obj);
    }
  }
}