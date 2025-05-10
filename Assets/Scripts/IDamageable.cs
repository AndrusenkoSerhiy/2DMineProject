public interface IDamageable {
  public bool hasTakenDamage { get; set; }

  public void Damage(float damage, bool isPlayer);

  public float GetHealth();

  public float GetMaxHealth();

  public void AfterDamageReceived();

  public void DestroyObject();
}