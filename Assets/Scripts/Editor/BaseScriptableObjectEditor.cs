using Scriptables;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseScriptableObject), true)]
public class BaseScriptableObjectEditor : Editor {
  public override void OnInspectorGUI() {
    var scriptable = (BaseScriptableObject)target;

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.LabelField("Id", scriptable.Id, EditorStyles.textField);

    if (GUILayout.Button("Copy", GUILayout.Width(50))) {
      EditorGUIUtility.systemCopyBuffer = scriptable.Id;
    }

    EditorGUILayout.EndHorizontal();

    if (GUILayout.Button("Regenerate Id")) {
      Undo.RecordObject(scriptable, "Regenerate Id");
      scriptable.RegenerateId();
      EditorUtility.SetDirty(scriptable);
    }

    DrawDefaultInspector();
  }
}