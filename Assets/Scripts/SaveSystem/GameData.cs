using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaveSystem {
  [Serializable]
  public class GameData {
    public int ProfileId;
    public bool IsNew;
    public bool DefaultItemAdded;
    public List<string> WeightItems;
    public float Weight;
    public SerializedDictionary<string, InventoryData> Inventories;
    public SerializedDictionary<string, WorkstationsData> Workstations;
    public RecipesData Recipes;
    public SerializedDictionary<string, RobotData> Robots;
    public WorldData WorldData;
    public PlayerData PlayerData;
    public SiegeData SiegeData;
    public List<ZombiesData> Zombies;
    public List<LocatorPointData> LocatorPointsData;
    public QuestData QuestData;
    public SerializedDictionary<string, ObjectivesData> Objectives;
    public Vector3 RespawnPosition;
  }

  [Serializable]
  public class ProfilesData {
    public ProfileData CurrentProfile;
    public List<ProfileData> Profiles;
  }

  [Serializable]
  public class ProfileData {
    public int ProfileId;
    public string Name;
  }
}