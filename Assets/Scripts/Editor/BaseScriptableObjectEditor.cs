using Scriptables;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseScriptableObject), true)]
public class BaseScriptableObjectEditor : Editor {
  public override void OnInspectorGUI() {
    var scriptable = (BaseScriptableObject)target;

    EditorGUILayout.LabelField("Id", scriptable.Id, EditorStyles.textField);

    if (GUILayout.Button("Regenerate Id")) {
      Undo.RecordObject(scriptable, "Regenerate Id");
      scriptable.RegenerateId();
      EditorUtility.SetDirty(scriptable);
    }

    DrawDefaultInspector();
  }
}