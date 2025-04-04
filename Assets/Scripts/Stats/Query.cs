namespace Stats {
  public class Query {
    public readonly StatType StatType;
    public float Value;

    public Query(StatType statType, float value) {
      StatType = statType;
      Value = value;
    }
  }
}