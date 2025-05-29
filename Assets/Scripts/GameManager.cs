using System;
using System.Collections.Generic;
using Windows;
using Audio;
using Craft;
using Craft.Recipes;
using DG.Tweening;
using Interaction;
using Player;
using Scriptables;
using Scriptables.Items;
using UnityEngine;
using World;
using Inventory;
using Items;
using Scriptables.CameraController;
using Menu;
using Messages;
using Movement;
using Pool;
using PoolActors;
using Quests;
using ScriptableObjects;
using Scriptables.DropZombieData;
using Settings;
using Siege;
using Stats;
using Tools;
using UI;
using UnityEngine.Serialization;
using Utility;

[DefaultExecutionOrder(-5)]
public class GameManager : PersistentSingleton<GameManager> {
  [SerializeField] private StartGameCameraController startGameCameraController;
  [SerializeField] private MenuController menuController;
  [SerializeField] private UserInput userInput;
  [SerializeField] private TaskManager taskManagerRef;
  [SerializeField] private AudioController audioController;
  [SerializeField] private MessagesManager messagesManager;
  [SerializeField] private RecipesManager recipesManager;
  [SerializeField] private TooltipManager tooltipManager;
  [SerializeField] private CraftManager craftManager;
  [SerializeField] private GroundItemPool groundItemPool;
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
  [SerializeField] private QuickSlotListener quickSlotListener;

  [SerializeField] private MapController mapController;
  [SerializeField] private InteractionPrompt interactionPromtUI;
  [SerializeField] private ListOfTmpSpriteAssets listOfTmpSpriteAssets;
  [SerializeField] private RespawnManager respawnManager;
  [SerializeField] private BuildPoolsController buildPoolsController;
  [SerializeField] private BuildingsDataController buildingsDataController;
  [SerializeField] private DropZombieData dropZombieData;
  [SerializeField] private SiegeManager siegeManager;

  [SerializeField] private ModifiersDatabaseObject modifiersDatabase;

  [SerializeField] private ActorsPooler actorsPooler;
  [SerializeField] private DynamicCrosshair dynamicCrosshair;
  [SerializeField] private BulletsPool bulletsPool;
  [SerializeField] private Locator locator;
  [SerializeField] private QuestManager questManager;

  //TODO 
  //robot don't need this param in own script
  [SerializeField] private LadderMovement playerLadderMovement;
  public bool showMenuOnStart = true;

  [FormerlySerializedAs("objectList")]
  [Tooltip("UI object that need to be disabled when we start from menu")]
  [SerializeField]
  private List<GameObject> uiObjectList;

  private PlayerController playerController;
  private PlayerControllerBase currPlayerController;
  private MiningRobotController miningRobotController;
  private GameStage gameStage = GameStage.MainMenu;
  private IStatModifierFactory statModifierFactory;

  public StartGameCameraController StartGameCameraController => startGameCameraController;
  public MenuController MenuController => menuController;
  public UserInput UserInput => userInput;
  public AudioController AudioController => audioController;
  public MessagesManager MessagesManager => messagesManager;
  public RecipesManager RecipesManager => recipesManager;
  public TooltipManager TooltipManager => tooltipManager;
  public CraftManager CraftManager => craftManager;
  public GroundItemPool GroundItemPool => groundItemPool;
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
  public QuickSlotListener QuickSlotListener => quickSlotListener;
  public LadderMovement PlayerLadderMovement => playerLadderMovement;
  public MapController MapController => mapController;
  public IStatModifierFactory StatModifierFactory => statModifierFactory;
  public InteractionPrompt InteractionPromptUI => interactionPromtUI;

  public ListOfTmpSpriteAssets ListOfTmpSpriteAssets => listOfTmpSpriteAssets;
  public BuildPoolsController BuildPoolsController => buildPoolsController;
  public BuildingsDataController BuildingsDataController => buildingsDataController;
  public DropZombieData DropZombieData => dropZombieData;
  public SiegeManager SiegeManager => siegeManager;
  public ModifiersDatabaseObject ModifiersDatabase => modifiersDatabase;

  public RespawnManager RespawnManager => respawnManager;
  public GameStage GameStage => gameStage;
  public ActorsPooler ActorsPooler => actorsPooler;
  public DynamicCrosshair DynamicCrosshair => dynamicCrosshair;
  public BulletsPool BulletsPool => bulletsPool;
  public Locator Locator => locator;
  public QuestManager QuestManager => questManager;

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

  private bool paused;
  public bool Paused => paused;
  public event Action OnGamePaused;
  public event Action OnGameResumed;

  public void PauseGame() {
    paused = true;
    OnGamePaused?.Invoke();
  }

  public void ResumeGame() {
    paused = false;
    OnGameResumed?.Invoke();
  }

  public void SetGameStage(GameStage stage) {
    gameStage = stage;
  }

  public bool InitScriptsOnStart() {
    return !showMenuOnStart;
  }

  protected override void Awake() {
    base.Awake();

    DOTween.Init();
    UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
    statModifierFactory = new StatModifierFactory();
  }

  private void Start() {
    audioController.PlayMainTheme();
    if (!showMenuOnStart) {
      menuController.Hide();
      return;
    }

    menuController.Show();
  }

  public void EnableUIElements(bool state) {
    foreach (var obj in uiObjectList) {
      obj.SetActive(state);
    }

    Locator.ShowHide(state);
  }

  //use for start cutscene fall to ground
  private void EnableAllInput(bool state) {
    userInput.EnableGamePlayControls(state);
    userInput.EnableUIControls(state);
  }

  //lock only gameplay controls
  public void EnableGamePlayInput(bool state) {
    userInput.EnableGamePlayControls(state);
  }

  //lock all input when we start new game
  public void LockOnNewGame() {
    EnableAllInput(false);
    playerController.GroundedChanged += ChangeGround;
  }

  private void ChangeGround(bool arg1, float arg2) {
    playerController.GroundedChanged -= ChangeGround;

    EnableAllInput(true);
    playerController.SetLockHighlight(false);
    EnableUIElements(true);
  }
}