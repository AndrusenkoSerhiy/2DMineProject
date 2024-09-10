using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;

namespace Game {
  public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    [SerializeField] private CellObjectsPool pool;

    public static GameManager instance {
      get { return _instance; }
    }

    public CellObjectsPool cellObjectsPool {
      get { return pool; }
    }

    private void Awake() {
      if (_instance != null && _instance != this) {
        Destroy(this.gameObject);
      }
      else {
        _instance = this;
      }
    }
  }
}
