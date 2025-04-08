#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerStats), true)]
public class PlayerStatsEditor : Editor {
  private PlayerStats stats;

  public override void OnInspectorGUI() {
    DrawDefaultInspector();

    if (!Application.isPlaying) {
      return;
    }

    stats = (PlayerStats)target;

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

    // Add a button to simulate taking damage
    if (GUILayout.Button("Take Damage")) {
      TakeDamage();
    }

    // Add a button to simulate healing
    if (GUILayout.Button("Heal")) {
      Heal();
    }
  }

  // Button click simulation method for taking damage
  private void TakeDamage() {
    stats.AddHealth(-20f);
  }

  // Button click simulation method for healing
  private void Heal() {
    stats.AddHealth(20f);
  }
}
#endif