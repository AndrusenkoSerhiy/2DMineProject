#if UNITY_EDITOR
using Objectives;
using Scriptables.Items;
using Scriptables.Objectives;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectiveGroupScriptableObject))]
public class ObjectiveGroupEditor : Editor {
  public override void OnInspectorGUI() {
    var group = (ObjectiveGroupScriptableObject)target;

    EditorGUILayout.LabelField("Group Settings", EditorStyles.boldLabel);
    // group.Id = EditorGUILayout.TextField("Group ID", group.GroupId);
    group.groupTitle = EditorGUILayout.TextField("Group Title", group.groupTitle);
    group.nextGroup = (ObjectiveGroupScriptableObject)EditorGUILayout.ObjectField("Next Group", group.nextGroup,
      typeof(ObjectiveGroupScriptableObject), false);

    EditorGUILayout.Space(10);
    EditorGUILayout.LabelField("Objectives", EditorStyles.boldLabel);

    if (GUILayout.Button("Add Objective")) {
      group.objectives.Add(new ObjectiveData { id = System.Guid.NewGuid().ToString() });
    }

    for (var i = 0; i < group.objectives.Count; i++) {
      var obj = group.objectives[i];

      EditorGUILayout.BeginVertical("box");
      obj.title = EditorGUILayout.TextField("Title", obj.title);
      obj.type = (ObjectiveTaskType)EditorGUILayout.EnumPopup("Type", obj.type);
      obj.targetId = EditorGUILayout.TextField("Target ID", obj.targetId);
      obj.requiredCount = EditorGUILayout.IntField("Required Count", obj.requiredCount);

      EditorGUILayout.Space(4);
      obj.reward.item =
        (ItemObject)EditorGUILayout.ObjectField("Reward Item", obj.reward.item, typeof(ItemObject), false);
      obj.reward.amount = EditorGUILayout.IntField("Reward Amount", obj.reward.amount);

      EditorGUILayout.Space(4);
      if (GUILayout.Button("Remove Objective")) {
        group.objectives.RemoveAt(i);
        break;
      }

      EditorGUILayout.EndVertical();
    }

    if (GUI.changed) {
      EditorUtility.SetDirty(group);
    }
  }
}
#endif