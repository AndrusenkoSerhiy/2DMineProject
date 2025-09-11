#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Objectives;
using Objectives.Data;
using Scriptables.Objectives;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectivesConfig))]
public class ObjectivesConfigEditor : Editor {
  private ObjectivesConfig config;

  private GUIStyle groupBoxStyle;
  private GUIStyle headerStyle;
  private GUIStyle subHeaderStyle;

  private void OnEnable() {
    config = (ObjectivesConfig)target;

    groupBoxStyle = new GUIStyle("box") {
      padding = new RectOffset(10, 10, 10, 10),
      margin = new RectOffset(0, 0, 10, 10),
    };

    headerStyle = new GUIStyle(EditorStyles.boldLabel) {
      fontSize = 14
    };

    subHeaderStyle = new GUIStyle(EditorStyles.label) {
      fontStyle = FontStyle.Bold
    };
  }

  public override void OnInspectorGUI() {
    serializedObject.Update();

    var groupIds = new HashSet<string>();
    var objectiveIds = new HashSet<string>();

    EditorGUILayout.LabelField("Objectives Config", headerStyle);
    EditorGUILayout.Space(10);
    
    config.id = EditorGUILayout.TextField("ID", config.id);

    if (GUILayout.Button("Generate Config ID")) {
      config.id = Guid.NewGuid().ToString();
      GUI.FocusControl(null);
    }
    
    EditorGUILayout.Space(5);
    
    config.titleIcon = (Sprite)EditorGUILayout.ObjectField("Title Icon", config.titleIcon, typeof(Sprite), false);
    config.titleColor = EditorGUILayout.ColorField("Title Color", config.titleColor);

    EditorGUILayout.Space(5);

    config.taskIconIncomplete = (Sprite)EditorGUILayout.ObjectField("Task Icon Incomplete", config.taskIconIncomplete, typeof(Sprite), false);
    config.taskIconCompleted = (Sprite)EditorGUILayout.ObjectField("Task Icon Completed", config.taskIconCompleted, typeof(Sprite), false);

    config.taskColorIncomplete = EditorGUILayout.ColorField("Task Color Incomplete", config.taskColorIncomplete);
    config.taskColorCompleted = EditorGUILayout.ColorField("Task Color Completed", config.taskColorCompleted);

    for (int i = 0; i < config.groups.Count; i++) {
      var group = config.groups[i];

      EditorGUILayout.BeginVertical(groupBoxStyle);
      GUI.backgroundColor = new Color(0.85f, 0.9f, 1f);

      EditorGUILayout.LabelField($"Group {i + 1}", headerStyle);

      group.groupTitle = EditorGUILayout.TextField("Title", group.groupTitle);
      group.id = EditorGUILayout.TextField("ID", group.id);
      group.questId = EditorGUILayout.IntField("QuestID", group.questId);

      if (GUILayout.Button("Generate Group ID")) {
        group.id = Guid.NewGuid().ToString();
        GUI.FocusControl(null);
      }

      if (!string.IsNullOrEmpty(group.id) && !groupIds.Add(group.id)) {
        EditorGUILayout.HelpBox("Duplicate group ID!", MessageType.Error);
      }

      EditorGUILayout.Space(5);
      
      // === Group Reward ===
      var groupsProp = serializedObject.FindProperty("groups");
      var groupProp = groupsProp.GetArrayElementAtIndex(i);
      var groupRewardProp = groupProp.FindPropertyRelative("reward");

      EditorGUILayout.PropertyField(groupRewardProp, new GUIContent("Group Reward"), true);
      EditorGUILayout.Space(10);

      for (int j = 0; j < group.objectives.Count; j++) {
        var obj = group.objectives[j];

        EditorGUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField($"Objective {j + 1}", subHeaderStyle);

        obj.title = EditorGUILayout.TextField("Title", obj.title);
        obj.id = EditorGUILayout.TextField("ID", obj.id);

        if (GUILayout.Button("Generate Objective ID")) {
          obj.id = Guid.NewGuid().ToString();
          GUI.FocusControl(null);
        }

        if (!string.IsNullOrEmpty(obj.id) && !objectiveIds.Add(obj.id)) {
          EditorGUILayout.HelpBox("Duplicate objective ID!", MessageType.Error);
        }

        // Serialized properties
        var objectivesProp = groupProp.FindPropertyRelative("objectives");
        var objectiveProp = objectivesProp.GetArrayElementAtIndex(j);
        var taskDataProp = objectiveProp.FindPropertyRelative("taskData");
        var rewardProp = objectiveProp.FindPropertyRelative("reward");

        GUILayout.Space(5);
        EditorGUILayout.PropertyField(taskDataProp, new GUIContent("Task Data"), true);

        GUILayout.Space(5);
        EditorGUILayout.PropertyField(rewardProp, new GUIContent("Reward"), true);

        GUILayout.Space(8);

        if (GUILayout.Button("Remove Objective")) {
          group.objectives.RemoveAt(j);
          break;
        }

        EditorGUILayout.EndVertical();
        GUILayout.Space(8);
      }

      GUILayout.Space(10);

      if (GUILayout.Button("Add Objective")) {
        group.objectives.Add(new ObjectiveData {
          id = Guid.NewGuid().ToString()
        });
      }

      GUILayout.Space(10);

      if (GUILayout.Button("Remove Group")) {
        config.groups.RemoveAt(i);
        break;
      }

      EditorGUILayout.EndVertical();
      GUILayout.Space(20);
    }

    GUILayout.Space(10);
    if (GUILayout.Button("Add Group")) {
      config.groups.Add(new ObjectiveGroup {
        id = Guid.NewGuid().ToString()
      });
    }

    serializedObject.ApplyModifiedProperties();
    EditorUtility.SetDirty(config);
  }
}
#endif