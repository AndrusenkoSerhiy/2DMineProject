using Windows;
using Player;
using Scriptables;
using Scriptables.Items;
using UnityEngine;
using World;

public class GameManager : MonoBehaviour {
  private static GameManager _instance;
  [SerializeField] private TaskManager taskManagerRef;
  [SerializeField] private GameConfig gameConfigRef;
  [SerializeField] private ChunkController _chunkController;
  [SerializeField] private CellObjectsPool _cellObjectsPool;
  [SerializeField] private PlayerController _playerController;
  [SerializeField] private MiningRobotController _miningRobotController;

  [SerializeField] private PlayerAttack _playerAttack;
  [SerializeField] private ItemDatabaseObject _database;
  
  [SerializeField] private Camera _mainCamera;
  [SerializeField] private WindowsController _windowsController;
  [SerializeField] private PlayerInventory _playerInventory;
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

  public TaskManager TaskManager {
    get { return taskManagerRef; }
  }

  public PlayerInventory PlayerInventory {
    set { _playerInventory = value; }
    get { return _playerInventory; }
  }

  public Camera MainCamera => _mainCamera;
  public PlayerController PlayerController {
    set { _playerController = value; }
    get { return _playerController; }
  }

  public PlayerAttack PlayerAttack {
    set { _playerAttack = value; }
    get { return _playerAttack; }
  }
  
  public MiningRobotController MiningRobotController {
    set { _miningRobotController = value; }
    get { return _miningRobotController; }
  }

  public ItemDatabaseObject ItemDatabaseObject {
    get { return _database; }
  }

  public WindowsController WindowsController {
    get { return _windowsController; }
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