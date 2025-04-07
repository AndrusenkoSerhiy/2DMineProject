#if UNITY_EDITOR
using Player;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerControllerBase), true)]
public class PlayerControllerBaseEditor : Editor {
  private PlayerControllerBase playerController;

  public override void OnInspectorGUI() {
    DrawDefaultInspector();

    if (!Application.isPlaying) {
      return;
    }

    playerController = (PlayerControllerBase)target;

    // Display the stats in the inspector
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
    EditorGUILayout.LabelField($"Health: {playerController.PlayerStats.Health}", EditorStyles.label);
    EditorGUILayout.LabelField($"Max Health: {playerController.PlayerStats.MaxHealth}", EditorStyles.label);
    EditorGUILayout.LabelField($"Health Regen: {playerController.PlayerStats.HealthRegen}", EditorStyles.label);
    EditorGUILayout.LabelField($"Stamina: {playerController.PlayerStats.Stamina}", EditorStyles.label);
    EditorGUILayout.LabelField($"Max Stamina: {playerController.PlayerStats.MaxStamina}", EditorStyles.label);
    EditorGUILayout.LabelField($"Stamina Drain: {playerController.PlayerStats.StaminaDrain}", EditorStyles.label);
    EditorGUILayout.LabelField($"Stamina Recovery: {playerController.PlayerStats.StaminaRecovery}",
      EditorStyles.label);
    EditorGUILayout.LabelField($"Attack Range: {playerController.PlayerStats.AttackRange}", EditorStyles.label);
    EditorGUILayout.LabelField($"Block Damage: {playerController.PlayerStats.BlockDamage}", EditorStyles.label);
    EditorGUILayout.LabelField($"Entity Damage: {playerController.PlayerStats.EntityDamage}", EditorStyles.label);
    EditorGUILayout.LabelField($"Time Between Attacks: {playerController.PlayerStats.TimeBtwAttacks}",
      EditorStyles.label);
    EditorGUILayout.LabelField($"Attack Stamina Usage: {playerController.PlayerStats.AttackStaminaUsage}",
      EditorStyles.label);
    EditorGUILayout.LabelField($"Armor: {playerController.PlayerStats.Armor}", EditorStyles.label);

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
    playerController.PlayerStats.AddHealth(-20f);
  }

  // Button click simulation method for healing
  private void Heal() {
    playerController.PlayerStats.AddHealth(20f);
  }
}
#endif