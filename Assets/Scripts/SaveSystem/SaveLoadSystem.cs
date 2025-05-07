using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Utility;

namespace SaveSystem {
  [Serializable]
  public class GameData {
    public string FileName;
    public string Name;
    public bool DefaultItemAdded;
    public SerializedDictionary<string, InventoryData> Inventories;
    public SerializedDictionary<string, WorkstationsData> Workstations;
    public RecipesData Recipes;
    public SerializedDictionary<string, RobotData> Robots;
    public WorldData WorldData;
    public PlayerData PlayerData;
    public SiegeData SiegeData;
    public List<ZombiesData> Zombies;
  }

  public interface ISaveLoad {
    void Save();
    void Load();
  }

  [DefaultExecutionOrder(-10)]
  public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem> {
    [SerializeField] public GameData gameData;
    [SerializeField] private int maxSaveFiles = 3;

    [SerializeField] private bool saveOnQuit = true;
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool autosave = true;
    [SerializeField] private bool prettyPrint = true;

    [Tooltip("Time in seconds")] [SerializeField]
    private float autosaveInterval = 300f;

    private readonly List<ISaveLoad> saveables = new();
    private float autosaveTimer = 0f;
    private IDataService dataService;
    private bool isNewGame;

    public int MaxSaveFiles => maxSaveFiles;
    public bool IsNewGame => isNewGame;

    protected override void Awake() {
      base.Awake();
      dataService = new FileDataService(new JsonSerializer(prettyPrint));
      if (loadOnStart) {
        LoadOrCreateNew();
      }
      else {
        NewGame();
      }
    }

    private void OnApplicationQuit() {
      if (!saveOnQuit) {
        return;
      }

      SaveAllEntities();
      SaveGame();
    }

    //TODO refactor????
    private void Update() {
      if (!autosave) {
        return;
      }

      autosaveTimer += Time.deltaTime;
      if (autosaveTimer >= autosaveInterval) {
        Autosave();
        autosaveTimer = 0f;
      }
    }

    private void Autosave() {
      //Todo show message
      //Todo Async????
      Debug.Log("Autosaving game...");
      SaveAllEntities();
      SaveGame();
      Debug.Log("Game Saved");
    }

    private void SaveAllEntities() {
      // var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveLoad>();

      foreach (var saveable in saveables) {
        saveable.Save();
      }
    }

    private void LoadOrCreateNew() {
      try {
        LoadGame("Game");
        isNewGame = false;
      }
      catch (Exception e) {
        Debug.LogWarning(e);
        NewGame();
      }
    }

    public void Register(ISaveLoad obj) => saveables.Add(obj);
    public void Unregister(ISaveLoad obj) => saveables.Remove(obj);

    public void NewGame() {
      gameData = new GameData {
        // FileName = $"Game_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
        FileName = "Game",
        Name = $"Game {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
        DefaultItemAdded = false,
        Inventories = new SerializedDictionary<string, InventoryData>(),
        Workstations = new SerializedDictionary<string, WorkstationsData>(),
        Recipes = new RecipesData(),
        Robots = new SerializedDictionary<string, RobotData>(),
        WorldData = new WorldData(),
        PlayerData = new PlayerData(),
        SiegeData = new SiegeData(),
        Zombies = new List<ZombiesData>()
      };
      isNewGame = true;
    }

    public void SaveGame() => dataService.Save(gameData);
    public void LoadGame(string gameName) => gameData = dataService.Load(gameName);
    public void ReloadGame() => LoadGame(gameData.Name);
    public void DeleteGame(string gameName) => dataService.Delete(gameName);
    public IEnumerable<string> GetAllSaveFilePaths() => dataService.ListSaves();
  }
}