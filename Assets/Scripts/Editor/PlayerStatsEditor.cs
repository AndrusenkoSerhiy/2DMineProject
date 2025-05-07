#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerStats), true)]
public class PlayerStatsEditor : BaseStatsEditor {
  private PlayerStats playerStats;

  public override void OnInspectorGUI() {
    base.OnInspectorGUI();
    if (!Application.isPlaying) {
      return;
    }
    
    playerStats = (PlayerStats)target;
    
    // Display the stats in the inspector
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
    EditorGUILayout.LabelField($"Health: {playerStats.Health}", EditorStyles.label);
    EditorGUILayout.LabelField($"Max Health: {playerStats.MaxHealth}", EditorStyles.label);
    EditorGUILayout.LabelField($"Health Regen: {playerStats.HealthRegen}", EditorStyles.label);
    EditorGUILayout.LabelField($"Stamina: {playerStats.Stamina}", EditorStyles.label);
    EditorGUILayout.LabelField($"Max Stamina: {playerStats.MaxStamina}", EditorStyles.label);
    EditorGUILayout.LabelField($"Stamina Drain: {playerStats.StaminaDrain}", EditorStyles.label);
    EditorGUILayout.LabelField($"Stamina Recovery: {playerStats.StaminaRecovery}",
      EditorStyles.label);
    EditorGUILayout.LabelField($"Attack Range: {playerStats.AttackRange}", EditorStyles.label);
    EditorGUILayout.LabelField($"Block Damage: {playerStats.BlockDamage}", EditorStyles.label);
    EditorGUILayout.LabelField($"Entity Damage: {playerStats.EntityDamage}", EditorStyles.label);
    EditorGUILayout.LabelField($"Time Between Attacks: {playerStats.TimeBtwAttacks}",
      EditorStyles.label);
    EditorGUILayout.LabelField($"Attack Stamina Usage: {playerStats.AttackStaminaUsage}",
      EditorStyles.label);
    EditorGUILayout.LabelField($"Armor: {playerStats.Armor}", EditorStyles.label);
    EditorGUILayout.LabelField($"MaxSpeed: {playerStats.MaxSpeed}", EditorStyles.label);
    EditorGUILayout.LabelField($"MaxBackSpeed: {playerStats.MaxBackSpeed}", EditorStyles.label);
    EditorGUILayout.LabelField($"SprintSpeed: {playerStats.SprintSpeed}", EditorStyles.label);
  }
}
#endif