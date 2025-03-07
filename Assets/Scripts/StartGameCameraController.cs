using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class StartGameCameraController : MonoBehaviour {
  public CinemachineCamera cinemachineCamera;
  public Transform player;
  public Vector3 playerStartPosition;
  public Vector3 cameraStartPosition;
  public float startOrthographicSize = 30f;
  public float targetOrthographicSize = 18f;
  public float followDuration = 2f;
  public float waitOnStart = 1f;

  private bool isFollowing;
  private Rigidbody2D playerRb;
  private Coroutine followCoroutine;

  private void Awake() {
    if (player == null) {
      Debug.LogError("StartGameCameraController Player not found!");
    }

    if (cinemachineCamera == null) {
      Debug.LogError("StartGameCameraController Camera not found!");
    }
  }

  public void Play() {
    followCoroutine = StartCoroutine(FollowPlayer());
  }

  public void Init() {
    playerRb = player.GetComponent<Rigidbody2D>();
    playerRb.simulated = false;
    player.transform.position = playerStartPosition;
    cinemachineCamera.Follow = null;
    cinemachineCamera.transform.position = new Vector3(playerStartPosition.x, cameraStartPosition.y,
      cinemachineCamera.transform.position.z);
    cinemachineCamera.Lens.OrthographicSize = startOrthographicSize;
  }

  private IEnumerator FollowPlayer() {
    playerRb.simulated = true;
    yield return new WaitForSeconds(waitOnStart);

    var elapsedTime = 0f;

    while (elapsedTime < followDuration) {
      elapsedTime += Time.deltaTime;
      var t = elapsedTime / followDuration;
      cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(startOrthographicSize, targetOrthographicSize, t);

      if (!cinemachineCamera.Follow && player.transform.position.y <= cinemachineCamera.transform.position.y) {
        cinemachineCamera.ForceCameraPosition(player.transform.position, player.transform.rotation);
        cinemachineCamera.Follow = player;
      }

      yield return null;
    }

    cinemachineCamera.Lens.OrthographicSize = targetOrthographicSize;

    Stop();
  }

  private void Stop() {
    if (followCoroutine == null) {
      return;
    }

    StopCoroutine(followCoroutine);
    followCoroutine = null;
  }
}