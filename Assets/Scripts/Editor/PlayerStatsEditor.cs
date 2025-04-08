#if UNITY_EDITOR
using Actors;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerStats), true)]
public class PlayerStatsEditor : Editor {
  private int[] values = new[] { 20, 50, 100 };
  private PlayerStats stats;
  private ActorBase actor;

  public override void OnInspectorGUI() {
    DrawDefaultInspector();

    if (!Application.isPlaying) {
      return;
    }

    stats = (PlayerStats)target;
    actor = target.GetComponent<ActorBase>();

    // Display the stats in the inspector
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
    EditorGUILayout.LabelField($"Health: {stats.Health}", EditorStyles.label);
    EditorGUILayout.LabelField($"Max Health: {stats.MaxHealth}", EditorStyles.label);
    EditorGUILayout.LabelField($"Health Regen: {stats.HealthRegen}", EditorStyles.label);
    EditorGUILayout.LabelField($"Stamina: {stats.Stamina}", EditorStyles.label);
    EditorGUILayout.LabelField($"Max Stamina: {stats.MaxStamina}", EditorStyles.label);
    EditorGUILayout.LabelField($"Stamina Drain: {stats.StaminaDrain}", EditorStyles.label);
    EditorGUILayout.LabelField($"Stamina Recovery: {stats.StaminaRecovery}",
      EditorStyles.label);
    EditorGUILayout.LabelField($"Attack Range: {stats.AttackRange}", EditorStyles.label);
    EditorGUILayout.LabelField($"Block Damage: {stats.BlockDamage}", EditorStyles.label);
    EditorGUILayout.LabelField($"Entity Damage: {stats.EntityDamage}", EditorStyles.label);
    EditorGUILayout.LabelField($"Time Between Attacks: {stats.TimeBtwAttacks}",
      EditorStyles.label);
    EditorGUILayout.LabelField($"Attack Stamina Usage: {stats.AttackStaminaUsage}",
      EditorStyles.label);
    EditorGUILayout.LabelField($"Armor: {stats.Armor}", EditorStyles.label);

    foreach (var val in values) {
      if (GUILayout.Button($"Remove {val} hp")) {
        stats.AddHealth(-val);
      }
    }

    foreach (var val in values) {
      if (GUILayout.Button($"Add {val} hp")) {
        stats.AddHealth(val);
      }
    }

    if (actor == null) {
      return;
    }

    foreach (var val in values) {
      if (GUILayout.Button($"Actor take {val} damage")) {
        actor.Damage(val);
      }
    }
  }
}
#endif