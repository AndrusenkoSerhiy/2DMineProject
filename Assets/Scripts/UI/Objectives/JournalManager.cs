using System;
using System.Collections.Generic;
using SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI.Objectives {
  [Serializable]
  public class JournalEntrySave {
    public int id = -1;
    public bool isOpened = false;
    public bool isSeen = false;
  }

  public class JournalManager : MonoBehaviour {
    public static JournalManager Instance { get; private set; }
    private List<JournalEntrySave> journalEntriesSaves = new();
    [SerializeField] private List<JournalEntryUI> journalEntriesUI;
    public Image MainImage;
    public TMP_Text SubText;
    public TMP_Text MainText;
    public bool isInited = false;

    public event Action OnEntrySeen;

    public void Awake() {
      if (Instance != null && Instance != this) {
        Destroy(gameObject);
        return;
      }

      Instance = this;
      JournalIcon.Instance.Init();
    }

    public void OnEnable() {
      if (!CheckSaved()) InitNewSave();
      ShowEntries();
    }

    public void OnDisable() {
      HideEntries();
    }

    private bool CheckSaved() {
      var savesData = SaveLoadSystem.Instance.gameData.JournalEntrySaves;
      if (savesData != null && savesData.Count != 0) {
        journalEntriesSaves = new List<JournalEntrySave>(savesData);
        return true;
      }

      return false;
    }

    private void InitNewSave() {
      if (isInited) return;
      journalEntriesSaves = new List<JournalEntrySave>();
      //new save
      for (var i = 0; i < journalEntriesUI.Count; i++) {
        journalEntriesSaves.Add(new JournalEntrySave() {
          id = journalEntriesUI[i].data.uniqueID,
          isOpened = false,
          isSeen = false
        });
      }

      UnlockEntry(1);
      UnlockEntry(2);
      //todo init from save
      isInited = true;
    }

    private JournalEntrySave GetEntrySave(int id) {
      for (var i = 0; i < journalEntriesSaves.Count; i++) {
        if (journalEntriesSaves[i].id == id) return journalEntriesSaves[i];
      }

      return new JournalEntrySave();
    }

    public void UnlockEntry(int id) {
      var entry = GetEntrySave(id);
      if (entry.id == -1) return;
      if (IsEntryUnlocked(id)) return;
      entry.isOpened = true;
      entry.isSeen = false;
      OnEntrySeen?.Invoke();
      Save();
    }

    public void MarkAsSeen(int id) {
      var entry = GetEntrySave(id);
      if (entry.id == -1) return;
      entry.isSeen = true;
      OnEntrySeen?.Invoke();
      Save();
    }

    public bool IsEntryUnlocked(int id) {
      var entry = GetEntrySave(id);
      return entry.id != -1 && entry.isOpened;
    }

    public bool IsEntrySeen(int id) {
      var entry = GetEntrySave(id);
      return entry.id != -1 && entry.isSeen;
    }

    public bool HasUnseen() {
      for (var i = 0; i < journalEntriesSaves.Count; i++) {
        if (journalEntriesSaves[i].isOpened && !journalEntriesSaves[i].isSeen) return true;
      }

      return false;
    }

    private void Save() {
      //todo make save to file
      SaveLoadSystem.Instance.gameData.JournalEntrySaves = new List<JournalEntrySave>(journalEntriesSaves);
    }


    public void SetSelected(int index) {
      JournalEntryUI target = null;
      for (var i = 0; i < journalEntriesUI.Count; i++) {
        if (i != index - 1)
          journalEntriesUI[i].DeSelect();
        else {
          target = journalEntriesUI[i];
        }
      }

      if (target == null) {
        Debug.LogError("Null entry : " + index);
      }

      MainImage.gameObject.SetActive(true);
      MainImage.sprite = target.data.imageRef;
      SubText.SetText(target.data.subName);
      MainText.SetText(target.data.description);
      MarkAsSeen(index);
    }

    public void ShowEntries() {
      for (var i = 0; i < journalEntriesUI.Count; i++) {
        if (IsEntryUnlocked(journalEntriesUI[i].data.uniqueID))
          journalEntriesUI[i].gameObject.SetActive(true);
        if (!IsEntrySeen(journalEntriesUI[i].data.uniqueID)) {
          journalEntriesUI[i].SetNew(true);
        }
      }

      journalEntriesUI[0].Select();
    }

    public void HideEntries() {
      MainImage.gameObject.SetActive(false);
      MainImage.sprite = null;
      SubText.SetText(string.Empty);
      MainText.SetText(string.Empty);
      for (var i = 0; i < journalEntriesUI.Count; i++) {
        journalEntriesUI[i].gameObject.SetActive(false);
      }
    }
  }
}