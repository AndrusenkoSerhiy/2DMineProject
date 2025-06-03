using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Utility;

namespace SaveSystem {
  public static class LoadPriority {
    public const int BUILDINGS = 0;
    public const int CHUNK = 10;
    public const int INVENTORIES = 20;
    public const int INVENTORIES_DISPLAY = 22;
    public const int QUICK_SLOTS = 24;
    public const int CRAFT = 30;
    public const int CRAFT_WINDOWS = 35;
    public const int RECIPES = 40;
    public const int QUESTS = 45;
    public const int PLAYER_CONTROLLER = 50;
    public const int EQUIPMENT = 55;
    public const int ACTORS = 60;
    public const int SIEGE = 70;
    public const int ENEMIES = 80;
    public const int LOCATOR = 90;
  }

  [DefaultExecutionOrder(-10)]
  public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem> {
    [SerializeField] private bool saveOnQuit = true;
    [SerializeField] private bool prettyPrint = true;

    [SerializeField] private bool autosave = true;

    [Tooltip("Time in seconds")] [SerializeField]
    private float autosaveInterval = 300f;

    [SerializeField] private string saveFileNamePrefix = "game.";
    [SerializeField] private string profilesFileName = "profiles";

    [NonSerialized] public GameData gameData;
    [NonSerialized] public ProfilesData profilesData;

    private readonly SortedList<int, List<ISaveLoad>> saveables = new();
    private float autosaveTimer = 0f;
    private IDataService dataService;
    private int profileLoaded;
    private bool needToSave = false;

    private int profilesCount = 3;

    protected override void Awake() {
      base.Awake();
      dataService = new FileDataService(new JsonSerializer(prettyPrint));
      LoadMetaData();
      ResetGameData();
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

    private void OnApplicationQuit() {
      Save();
    }

    #region Profiles

    private void LoadMetaData() {
      try {
        profilesData = dataService.Load<ProfilesData>(profilesFileName);
      }
      catch (Exception e) {
        profilesData = CreateNewProfilesData();
      }
    }

    private bool SaveMetaData() {
      try {
        dataService.Save(profilesData, profilesFileName);
      }
      catch (Exception e) {
        Debug.Log($"SaveMetaData e: {e.Message}");
        return false;
      }

      return true;
    }

    private ProfilesData CreateNewProfilesData() {
      var profiles = new List<ProfileData>();
      for (var i = 0; i < profilesCount; i++) {
        profiles.Add(new ProfileData {
          ProfileId = i + 1,
          Name = string.Empty
        });
      }

      return new ProfilesData {
        CurrentProfile = new ProfileData {
          ProfileId = 0,
          Name = string.Empty
        },
        Profiles = profiles
      };
    }

    private string GetCurrentFileName() {
      return GetFileNameByProfileId(profilesData.CurrentProfile.ProfileId);
    }

    private string GetFileNameByProfileId(int profileId) {
      return $"{saveFileNamePrefix}{profileId}";
    }

    public bool IsProfileSet() {
      return profilesData.CurrentProfile.ProfileId > 0;
    }

    public bool IsNameExist(string newName) {
      if (string.IsNullOrEmpty(newName)) {
        return false;
      }

      foreach (var data in profilesData.Profiles) {
        if (data.Name == newName) {
          return true;
        }
      }

      return false;
    }

    public void SetCurrentProfile(int profileId, string profileName) {
      if (profileId < 1 || profileId > profilesCount) {
        throw new ArgumentOutOfRangeException(nameof(profileId), "Profile ID is out of range");
      }

      profilesData.CurrentProfile.ProfileId = profileId;
      profilesData.CurrentProfile.Name = profileName;
    }

    public void UpdateProfileName(int profileId, string profileName) {
      if (profileId < 1 || profileId > profilesCount) {
        throw new ArgumentOutOfRangeException(nameof(profileId), "Profile ID is out of range");
      }

      var profile = profilesData.Profiles.Find(p => p.ProfileId == profileId);
      if (profile == null) {
        throw new ArgumentException($"Profile with ID {profileId} not found");
      }

      profile.Name = profileName;
    }

    public bool DeleteGame(int profileId) {
      UpdateProfileName(profileId, string.Empty);
      if (profilesData.CurrentProfile.ProfileId == profileId) {
        profilesData.CurrentProfile.ProfileId = 0;
        profilesData.CurrentProfile.Name = string.Empty;
      }

      DeleteSaveFile(profileId);

      return true;
    }

    #endregion

    public void Register(ISaveLoad obj) {
      if (!saveables.ContainsKey(obj.Priority)) {
        saveables[obj.Priority] = new List<ISaveLoad>();
      }

      saveables[obj.Priority].Add(obj);
    }

    public void Unregister(ISaveLoad obj) {
      if (!saveables.ContainsKey(obj.Priority)) {
        return;
      }

      saveables[obj.Priority].Remove(obj);
    }

    private void SaveAllEntities() {
      foreach (var pair in saveables) {
        foreach (var saveable in pair.Value) {
          saveable.Save();
        }
      }
    }

    private void LoadAllEntities() {
      foreach (var pair in saveables) {
        foreach (var saveable in pair.Value) {
          saveable.Load();
        }
      }
    }

    private void ClearAllEntities() {
      foreach (var pair in saveables) {
        foreach (var saveable in pair.Value) {
          saveable.Clear();
        }
      }

      profileLoaded = 0;
    }

    public bool CanContinueGame() {
      if (!IsProfileSet()) {
        return false;
      }

      return dataService.FileExists(GetCurrentFileName())
             || !gameData.IsNew && gameData.ProfileId == profilesData.CurrentProfile.ProfileId;
    }

    public bool IsNewGame() {
      return gameData == null || gameData.IsNew;
    }

    private bool DataForCurrentProfileLoaded() {
      var currId = profilesData.CurrentProfile.ProfileId;
      return currId > 0 && currId == profileLoaded;
    }

    public void NewGame() {
      if (!IsProfileSet()) {
        throw new Exception("No profile set. Cannot create a new game.");
      }

      DeleteSaveFile(profilesData.CurrentProfile.ProfileId);
      ResetGameData();
      profileLoaded = profilesData.CurrentProfile.ProfileId;
      LoadAllEntities();
    }

    private void ResetGameData() {
      gameData = new GameData {
        ProfileId = profilesData.CurrentProfile.ProfileId,
        IsNew = true,
        DefaultItemAdded = false,
        Weight = 0f,
        WeightItems = new List<string>(),
        Inventories = new SerializedDictionary<string, InventoryData>(),
        Workstations = new SerializedDictionary<string, WorkstationsData>(),
        Recipes = new RecipesData(),
        QuestData = new QuestData(),
        Robots = new SerializedDictionary<string, RobotData>(),
        WorldData = new WorldData(),
        PlayerData = new PlayerData(),
        SiegeData = new SiegeData(),
        Zombies = new List<ZombiesData>(),
        LocatorPointsData = new List<LocatorPointData>(),
      };
    }

    public void Save() {
      if (!saveOnQuit) {
        return;
      }

      SaveMetaData();

      if (!DataForCurrentProfileLoaded()) {
        return;
      }

      if (!IsProfileSet()) {
        return;
      }

      SaveAllEntities();
      gameData.IsNew = false;
      SaveGame();
      ClearAllEntities();
    }

    private void Autosave() {
      //Todo show message
      //Todo Async????
      Debug.Log("Autosaving game...");
      Save();
      Debug.Log("Game Saved");
    }

    private bool SaveGame() {
      if (gameData == null) {
        Debug.LogError("GameData is null. Cannot save game.");
        return false;
      }

      try {
        dataService.Save(gameData, GetCurrentFileName());
      }
      catch (Exception e) {
        return false;
      }

      return true;
    }

    public bool LoadGame() {
      if (!IsProfileSet()) {
        Debug.LogError("No profile set. Cannot load game.");
        return false;
      }

      if (DataForCurrentProfileLoaded()) {
        return true;
      }

      try {
        gameData = dataService.Load<GameData>(GetCurrentFileName());
        profileLoaded = profilesData.CurrentProfile.ProfileId;
      }
      catch (Exception e) {
        return false;
      }

      LoadAllEntities();

      return true;
    }

    private void DeleteSaveFile(int profileId) {
      try {
        dataService.Delete(GetFileNameByProfileId(profileId));
      }
      catch (Exception e) {
        // ignored
      }
    }
  }
}