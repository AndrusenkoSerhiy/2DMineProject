using Scriptables.Siege;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SiegesSettings))]
public class SiegesSettingsEditor : Editor {
  private SerializedProperty preSiegeNotifications;

  private void OnEnable() {
    preSiegeNotifications = serializedObject.FindProperty("PreSiegeNotifications");
  }

  public override void OnInspectorGUI() {
    serializedObject.Update();

    DrawDefaultInspector();

    EditorGUILayout.Space(10);
    EditorGUILayout.LabelField("⏰ Message before siege", EditorStyles.boldLabel);

    for (var i = 0; i < preSiegeNotifications.arraySize; i++) {
      var element = preSiegeNotifications.GetArrayElementAtIndex(i);
      EditorGUILayout.BeginVertical("box");
      EditorGUILayout.PropertyField(element.FindPropertyRelative("SecondsBeforeStart"),
        new GUIContent("Seconds Before Start"));
      EditorGUILayout.PropertyField(element.FindPropertyRelative("Message"), new GUIContent("Message"));

      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      if (GUILayout.Button("❌ Delete", GUILayout.Width(100))) {
        preSiegeNotifications.DeleteArrayElementAtIndex(i);
        break;
      }

      EditorGUILayout.EndHorizontal();
      EditorGUILayout.EndVertical();
    }

    EditorGUILayout.Space();
    if (GUILayout.Button("➕ Add Notification")) {
      preSiegeNotifications.arraySize++;
    }

    serializedObject.ApplyModifiedProperties();
  }
}