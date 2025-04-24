#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerStats), true)]
public class PlayerStatsEditor : BaseStatsEditor {
  private int[] values = new[] { 20, 50, 100 };
  private PlayerStats stats;

  public override void OnInspectorGUI() {
    base.OnInspectorGUI();
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
  }
}
#endif