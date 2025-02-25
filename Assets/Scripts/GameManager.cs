using Windows;
using DG.Tweening;
using Player;
using Scriptables;
using Scriptables.Items;
using UnityEngine;
using World;
using Inventory;
using Utility;

public class GameManager : PersistentSingleton<GameManager> {
  [SerializeField] private TaskManager taskManagerRef;
  [SerializeField] private GameConfig gameConfigRef;
  [SerializeField] private ChunkController chunkController;
  [SerializeField] private CellObjectsPool cellObjectsPool;
  [SerializeField] private PlayerController playerController;
  [SerializeField] private MiningRobotController miningRobotController;

  [SerializeField] private ItemDatabaseObject database;

  [SerializeField] private Camera mainCamera;
  [SerializeField] private Canvas canvas;
  [SerializeField] private WindowsController windowsController;
  [SerializeField] private PlayerInventory playerInventory;
  [SerializeField] private PlayerControllerBase currPlayerController;
  [SerializeField] private AnimatorParameters animatorParameters;
  [SerializeField] private PlayerEquipment playerEquipment;
  [SerializeField] private PlaceCell placeCell;
  [SerializeField] private UISettings uiSettings;
  [SerializeField] private SplitItem splitItem;
  [SerializeField] private GameObject tempDragItem;

  public ChunkController ChunkController => chunkController;
  public UISettings UISettings => uiSettings;
  public SplitItem SplitItem => splitItem;
  public GameObject TempDragItem => tempDragItem;
  public GameConfig GameConfig => gameConfigRef;

  public CellObjectsPool CellObjectsPool => cellObjectsPool;

  public TaskManager TaskManager => taskManagerRef;

  public PlayerInventory PlayerInventory => playerInventory;

  public Camera MainCamera => mainCamera;
  public Canvas Canvas => canvas;

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
    base.Awake();

    DOTween.Init();
    UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
  }
}