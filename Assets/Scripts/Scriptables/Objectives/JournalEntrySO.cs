using UnityEngine;

namespace Scriptables.Objectives {
  [CreateAssetMenu(
    fileName = "JournalEntry",
    menuName = "Journal/New Entry",
    order = 0)]
  public class JournalEntrySO : BaseScriptableObject {
    [Header("Identification")] public int uniqueID;

    [Header("Content")] public string entryName;
    [TextArea(3, 10)] public string subName;
    [TextArea(3, 10)] public string description;
    

    [Header("Visual")] public Sprite imageRef;
  }
}