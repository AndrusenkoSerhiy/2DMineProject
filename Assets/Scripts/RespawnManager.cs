using SaveSystem;
using UnityEngine;

public class RespawnManager : MonoBehaviour, ISaveLoad {
    [SerializeField] private Transform defaultRespawnPoint;
    [SerializeField] private Vector3 respawnPoint;

    private void Awake() {
      SaveLoadSystem.Instance.Register(this);
    }
    
    public void SetRespawnPoint(Vector3 pos) {
      respawnPoint = pos;
    }
    public Vector3 GetRespawnPoint() {
      return respawnPoint.Equals(Vector3.zero) ? defaultRespawnPoint.position : respawnPoint;
    }
    
    #region Save/Load

    public int Priority { get; }

    public void Save() {
      SaveLoadSystem.Instance.gameData.RespawnPosition = respawnPoint;
    }

    public void Load() {
      respawnPoint = SaveLoadSystem.Instance.gameData.RespawnPosition;
    }

    public void Clear() {
      respawnPoint = Vector3.zero;
    }

    #endregion
}