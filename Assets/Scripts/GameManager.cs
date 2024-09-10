using UnityEngine;
using World;

namespace Game {
  public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    [SerializeField] private CellObjectsPool pool;

    public static GameManager instance {
      get {
        if (_instance == null) {
          _instance = FindObjectOfType<GameManager>();

          if (_instance == null) {
            Debug.LogError("GameManager instance not found in the scene.");
          }
        }
        return _instance;
      }
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
        DontDestroyOnLoad(this.gameObject);
      }
    }
  }
}
