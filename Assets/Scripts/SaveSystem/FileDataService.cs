using System;
using System.IO;
using UnityEngine;

namespace SaveSystem {
  public class FileDataService : IDataService {
    private ISerializer serializer;
    private string dataPath;
    private string fileExtension;

    public FileDataService(ISerializer serializer) {
      this.dataPath = Application.persistentDataPath;
      this.fileExtension = "json";
      this.serializer = serializer;
    }

    private string GetPathToFile(string fileName) {
      return Path.Combine(dataPath, string.Concat(fileName, ".", fileExtension));
    }

    public bool FileExists(string filename) {
      var fileLocation = GetPathToFile(filename);
      return File.Exists(fileLocation);
    }

    public void Save<T>(T data, string filename, bool overwrite = true) {
      var fileLocation = GetPathToFile(filename);
      var directoryPath = Path.GetDirectoryName(fileLocation);

      // Ensure the directory exists
      if (!Directory.Exists(directoryPath)) {
        Directory.CreateDirectory(directoryPath);
      }

      if (!overwrite && File.Exists(fileLocation)) {
        throw new IOException(
          $"The file '{filename}.{fileExtension}' already exists and cannot be overwritten.");
      }

      File.WriteAllText(fileLocation, serializer.Serialize(data));
    }

    public T Load<T>(string name) {
      var fileLocation = GetPathToFile(name);

      if (!File.Exists(fileLocation)) {
        throw new ArgumentException($"No persisted GameData with name '{name}'");
      }

      return serializer.Deserialize<T>(File.ReadAllText(fileLocation));
    }

    public void Delete(string name) {
      var fileLocation = GetPathToFile(name);

      if (File.Exists(fileLocation)) {
        File.Delete(fileLocation);
      }
    }
  }
}