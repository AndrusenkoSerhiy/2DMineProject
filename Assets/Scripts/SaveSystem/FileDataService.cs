using System;
using System.Collections.Generic;
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

    public void Save(GameData data, bool overwrite = true) {
      var fileLocation = GetPathToFile(data.FileName);
      var directoryPath = Path.GetDirectoryName(fileLocation);

      // Ensure the directory exists
      if (!Directory.Exists(directoryPath)) {
        Directory.CreateDirectory(directoryPath);
      }

      if (!overwrite && File.Exists(fileLocation)) {
        throw new IOException(
          $"The file '{data.FileName}.{fileExtension}' already exists and cannot be overwritten.");
      }

      File.WriteAllText(fileLocation, serializer.Serialize(data));
    }

    public GameData Load(string name) {
      var fileLocation = GetPathToFile(name);

      if (!File.Exists(fileLocation)) {
        throw new ArgumentException($"No persisted GameData with name '{name}'");
      }

      return serializer.Deserialize<GameData>(File.ReadAllText(fileLocation));
    }

    public void Delete(string name) {
      var fileLocation = GetPathToFile(name);

      if (File.Exists(fileLocation)) {
        File.Delete(fileLocation);
      }
    }

    public void DeleteAll() {
      foreach (var filePath in Directory.GetFiles(dataPath)) {
        File.Delete(filePath);
      }
    }

    public IEnumerable<string> ListSaves() {
      foreach (var path in Directory.EnumerateFiles(dataPath)) {
        var fileName = Path.GetFileName(path);

        // Check if the file starts with "Game_" and has the correct file extension
        if (fileName.StartsWith("Game_") && Path.GetExtension(path) == fileExtension) {
          yield return Path.GetFileNameWithoutExtension(path);
        }
      }
    }
  }
}