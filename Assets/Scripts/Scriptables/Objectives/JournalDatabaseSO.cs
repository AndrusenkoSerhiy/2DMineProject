using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.Objectives {
  [CreateAssetMenu(
    fileName = "JournalDatabase",
    menuName = "Journal/Database",
    order = 1)]
  public class JournalDatabaseSO : BaseScriptableObject {
    public List<JournalEntrySO> entries;

    public JournalEntrySO GetEntry(int index) {
      for (int i = 0; i < entries.Count; i++) {
        if (entries[i].uniqueID == index) {
          return entries[i];
        }
      }

      return null;
    }
  }
}