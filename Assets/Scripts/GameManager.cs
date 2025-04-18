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
using ScriptableObjects;
using Settings;
using Stats;
using UI;
using UnityEngine.Serialization;
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
  //TODO 
  //robot don't need this param in own script
  [SerializeField] private LadderMovement playerLadderMovement;
  public bool showMenuOnStart = true;
  [FormerlySerializedAs("objectList")]
  [Tooltip("UI object that need to be disabled when we start from menu")]
  [SerializeField] private List<GameObject> uiObjectList;
  
  private PlayerController playerController;
  private PlayerControllerBase currPlayerController;
  private MiningRobotController miningRobotController;
  private GameStage gameStage = GameStage.MainMenu;
  private IStatModifierFactory statModifierFactory;

  public StartGameCameraController StartGameCameraController => startGameCameraController;
  public MainMenu MainMenu => mainMenu;
  public InGameMenu InGameMenu => inGameMenu;
  public UserInput UserInput => userInput;
  public AudioManager AudioManager => audioManager;
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
  

  public RespawnManager RespawnManager => respawnManager;
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
  
  protected override void Awake() {
    base.Awake();

    DOTween.Init();
    UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;

    statModifierFactory = new StatModifierFactory();
  }

  private void Start() {
    if (!showMenuOnStart) {
      mainMenu.Hide();
      return;
    }
    
    EnableUIElements(false);
    startGameCameraController.Init();
    mainMenu.Show();
    EnableInput(false);
    //need to subscribe for player grounded first time
    playerController.GroundedChanged += ChangeGround;
  }

  private void EnableUIElements(bool state) {
    foreach (var obj in uiObjectList) {
      obj.SetActive(state);
    }
  }

  //use for start cutscene fall to ground
  private void EnableInput(bool state) {
    userInput.EnableGamePlayControls(state);
    userInput.EnableUIControls(state);

    playerController.SetLockHighlight(true);
  }

  private void ChangeGround(bool arg1, float arg2) {
    playerController.GroundedChanged -= ChangeGround;
    
    EnableInput(true);
    playerController.SetLockHighlight(false);
    EnableUIElements(true);
  }

  public void StartNewGame() {
    mainMenu.Hide();
    gameStage = GameStage.Game;
    startGameCameraController.Play();
  }

  public void ExitGame() {
    if (Application.isEditor) {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#endif
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