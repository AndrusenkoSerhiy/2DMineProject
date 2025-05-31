using System;
using System.Collections;
using System.Threading.Tasks;
using SaveSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace Analytics {
  [Serializable]
  public class TimelineEvent {
    public string player_id;
    public string session_id;
    public int play_time_seconds;
    public int idle_time_seconds;
    public int menu_time_seconds;
  }

  [Serializable]
  public class GameEvent {
    public string player_id;
    public string session_id;
    public string event_name;
    public string metadata;
  }

  [DefaultExecutionOrder(-2)]
  public class AnalyticsManager : MonoBehaviour {
    public static AnalyticsManager Instance;

    [Header("PostgREST Config")] [SerializeField]
    private string postgrestUrl = "https://postgrest.olehm.site";

    [Header("Options")] [SerializeField] private bool analyticsEnabled = true;
    [SerializeField] private bool onlyInBuild = true;
    [SerializeField] private AnalyticsEvent analyticsEvent;
    [SerializeField] private int sendPlayTimeInterval = 5;

    private string playerId;
    private string sessionId;
    private float gameTime = 0f;
    private float idleTime = 0f;
    private float menuTime = 0f;

    private float idleThreshold = 5f;
    private float lastInputTime;

    private bool timelineSent = false;

    private string token =
      "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoibmV1cmFsbW9ua2V5c191c2VyIn0.2ny64uTqnmdY8QHWFfvVt5X9XELn-f-80SW8YD-GOPc";

    private void Awake() {
      if (Instance) {
        Destroy(gameObject);
        return;
      }

      Instance = this;
      DontDestroyOnLoad(gameObject);

      // Створення або завантаження playerId
      if (!PlayerPrefs.HasKey("player_id")) {
        var newId = Guid.NewGuid().ToString("N");
        PlayerPrefs.SetString("player_id", newId);
        PlayerPrefs.Save();
      }

      playerId = PlayerPrefs.GetString("player_id");
      sessionId = Guid.NewGuid().ToString("N");

#if UNITY_EDITOR
      if (onlyInBuild) {
        analyticsEnabled = false;
      }
#endif
    }

    private void Start() {
      LogUserInfo();

      lastInputTime = Time.time;

      if (analyticsEnabled) {
        StartCoroutine(TimelineUpdateCoroutine());
      }
    }

    private IEnumerator TimelineUpdateCoroutine() {
      while (true) {
        yield return new WaitForSeconds(sendPlayTimeInterval);
        _ = SendTimelineEvent(gameTime, idleTime, menuTime);
      }
    }

    private void Update() {
      gameTime += Time.deltaTime;

      if (GameManager.Instance.GameStage == GameStage.MainMenu) {
        menuTime += Time.deltaTime;
      }

      if (Input.anyKey || Input.mouseScrollDelta.sqrMagnitude > 0 || Input.touchCount > 0) {
        lastInputTime = Time.time;
      }

      if (Time.time - lastInputTime > idleThreshold) {
        idleTime += Time.deltaTime;
      }
    }

    public async Task SendBasicStatsAsync() {
      await SendTimelineEvent(gameTime, idleTime, menuTime);
    }

    public void LogItemCrafted(string item, int count) {
      //SendGameEvent(analyticsEvent.ItemCrafted, $"{item}, {count}");
    }

    public void LogStationPlaced(string stationName) {
      SendGameEvent(analyticsEvent.StationPlaced, stationName);
    }

    public void LogPlayerDied() {
      SendGameEvent(analyticsEvent.PlayerDied);
    }

    public void LogRobotRepaired(string robot, float repairValue) {
      SendGameEvent(analyticsEvent.RobotRepaired, $"{robot}, {repairValue}");
    }

    public void LogUserInfo() {
      SendGameEvent(analyticsEvent.UserInfo, GetSystemInfoText());
    }

    public void LogProfileInfo(string profileName) {
      SendGameEvent(analyticsEvent.ProfileSelected, profileName);
    }

    private string GetSystemInfoText() {
      const string newline = "\n";
      return
        $"| os: {SystemInfo.operatingSystem}{newline} | " +
        $"platform: {Application.platform}{newline} | " +
        $"device_model: {SystemInfo.deviceModel}{newline} | " +
        $"cpu: {SystemInfo.processorType}{newline} | " +
        $"gpu: {SystemInfo.graphicsDeviceName}{newline} | " +
        $"ram_mb: {SystemInfo.systemMemorySize}{newline} | " +
        $"gpu_ram_mb: {SystemInfo.graphicsMemorySize}{newline} | " +
        $"screen_res: {Screen.width}x{Screen.height}{newline} | " +
        $"language: {Application.systemLanguage}{newline} | " +
        $"game_version: {Application.version} |";
    }

    private async Task SendGameEvent(string eventName, string metadata = null) {
      if (!analyticsEnabled || string.IsNullOrEmpty(postgrestUrl)) {
        return;
      }

      try {
        var ev = new GameEvent {
          player_id = playerId,
          session_id = sessionId,
          event_name = eventName,
          metadata = metadata
        };

        var json = JsonUtility.ToJson(ev);
        var request = new UnityWebRequest($"{postgrestUrl}/game_events", "POST");
        var bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Content-Type", "application/json");

        var op = request.SendWebRequest();
        while (!op.isDone) {
          await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success) {
          Debug.LogWarning("PostgREST error (game_event): " + request.error);
        }
      }
      catch (Exception ex) {
        Debug.LogException(ex);
      }
    }

    private async Task SendTimelineEvent(float playTime, float idle, float menu) {
      if (!analyticsEnabled || string.IsNullOrEmpty(postgrestUrl)) {
        return;
      }

      try {
        var ev = new TimelineEvent {
          player_id = playerId,
          session_id = sessionId,
          play_time_seconds = Mathf.RoundToInt(playTime),
          idle_time_seconds = Mathf.RoundToInt(idle),
          menu_time_seconds = Mathf.RoundToInt(menu)
        };

        var json = JsonUtility.ToJson(ev);
        UnityWebRequest request;

        request = !timelineSent
          ? new UnityWebRequest($"{postgrestUrl}/time_events", "POST")
          : new UnityWebRequest($"{postgrestUrl}/time_events?session_id=eq.{sessionId}", "PATCH");

        var bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Content-Type", "application/json");

        var op = request.SendWebRequest();
        while (!op.isDone) {
          await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success) {
          Debug.LogWarning("PostgREST error (timeline): " + request.error);
        }
        else {
          timelineSent = true;
        }
      }
      catch (Exception ex) {
        Debug.LogException(ex);
      }
    }
  }
}