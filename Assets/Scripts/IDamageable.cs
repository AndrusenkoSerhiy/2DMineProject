using Scriptables;

public enum DamageableType {
  Default = 0,
  Player = 10,
  Robot = 20,
  Enemy = 30,
  Cell = 40,
};

public interface IDamageable {
  public DamageableType DamageableType { get; set; }
  public AudioData OnTakeDamageAudioData { get; set; }
  public bool hasTakenDamage { get; set; }

  public void Damage(float damage, bool isPlayer);

  public float GetHealth();

  public float GetMaxHealth();

  public void AfterDamageReceived();

  public void DestroyObject();
}