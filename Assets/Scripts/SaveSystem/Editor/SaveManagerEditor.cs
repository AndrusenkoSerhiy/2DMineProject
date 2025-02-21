using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {
  [CustomEditor(typeof(SaveLoadSystem))]
  public class SaveManagerEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() {
      var saveLoadSystem = (SaveLoadSystem)target;
      var gameName = saveLoadSystem.gameData.FileName;

      DrawDefaultInspector();

      if (GUILayout.Button("New Game")) {
        saveLoadSystem.NewGame();
      }

      if (GUILayout.Button("Save Game")) {
        saveLoadSystem.SaveGame();
      }

      if (GUILayout.Button("Load Game")) {
        saveLoadSystem.LoadGame(gameName);
      }

      if (GUILayout.Button("Delete Game")) {
        saveLoadSystem.DeleteGame(gameName);
      }
    }
  }
}