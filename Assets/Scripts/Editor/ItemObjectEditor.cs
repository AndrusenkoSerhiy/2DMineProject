using Scriptables.Items;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemObject), true)]
public class ItemObjectEditor : Editor {
  public override void OnInspectorGUI() {
    // Reference to the target ScriptableObject
    var scriptable = (ItemObject)target;

    // Display GUID (Read-Only)
    EditorGUILayout.LabelField("Id", scriptable.Id, EditorStyles.textField);

    // Button to regenerate GUID
    if (GUILayout.Button("Regenerate Id")) {
      Undo.RecordObject(scriptable, "Regenerate Id");
      typeof(ItemObject)
        .GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.SetValue(scriptable, System.Guid.NewGuid().ToString());

      EditorUtility.SetDirty(scriptable);
    }

    DrawDefaultInspector();
  }
}