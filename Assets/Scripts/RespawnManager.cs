using UnityEngine;

public class RespawnManager : MonoBehaviour {
    [SerializeField] private Transform defaultRespawnPoint;
    [SerializeField] private Transform respawnPoint;

    public void SetRespawnPoint(Transform tr) {
      respawnPoint = tr;
    }
    public Vector3 GetRespawnPoint() {
      return respawnPoint == null ? defaultRespawnPoint.position : respawnPoint.position;
    }
}