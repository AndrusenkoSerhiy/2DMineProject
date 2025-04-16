namespace Scriptables.Items {
  public enum DurabilityUsageType {
    Always,
    OnHit
  }

  public interface IDurableItem {
    public float MaxDurability { get; }
    public float DurabilityUse { get; }
    public DurabilityUsageType DurabilityUsageType { get; }
  }
}