using System;
using System.Collections.Generic;
using Scriptables.Items;
using Scriptables.Objectives;
using UnityEngine;

namespace Objectives {
  public class ObjectivesManager : MonoBehaviour {
    [SerializeField] private ObjectiveGroupScriptableObject startGroup;

    private ObjectiveGroupScriptableObject currentGroup;
    private HashSet<string> completedObjectives = new();
    private GameManager gameManager; // заміни на свій спосіб доступу

    public static ObjectivesManager Instance { get; private set; }

    private void Awake() {
      if (Instance != null && Instance != this) {
        Destroy(gameObject);
        return;
      }

      Instance = this;
      DontDestroyOnLoad(gameObject);

      gameManager = GameManager.Instance;
      LoadProgress();
    }

    public ObjectiveGroupScriptableObject GetCurrentGroup() => currentGroup;

    private void Start() {
      if (currentGroup == null) {
        currentGroup = startGroup;
      }
    }

    public void CompleteObjective(string id) {
      if (currentGroup == null) return;

      var obj = currentGroup.objectives.Find(o => o.id == id);
      if (obj == null) return;
      if (completedObjectives.Contains(id)) return;

      completedObjectives.Add(id);
      GrantReward(obj.reward);

      Debug.Log($"Objective Completed: {obj.title}");

      if (IsCurrentGroupCompleted()) {
        Debug.Log("All objectives in group completed!");

        if (currentGroup.nextGroup != null) {
          currentGroup = currentGroup.nextGroup;
          Debug.Log("Next group activated: " + currentGroup.groupTitle);
        }
      }

      SaveProgress();
    }

    private void GrantReward(ObjectiveRewardData reward) {
      if (reward.item == null || reward.amount <= 0) return;

      var item = new Item(reward.item);

      gameManager.PlayerInventory.SpawnItem(item, reward.amount);
      gameManager.MessagesManager.ShowSimpleMessage("Received reward: " + reward.item.name);
    }

    private bool IsCurrentGroupCompleted() {
      foreach (var obj in currentGroup.objectives) {
        if (!completedObjectives.Contains(obj.id)) {
          return false;
        }
      }

      return true;
    }

    public bool IsObjectiveCompleted(string id) => completedObjectives.Contains(id);

    private void SaveProgress() {
      var key = "Objectives_Completed";
      var save = string.Join(",", completedObjectives);
      PlayerPrefs.SetString(key, save);
      PlayerPrefs.SetString("Objectives_CurrentGroup", currentGroup?.Id ?? "");
    }

    private void LoadProgress() {
      var key = "Objectives_Completed";
      if (PlayerPrefs.HasKey(key)) {
        var saved = PlayerPrefs.GetString(key);
        completedObjectives = new HashSet<string>(saved.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
      }

      var groupId = PlayerPrefs.GetString("Objectives_CurrentGroup", "");
      if (!string.IsNullOrEmpty(groupId)) {
        currentGroup = FindGroupById(groupId);
      }
      else {
        currentGroup = startGroup;
      }
    }

    private ObjectiveGroupScriptableObject FindGroupById(string id) {
      var allGroups = Resources.LoadAll<ObjectiveGroupScriptableObject>("");
      foreach (var g in allGroups) {
        if (g.Id == id)
          return g;
      }

      return null;
    }

    public void ResetProgress() {
      PlayerPrefs.DeleteKey("Objectives_Completed");
      PlayerPrefs.DeleteKey("Objectives_CurrentGroup");
      completedObjectives.Clear();
      currentGroup = startGroup;
    }
  }
}