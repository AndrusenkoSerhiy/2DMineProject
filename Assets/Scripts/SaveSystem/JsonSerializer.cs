using UnityEngine;

namespace SaveSystem {
  public class JsonSerializer : ISerializer {
    private readonly bool pretty;

    public JsonSerializer(bool pretty = true) {
      this.pretty = pretty;
    }

    public string Serialize<T>(T obj) {
      return JsonUtility.ToJson(obj, pretty);
    }

    public T Deserialize<T>(string json) {
      return JsonUtility.FromJson<T>(json);
    }
  }
}