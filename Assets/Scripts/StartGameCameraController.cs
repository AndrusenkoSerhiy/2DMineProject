using System.Collections;
using Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StartGameCameraController : MonoBehaviour {
  public CinemachineCamera cinemachineCamera;
  public Transform player;
  public Vector3 playerStartPosition;
  public Vector3 cameraStartPosition;
  public float startOrthographicSize = 30f;
  public float targetOrthographicSize = 18f;
  public float waitOnStart = 1f;
  public Volume volume;

  private bool isFollowing;
  private Rigidbody2D playerRb;
  private Coroutine followCoroutine;
  private Coroutine vignetteCoroutine;
  private Vignette vignette;

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
    vignetteCoroutine = StartCoroutine(FadeVignette(0.537f, 1.5f));
  }

  public void ResetBeforeNewGame() {
    var gameManager = GameManager.Instance;
    var playerController = gameManager.PlayerController;
    playerController.transform.SetParent(null);
    playerController.enabled = true;
    playerController.EnableCollider(true);
    gameManager.CurrPlayerController = playerController;
  }

  public void Init() {
    SetPlayerToStartPosition();
    SetCameraToStartPosition();
  }

  public void ResetPlayer() {
    playerRb.simulated = true;
    playerRb.gravityScale = 0f;
  }

  private void SetPlayerToStartPosition() {
    playerRb = player.GetComponent<Rigidbody2D>();
    playerRb.simulated = false;
    playerRb.gravityScale = 200f;
    player.transform.position = playerStartPosition;
  }

  private void SetCameraToStartPosition() {
    cinemachineCamera.Follow = null;
    cinemachineCamera.transform.position = new Vector3(playerStartPosition.x, cameraStartPosition.y,
      cinemachineCamera.transform.position.z);
    cinemachineCamera.Lens.OrthographicSize = startOrthographicSize;

    volume.profile.TryGet(out vignette);
    if (vignette) {
      vignette.intensity.value = 1f;
    }
  }

  private IEnumerator FollowPlayer() {
    cinemachineCamera.Follow = player;
    GameManager.Instance.CameraConfigManager.SetCameraHigh();
    playerRb.simulated = true;
    yield return new WaitForSeconds(waitOnStart);
    while (playerRb.linearVelocity.magnitude > 0.1f) {
      yield return null;
    }

    GameManager.Instance.CameraConfigManager.SetCameraLow();
    playerRb.gravityScale = 0f;
    yield return new WaitForSeconds(1f);
    GameManager.Instance.CameraConfigManager.SetCameraDefault();
    GameManager.Instance.QuestManager.StartQuest(0);
    Stop();
  }

  IEnumerator FadeVignette(float targetIntensity, float duration) {
    float startIntensity = vignette.intensity.value;
    float elapsedTime = 0f;

    while (elapsedTime < duration) {
      vignette.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / duration);
      elapsedTime += Time.deltaTime;
      yield return null;
    }

    vignette.intensity.value = targetIntensity;
    StopVignette();
  }

  private void StopVignette() {
    if (vignetteCoroutine == null) {
      return;
    }

    StopCoroutine(vignetteCoroutine);
  }

  private void Stop() {
    if (followCoroutine == null) {
      return;
    }

    StopCoroutine(followCoroutine);
    followCoroutine = null;
  }
}