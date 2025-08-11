#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Objectives.Data;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ObjectiveTaskData), true)]
public class ObjectiveTaskDataDrawer : PropertyDrawer {
  private static readonly Type baseType = typeof(ObjectiveTaskData);

  private static readonly List<Type> derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => a.GetTypes())
    .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract)
    .ToList();

  private static readonly string[] typeNames = derivedTypes
    .Select(t => t.Name.Replace("TaskData", ""))
    .ToArray();

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    EditorGUI.BeginProperty(position, label, property);

    if (property.managedReferenceValue == null) {
      Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
      int selectedIndex = EditorGUI.Popup(dropdownRect, "Task Type", -1, typeNames);
      if (selectedIndex >= 0 && selectedIndex < derivedTypes.Count) {
        var instance = Activator.CreateInstance(derivedTypes[selectedIndex]);
        property.managedReferenceValue = instance;
        property.serializedObject.ApplyModifiedProperties();
      }

      EditorGUI.EndProperty();
      return;
    }

    // Draw type dropdown
    Type currentType = property.managedReferenceValue.GetType();
    int currentIndex = derivedTypes.IndexOf(currentType);
    Rect popupRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
    int newIndex = EditorGUI.Popup(popupRect, "Task Type", currentIndex, typeNames);

    if (newIndex != currentIndex && newIndex >= 0) {
      var newInstance = Activator.CreateInstance(derivedTypes[newIndex]);
      property.managedReferenceValue = newInstance;
      property.serializedObject.ApplyModifiedProperties();
      EditorGUI.EndProperty();
      return;
    }

    // Draw internal fields
    EditorGUI.indentLevel++;
    var iterator = property.Copy();
    var endProperty = iterator.GetEndProperty();

    float yOffset = popupRect.y + EditorGUIUtility.singleLineHeight + 2;

    iterator.NextVisible(true);
    while (!SerializedProperty.EqualContents(iterator, endProperty)) {
      float height = EditorGUI.GetPropertyHeight(iterator, true);
      Rect fieldRect = new Rect(position.x, yOffset, position.width, height);
      EditorGUI.PropertyField(fieldRect, iterator, true);
      yOffset += height + 2;
      if (!iterator.NextVisible(false)) break;
    }

    EditorGUI.indentLevel--;

    EditorGUI.EndProperty();
  }

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    float height = EditorGUIUtility.singleLineHeight + 4;

    if (property.managedReferenceValue == null) {
      return height;
    }

    SerializedProperty iterator = property.Copy();
    var endProperty = iterator.GetEndProperty();

    iterator.NextVisible(true);
    while (!SerializedProperty.EqualContents(iterator, endProperty)) {
      height += EditorGUI.GetPropertyHeight(iterator, true) + 2;
      if (!iterator.NextVisible(false)) break;
    }

    return height;
  }
}
#endif