using System;
using Windows;
using Audio;
using Craft;
using Craft.Recipes;
using DG.Tweening;
using Player;
using Scriptables;
using Scriptables.Items;
using UnityEngine;
using World;
using Inventory;
using Scriptables.CameraController;
using Menu;
using Messages;
using Pool;
using Settings;
using UI;
using Utility;

[DefaultExecutionOrder(-5)]
public class GameManager : PersistentSingleton<GameManager> {
  [SerializeField] private StartGameCameraController startGameCameraController;
  [SerializeField] private MainMenu mainMenu;
  [SerializeField] private InGameMenu inGameMenu;
  [SerializeField] private UserInput userInput;
  [SerializeField] private TaskManager taskManagerRef;
  [SerializeField] private AudioManager audioManager;
  [SerializeField] private MessagesManager messagesManager;
  [SerializeField] private RecipesManager recipesManager;
  [SerializeField] private TooltipManager tooltipManager;
  [SerializeField] private CraftTasks craftTasks;
  [SerializeField] private GameConfig gameConfigRef;
  [SerializeField] private ChunkController chunkController;
  [SerializeField] private CellObjectsPool cellObjectsPool;
  [SerializeField] private ItemDatabaseObject database;
  [SerializeField] private Camera mainCamera;
  [SerializeField] private CameraConfigManager cameraConfigManager;
  [SerializeField] private Canvas canvas;
  [SerializeField] private WindowsController windowsController;
  [SerializeField] private PlayerInventory playerInventory;
  [SerializeField] private AnimatorParameters animatorParameters;
  [SerializeField] private PlayerEquipment playerEquipment;
  [SerializeField] private PlaceCell placeCell;
  [SerializeField] private UISettings uiSettings;
  [SerializeField] private SplitItem splitItem;
  [SerializeField] private GameObject tempDragItem;
  [SerializeField] private ObjectPooler poolEffects;

  private PlayerController playerController;
  private PlayerControllerBase currPlayerController;
  private MiningRobotController miningRobotController;
  private GameStage gameStage = GameStage.MainMenu;

  public StartGameCameraController StartGameCameraController => startGameCameraController;
  public MainMenu MainMenu => mainMenu;
  public InGameMenu InGameMenu => inGameMenu;
  public UserInput UserInput => userInput;
  public AudioManager AudioManager => audioManager;
  public MessagesManager MessagesManager => messagesManager;
  public RecipesManager RecipesManager => recipesManager;
  public TooltipManager TooltipManager => tooltipManager;
  public CraftTasks CraftTasks => craftTasks;
  public ChunkController ChunkController => chunkController;
  public UISettings UISettings => uiSettings;
  public SplitItem SplitItem => splitItem;
  public GameObject TempDragItem => tempDragItem;
  public GameConfig GameConfig => gameConfigRef;
  public CellObjectsPool CellObjectsPool => cellObjectsPool;
  public TaskManager TaskManager => taskManagerRef;
  public PlayerInventory PlayerInventory => playerInventory;
  public Camera MainCamera => mainCamera;
  public CameraConfigManager CameraConfigManager => cameraConfigManager;
  public Canvas Canvas => canvas;
  public ItemDatabaseObject ItemDatabaseObject => database;
  public PlayerEquipment PlayerEquipment => playerEquipment;
  public WindowsController WindowsController => windowsController;
  public PlaceCell PlaceCell => placeCell;
  public AnimatorParameters AnimatorParameters => animatorParameters;
  public ObjectPooler PoolEffects => poolEffects;

  public PlayerController PlayerController {
    set => playerController = value;
    get => playerController;
  }

  public MiningRobotController MiningRobotController {
    set => miningRobotController = value;
    get => miningRobotController;
  }

  public PlayerControllerBase CurrPlayerController {
    set => currPlayerController = value;
    get => currPlayerController;
  }

  public bool showMenuOnStart = true;

  protected override void Awake() {
    Debug.Log("Game manager awake");
    base.Awake();

    DOTween.Init();
    UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
  }

  private void Start() {
    if (!showMenuOnStart) {
      return;
    }

    startGameCameraController.Init();
    mainMenu.Show();
  }

  public void StartNewGame() {
    mainMenu.Hide();
    gameStage = GameStage.Game;
    startGameCameraController.Play();
  }

  public void ExitGame() {
    if (Application.isEditor) {
      UnityEditor.EditorApplication.isPlaying = false;
    }
    else {
      Application.Quit();
    }
  }

  public void ExitToMainMenu() {
    startGameCameraController.Init();
    inGameMenu.Hide();
    mainMenu.Show();
  }

  public void ShowInGameMenu() {
    inGameMenu.Show();
  }
}