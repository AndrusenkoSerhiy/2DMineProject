using Windows;
using Player;
using Scriptables;
using Scriptables.Items;
using UnityEngine;
using World;
using Inventory;

public class GameManager : MonoBehaviour {
  private static GameManager _instance;
  [SerializeField] private TaskManager taskManagerRef;
  [SerializeField] private GameConfig gameConfigRef;
  [SerializeField] private ChunkController chunkController;
  [SerializeField] private CellObjectsPool cellObjectsPool;
  [SerializeField] private PlayerController playerController;
  [SerializeField] private MiningRobotController miningRobotController;

  [SerializeField] private ItemDatabaseObject database;

  [SerializeField] private Camera mainCamera;
  [SerializeField] private WindowsController windowsController;
  [SerializeField] private PlayerInventory playerInventory;
  [SerializeField] private PlayerControllerBase currPlayerController;
  [SerializeField] private AnimatorParameters animatorParameters;
  [SerializeField] private PlayerEquipment playerEquipment;
  [SerializeField] private PlaceCell placeCell;
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

  public ChunkController ChunkController => chunkController;
  public GameConfig GameConfig => gameConfigRef;

  public CellObjectsPool CellObjectsPool => cellObjectsPool;

  public TaskManager TaskManager => taskManagerRef;

  public PlayerInventory PlayerInventory {
    set { playerInventory = value; }
    get { return playerInventory; }
  }

  public Camera MainCamera => mainCamera;
  public PlayerController PlayerController {
    set { playerController = value; }
    get { return playerController; }
  }

  public MiningRobotController MiningRobotController {
    set { miningRobotController = value; }
    get { return miningRobotController; }
  }

  public ItemDatabaseObject ItemDatabaseObject => database;

  public PlayerControllerBase CurrPlayerController {
    set { currPlayerController = value; }
    get { return currPlayerController; }
  }
  
  public PlayerEquipment PlayerEquipment => playerEquipment;

  public WindowsController WindowsController => windowsController;
  public PlaceCell PlaceCell => placeCell;

  public AnimatorParameters AnimatorParameters => animatorParameters;
  private void Awake() {
    if (_instance != null && _instance != this) {
      Destroy(gameObject);
    }
    else {
      _instance = this;
      //DontDestroyOnLoad(this.gameObject);
    }
    UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
  }
}