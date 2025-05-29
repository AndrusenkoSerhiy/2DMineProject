using System;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Analytics {
  [DefaultExecutionOrder(-2)]
  public class AnalyticsManager : MonoBehaviour {
    public static AnalyticsManager Instance;

    [Header("Matomo Config")] [SerializeField]
    private string matomoUrl = "https://matomo.olehm.site/matomo.php";

    [SerializeField] private string siteUrl = "https://rockstoolshordes.com/";

    [SerializeField] private int siteId = 1;

    [Header("Options")] [SerializeField] private bool analyticsEnabled = true;
    [SerializeField] private AnalyticsEvent analyticsEvent;

    private string playerId;
    private float gameTime = 0f;
    private float idleTime = 0f;
    private float menuTime = 0f;

    private float idleThreshold = 5f;
    private float lastInputTime;

    private void Awake() {
      if (Instance) {
        Destroy(gameObject);
        return;
      }

      Instance = this;
      DontDestroyOnLoad(gameObject);

      // Завантажити/створити playerId
      if (!PlayerPrefs.HasKey("player_id")) {
        var newId = Guid.NewGuid().ToString("N");
        PlayerPrefs.SetString("player_id", newId);
        PlayerPrefs.Save();
      }

      playerId = PlayerPrefs.GetString("player_id");
    }

    private void Start() {
      lastInputTime = Time.time;
    }

    private async Task OnApplicationQuit() {
      await SendBasicStatsAsync();
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

    private async Task SendBasicStatsAsync() {
      if (!analyticsEnabled) {
        return;
      }

      await LogCustomEvent("GameTime(m)", "GameTime", "Playtime", (gameTime / 60));
      await LogCustomEvent("GameTime(m)", "IdleTime", "Idle", (idleTime / 60));
      await LogCustomEvent("GameTime(m)", "MenuTime", "Menu", (menuTime / 60));
    }

    public async Task LogCustomEvent(string category, string action, string eventName, double value = 0) {
      if (!analyticsEnabled || string.IsNullOrEmpty(matomoUrl)) {
        return;
      }

      var url = $"{matomoUrl}?idsite={siteId}&rec=1" +
                $"&uid={playerId}" +
                $"&_id={playerId.Substring(0, 16)}" +
                "&apiv=1" +
                "&bots=1" +
                $"&url={siteUrl}" +
                $"&e_c={UnityWebRequest.EscapeURL(category)}" +
                $"&e_a={UnityWebRequest.EscapeURL(action)}" +
                $"&e_n={UnityWebRequest.EscapeURL(eventName)}" +
                $"&e_v={value.ToString("F2", CultureInfo.InvariantCulture)}" +
                $"&rand={UnityEngine.Random.Range(0, 999999)}";

      using var request = UnityWebRequest.Get(url);
      var op = request.SendWebRequest();
      while (!op.isDone) {
        await Task.Yield();
      }

      if (request.result != UnityWebRequest.Result.Success) {
        Debug.LogWarning("Matomo analytics failed: " + request.error);
      }
    }

    public void LogItemCrafted(string item, int count) {
      LogCustomEvent("Items", analyticsEvent.ItemCrafted, item, count);
    }

    public void LogStationPlaced(string stationName) {
      LogCustomEvent("Stations", analyticsEvent.StationPlaced, stationName);
    }

    public void LogPlayerDied() {
      LogCustomEvent("Player", analyticsEvent.PlayerDied, analyticsEvent.PlayerDied);
    }

    public void LogRobotRepaired(string robot, float repairValue) {
      LogCustomEvent("Robots", analyticsEvent.RobotRepaired, robot, repairValue);
    }
  }
}