namespace SaveSystem {
  public interface ISaveLoad {
    int Priority { get; }
    void Save();
    void Load();
    void Clear();
  }
}