using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using UnityEngine;
using UnityEngine.Rendering;
using Utility;

namespace SaveSystem {
  [Serializable]
  public class GameData {
    public string FileName;
    public string Name;
    public SerializedDictionary<string, InventoryData> Inventories;
  }

  public interface ISaveable {
    string Id { get; }
  }

  public interface ISaveLoad : ISaveable {
    void Save();
    void Load();
  }

  [DefaultExecutionOrder(-1)]
  public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem> {
    [SerializeField] public GameData gameData;
    [SerializeField] private int maxSaveFiles = 3;
    private IDataService dataService;

    public int MaxSaveFiles => maxSaveFiles;

    protected override void Awake() {
      base.Awake();
      // Debug.Log("SaveLoadSystem Awake()");
      dataService = new FileDataService(new JsonSerializer());
      LoadOrCreateNew();
    }

    private void OnApplicationQuit() {
      SaveAllEntities();
      // Debug.Log("OnApplicationQuit SaveGame");
      SaveGame();
    }

    private void SaveAllEntities() {
      var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveLoad>();

      foreach (var saveable in saveables) {
        saveable.Save();
      }
    }

    private void LoadOrCreateNew() {
      try {
        LoadGame("Game");
      }
      catch (Exception e) {
        Debug.LogWarning(e);
        NewGame();
      }
    }

    public void NewGame() {
      gameData = new GameData {
        // FileName = $"Game_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
        FileName = "Game",
        Name = $"Game {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
        Inventories = new SerializedDictionary<string, InventoryData>()
      };
    }

    public void SaveGame() => dataService.Save(gameData);
    public void LoadGame(string gameName) => gameData = dataService.Load(gameName);
    public void ReloadGame() => LoadGame(gameData.Name);
    public void DeleteGame(string gameName) => dataService.Delete(gameName);
    public IEnumerable<string> GetAllSaveFilePaths() => dataService.ListSaves();
  }
}