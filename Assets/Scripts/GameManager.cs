using UnityEngine;
using World;

namespace Game {
  public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    [SerializeField] private TaskManager taskManagerRef;
    [SerializeField] private ChunkController _chunkController;
    [SerializeField] private CellObjectsPool _cellObjectsPool;
    [SerializeField] private ChunkObjectsPool _chunkObjectsPool;
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

    public ChunkController ChunkController => _chunkController;
    
    public CellObjectsPool cellObjectsPool {
      get { return _cellObjectsPool; }
    }  
    public ChunkObjectsPool chunkObjectsPool {
      get { return _chunkObjectsPool; }
    }  
    public TaskManager TaskManager {
      get { return taskManagerRef; }
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
