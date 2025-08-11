using System.Collections.Generic;
using SaveSystem;
using Scriptables.Items;
using UI.Objectives;
using UnityEngine;
using UnityEngine.Rendering;

namespace Objectives {
  public class ObjectivesSystem : MonoBehaviour, ISaveLoad {
    [SerializeField] private List<Block> blocks = new();

    private readonly List<ObjectivesManager> managers = new();

    private void Awake() {
      SaveLoadSystem.Instance.Register(this);
    }

    public bool HasActiveGroups() {
      foreach (var manager in managers) {
        if (manager.GetCurrentGroup() != null) {
          return true;
        }
      }

      return false;
    }

    /*private void Start() {
      // Якщо це нова гра — ініціалізуємо без збереження
      if (SaveLoadSystem.Instance.IsNewGame()) {
        Initialize(loadFromSave: false);
      }
    }*/

    private void Initialize(bool loadFromSave) {
      var data = SaveLoadSystem.Instance.gameData.Objectives;

      foreach (var block in blocks) {
        ObjectivesData managerData = null;

        if (loadFromSave && data != null && data.TryGetValue(block.GetConfigId(), out var loaded)) {
          managerData = loaded;
        }

        var manager = block.Init(managerData);
        AddManager(manager);
      }
    }

    private void AddManager(ObjectivesManager manager) {
      managers.Add(manager);
    }

    private void Report(ObjectiveTaskType type, object context) {
      foreach (var manager in managers) {
        manager.Report(type, context);
      }
    }

    public void ReportBuild(BuildingBlock block, int amount) {
      Report(ObjectiveTaskType.Build, (block, amount));
    }

    public void ReportCraft(ItemObject item, int amount) {
      Report(ObjectiveTaskType.CraftItem, (item, amount));
    }

    public void ReportPickup(ItemObject item, int amount) {
      Report(ObjectiveTaskType.PickupItem, (item, amount));
    }

    public void ReportItemUse(ItemObject item, int amount) {
      Report(ObjectiveTaskType.ItemUse, (item, amount));
    }

    public void ReportItemEquip(ItemObject item, int amount) {
      Report(ObjectiveTaskType.ItemEquip, (item, amount));
    }

    public void ReportItemRepair(ItemObject item, int amount) {
      Report(ObjectiveTaskType.ItemRepair, (item, amount));
    }

    public void ReportRobotRepair(bool success) {
      if (success) {
        Report(ObjectiveTaskType.RobotRepair, null);
      }
    }

    public void ReportSurviveSiege(bool survived) {
      if (survived) {
        Report(ObjectiveTaskType.SurviveSiege, null);
      }
    }

    #region Save/Load

    public int Priority => LoadPriority.OBJECTIVES;

    public void Save() {
      if (managers.Count == 0) {
        return;
      }

      SaveLoadSystem.Instance.gameData.Objectives = new SerializedDictionary<string, ObjectivesData>();
      foreach (var manager in managers) {
        SaveLoadSystem.Instance.gameData.Objectives.Add(manager.GetConfigId(), manager.GetSaveData());
      }
    }

    public void Load() {
      Initialize(loadFromSave: true);
    }

    public void Clear() {
      SaveLoadSystem.Instance.gameData.Objectives = new SerializedDictionary<string, ObjectivesData>();
    }

    #endregion
  }
}