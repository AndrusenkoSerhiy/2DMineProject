using System.Collections.Generic;

namespace SaveSystem {
  public interface IDataService {
    public void Save(GameData data, bool overwrite = true);
    public GameData Load(string name);
    public void Delete(string name);
    void DeleteAll();
    public IEnumerable<string> ListSaves();
  }
}