using Interface;
using Player;
using Scriptables;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Game {
  public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    [SerializeField] private TaskManager taskManagerRef;
    [SerializeField] private GameConfig gameConfigRef;
    [SerializeField] private ChunkController _chunkController;
    [SerializeField] private CellObjectsPool _cellObjectsPool;
    [SerializeField] private ChunkObjectsPool _chunkObjectsPool;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private ItemDatabaseObject _database;
    private PlayerInventory _playerInventory;
    public static GameManager instance {
      get {
        if (_instance == null) {
          _instance = FindAnyObjectByType<GameManager>();

          if (_instance == null) {
            Debug.LogError("GameManager instance not found in the scene.");
          }
        }
        return _instance;
      }
    }

    public ChunkController ChunkController => _chunkController;
    public GameConfig GameConfig => gameConfigRef;

    public CellObjectsPool cellObjectsPool {
      get { return _cellObjectsPool; }
    }
    public ChunkObjectsPool chunkObjectsPool {
      get { return _chunkObjectsPool; }
    }
    public TaskManager TaskManager {
      get { return taskManagerRef; }
    }

    public PlayerInventory PlayerInventory {
      set { _playerInventory = value; }
      get { return _playerInventory; }
    }

    public PlayerController PlayerController {
      set { _playerController = value; }
      get { return _playerController; }
    }

    public ItemDatabaseObject ItemDatabaseObject {
      get { return _database; }
    }
    private void Awake() {
      if (_instance != null && _instance != this) {
        Destroy(this.gameObject);
      }
      else {
        _instance = this;
        //DontDestroyOnLoad(this.gameObject);
      }
      UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
    }
  }
}
