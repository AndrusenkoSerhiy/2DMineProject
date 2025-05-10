namespace SaveSystem {
  public interface IDataService {
    public bool FileExists(string filename);
    public void Save<T>(T data, string filename, bool overwrite = true);
    public T Load<T>(string name);
    public void Delete(string name);
  }
}