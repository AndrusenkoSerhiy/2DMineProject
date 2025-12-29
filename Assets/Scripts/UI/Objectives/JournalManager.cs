using System;
using System.Collections.Generic;
using Scriptables.Objectives;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Objectives {
  public struct JournalEntrySave {
    public int id;
    public bool isOpened;
    public bool isSeen;
  }

  public class JournalManager : MonoBehaviour {
    private JournalEntrySave[] journalEntriesSaves;
    [SerializeField] private List<JournalEntryUI> journalEntriesUI;
    public Image MainImage;
    public TMP_Text SubText;
    public TMP_Text MainText;
    public bool isInited = false;

    public void Awake() {
      isInited = false;
    }

    public void OnEnable() {
      journalEntriesSaves = new JournalEntrySave[journalEntriesUI.Count];
      InitNewSave();
      ShowEntries();
    }

    public void OnDisable() {
      HideEntries();
    }

    private void InitNewSave() {
      if (isInited) return;
      //new save
      for (var i = 0; i < journalEntriesUI.Count; i++) {
        journalEntriesSaves[i] = new JournalEntrySave() {
          id = journalEntriesUI[i].data.uniqueID,
          isOpened = false,
          isSeen = false
        };
      }

      //todo init from save
      isInited = true;
    }

    private JournalEntrySave GetEntrySave(int id) {
      for (var i = 0; i < journalEntriesSaves.Length; i++) {
        if (journalEntriesSaves[i].id == id) return journalEntriesSaves[i];
      }

      return new JournalEntrySave();
    }

    public void UnlockEntry(int id) {
      var entry = GetEntrySave(id);
      if (entry.id == 0) return;

      entry.isOpened = true;
      entry.isSeen = false;
      Save();
    }

    public void MarkAsSeen(int id) {
      var entry = GetEntrySave(id);
      if (entry.id == 0) return;
      entry.isSeen = true;
      Save();
    }

    public bool IsEntryUnlocked(int id) {
      var entry = GetEntrySave(id);
      return entry.id != 0 && entry.isOpened;
    } 
    public bool IsEntrySeen(int id) {
      var entry = GetEntrySave(id);
      return entry.id != 0 && entry.isSeen;
    }

    private void Save() {
      //todo make save to file
    }


    public void SetSelected(int index) {
      JournalEntryUI target = null;
      for (var i = 0; i < journalEntriesUI.Count; i++) {
        if (i != index)
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