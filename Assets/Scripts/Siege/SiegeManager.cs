using System;
using System.Collections.Generic;
using Actors;
using Audio;
using SaveSystem;
using Scriptables.Siege;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Siege {
  public class SiegeManager : MonoBehaviour, ISaveLoad {
    [SerializeField] private SiegesSettings siegesSettings;
    [SerializeField] private ZombieDifficultyDatabase zombieDifficultyDatabase;
    [SerializeField] private SiegeTimelineUI siegeTimelineUI;
    [SerializeField] private bool calculateWithWeightAndCycle = true;

    public event Action<ActiveSiegeTemplate> OnSiegeStarted;
    public event Action<ActiveSiegeTemplate> OnSiegeEnded;
    public event Action<ActiveSiegeTemplate> OnZombieSpawn;

    private List<ActiveSiegeTemplate> siegeQueue = new();
    [SerializeField] private int currentSiegeCycle = 1;
    [SerializeField] private int currentSiegeIndex = 0;
    [SerializeField] private ActiveSiegeTemplate currentSiege;

    [SerializeField] private bool siegesStarted = false;
    [SerializeField] private bool isSiegeInProgress = false;
    [SerializeField] private bool isPaused = false;

    [SerializeField] private float durationTimer = 0f;
    [SerializeField] private float siegeCycleElapsedTime = 0f;
    [SerializeField] private float totalCycleTime = 0f;
    [SerializeField] private float timeToNextSegment = 0f;

    public bool IsPaused => isPaused;
    public bool SiegesStarted => siegesStarted;
    public bool IsSiegeInProgress => isSiegeInProgress;
    public float TimeToNextSegment => timeToNextSegment;
    public float SiegeCycleElapsedTime => siegeCycleElapsedTime;
    public float TotalCycleTime => totalCycleTime;
    public ZombieDifficultyDatabase ZombieDifficultyDatabase => zombieDifficultyDatabase;

    private enum SiegePhase {
      Idle,
      WaitingBeforeSiege,
      ActiveSiege
    }

    private SiegePhase currentPhase = SiegePhase.Idle;
    private GameManager gameManager;
    private AudioController audioController;

    private float waitTimer = 0f;
    private float nextWaveTime = 0f;
    private int wavesSpawned = 0;

    private class PendingNotification {
      public float TriggerTime;
      public string Message;
      public bool Triggered;
    }

    private readonly List<PendingNotification> pendingNotifications = new();

    #region SaveLoad

    public int Priority => LoadPriority.SIEGE;

    public void Save() {
      var data = SaveLoadSystem.Instance.gameData.SiegeData;

      data.CurrentSiegeCycle = currentSiegeCycle;
      data.CurrentSiegeIndex = currentSiegeIndex;
      data.DurationTimer = durationTimer;
      data.SiegeCycleElapsedTime = siegeCycleElapsedTime;
      data.SiegesStarted = siegesStarted;
      data.IsPaused = isPaused;
      data.SiegeQueue = new List<ActiveSiegeTemplate>(siegeQueue);
      data.TotalCycleTime = totalCycleTime;
      data.IsSiegeInProgress = isSiegeInProgress;
      data.TimeToNextSegment = timeToNextSegment;
      data.CurrentPhase = (int)currentPhase;
      data.CurrentSiege = currentSiege;
      data.WaitTimer = waitTimer;
      data.NextWaveTime = nextWaveTime;
      data.WavesSpawned = wavesSpawned;
      data.IsSet = true;
    }

    public void Load() {
      var data = SaveLoadSystem.Instance.gameData.SiegeData;
      if (SaveLoadSystem.Instance.IsNewGame() || !data.IsSet) {
        return;
      }

      currentSiegeCycle = data.CurrentSiegeCycle;
      currentSiegeIndex = data.CurrentSiegeIndex;
      durationTimer = data.DurationTimer;
      siegeCycleElapsedTime = data.SiegeCycleElapsedTime;
      siegesStarted = data.SiegesStarted;
      isPaused = data.IsPaused;
      siegeQueue = new List<ActiveSiegeTemplate>(data.SiegeQueue);
      totalCycleTime = data.TotalCycleTime;
      isSiegeInProgress = data.IsSiegeInProgress;
      timeToNextSegment = data.TimeToNextSegment;
      currentPhase = (SiegePhase)data.CurrentPhase;
      currentSiege = data.CurrentSiege;
      waitTimer = data.WaitTimer;
      nextWaveTime = data.NextWaveTime;
      wavesSpawned = data.WavesSpawned;

      pendingNotifications.Clear();

      if (siegesStarted && siegeQueue.Count > 0) {
        siegeTimelineUI.SetupTimeline(siegeQueue);
      }
    }

    public void Clear() {
      siegeTimelineUI.Reset();
      siegeQueue.Clear();

      currentSiegeCycle = 1;
      currentSiegeIndex = 0;
      currentSiege = null;
      siegesStarted = false;
      isSiegeInProgress = false;
      isPaused = false;

      durationTimer = 0f;
      siegeCycleElapsedTime = 0f;
      totalCycleTime = 0f;
      timeToNextSegment = 0f;
      waitTimer = 0f;
      nextWaveTime = 0f;
      wavesSpawned = 0;

      pendingNotifications.Clear();
      currentPhase = SiegePhase.Idle;
    }

    #endregion

    private void Start() {
      SaveLoadSystem.Instance.Register(this);
      gameManager = GameManager.Instance;
      audioController = gameManager.AudioController;

      ActorPlayer.OnPlayerDeath += OnPlayerDeathHandler;
      ActorPlayer.OnPlayerRespawn += OnPlayerRespawnHandler;
      gameManager.OnGamePaused += OnGamePausedHandler;
      gameManager.OnGameResumed += OnGameResumedHandler;
    }

    private void Update() {
      if (!siegesStarted || isPaused || currentSiege == null) {
        return;
      }

      var deltaTime = Time.deltaTime;
      siegeCycleElapsedTime += deltaTime;

      switch (currentPhase) {
        case SiegePhase.WaitingBeforeSiege:
          UpdateWaitingBeforeSiege(deltaTime);
          break;

        case SiegePhase.ActiveSiege:
          UpdateActiveSiege(deltaTime);
          break;

        case SiegePhase.Idle:
          break;

        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private void UpdateWaitingBeforeSiege(float deltaTime) {
      waitTimer -= deltaTime;
      timeToNextSegment = Mathf.Max(waitTimer, 0f);

      TriggerPendingNotifications();

      if (waitTimer <= 0f) {
        StartSiege();
      }
    }

    private void TriggerPendingNotifications() {
      foreach (var notif in pendingNotifications) {
        if (notif.Triggered || !(waitTimer <= notif.TriggerTime)) {
          continue;
        }

        notif.Triggered = true;
        gameManager.MessagesManager.ShowSimpleMessage(notif.Message);
      }
    }

    private void UpdateActiveSiege(float deltaTime) {
      durationTimer += deltaTime;
      timeToNextSegment = Mathf.Max(currentSiege.Duration - durationTimer, 0f);

      if (ShouldSpawnNextWave()) {
        ZombieSpawn();
        wavesSpawned++;
        nextWaveTime += currentSiege.Duration / currentSiege.WavesOfZombies;
      }

      if (!(durationTimer >= currentSiege.Duration)) {
        return;
      }

      EndSiege();
      PrepareNextSiege();
    }

    private bool ShouldSpawnNextWave() {
      return durationTimer >= nextWaveTime && wavesSpawned < currentSiege.WavesOfZombies;
    }

    #region Siege Control

    public void StartSieges() {
      if (siegesStarted) {
        return;
      }

      siegeQueue.Clear();
      siegeQueue.Add(GetActiveTemplate(siegesSettings.FirstSiege));

      var min = (int)siegesSettings.SiegesCount.x - 2;
      var max = (int)siegesSettings.SiegesCount.y - 2;
      var count = Random.Range(min, max + 1);

      for (var i = 0; i < count; i++) {
        siegeQueue.Add(GetActiveTemplate(siegesSettings.RandomSiegeTemplate));
      }

      siegeQueue.Add(GetActiveTemplate(siegesSettings.FinalSiege));

      totalCycleTime = 0f;
      foreach (var siege in siegeQueue) {
        totalCycleTime += siege.TimeBeforeSiege + siege.Duration;
      }

      siegeCycleElapsedTime = 0f;
      currentSiegeIndex = 0;
      siegesStarted = true;

      siegeTimelineUI.SetupTimeline(siegeQueue);

      PrepareNextSiege();
    }

    private void PrepareNextSiege() {
      if (currentSiegeIndex >= siegeQueue.Count) {
        siegesStarted = false;
        currentSiegeCycle++;
        StartSieges();
        return;
      }

      currentSiege = siegeQueue[currentSiegeIndex];
      currentPhase = SiegePhase.WaitingBeforeSiege;
      waitTimer = currentSiege.TimeBeforeSiege;
      durationTimer = 0f;
      timeToNextSegment = waitTimer;

      pendingNotifications.Clear();
      foreach (var notif in siegesSettings.PreSiegeNotifications) {
        if (notif.SecondsBeforeStart < waitTimer) {
          pendingNotifications.Add(new PendingNotification {
            TriggerTime = notif.SecondsBeforeStart,
            Message = notif.Message,
            Triggered = false
          });
        }
      }
    }

    private void StartSiege() {
      StratSiegeAudio();
      currentPhase = SiegePhase.ActiveSiege;
      isSiegeInProgress = true;
      durationTimer = 0f;
      wavesSpawned = 0;
      nextWaveTime = 0f;

      OnSiegeStarted?.Invoke(currentSiege);
      gameManager.MessagesManager.ShowSimpleMessage("Siege started!");
    }

    private void EndSiege(bool playedDied = false) {
      EndSiegeAudio();
      isSiegeInProgress = false;
      currentPhase = SiegePhase.Idle;

      OnSiegeEnded?.Invoke(currentSiege);
      gameManager.MessagesManager.ShowSimpleMessage("Siege ended!");

      // if (!playedDied && currentSiegeIndex == siegeQueue.Count - 1) {
      if (!playedDied) {
        gameManager.ObjectivesSystem.ReportSurviveSiege();
      }

      currentSiegeIndex++;
    }

    private void ZombieSpawn() {
      OnZombieSpawn?.Invoke(currentSiege);
    }

    #endregion

    #region Audio

    private void StratSiegeAudio() {
      audioController.StopMainTheme();
      audioController.PlaySiegeTheme();
    }

    private void EndSiegeAudio() {
      audioController.StopSiegeTheme();
      audioController.PlayMainTheme();
    }

    private void ResumeSiegeAudio() {
      audioController.StopMainTheme();
      audioController.ResumeSiegeTheme();
    }

    private void PauseSiegeAudio() {
      audioController.PauseSiegeTheme();
      audioController.PlayMainTheme();
    }

    #endregion

    #region Pause/Resume

    private void PauseSiege() {
      isPaused = true;

      if (isSiegeInProgress) {
        PauseSiegeAudio();
      }
    }

    private void ResumeSiege() {
      if (!isPaused || currentSiege == null) {
        return;
      }

      isPaused = false;
      if (isSiegeInProgress) {
        ResumeSiegeAudio();
      }
    }

    private void ResumeWithSkipSiege() {
      if (!isSiegeInProgress) {
        ResumeSiege();
        return;
      }

      var remainingTime = Mathf.Max(currentSiege.Duration - durationTimer, 0f);
      siegeCycleElapsedTime += remainingTime;
      timeToNextSegment = 0f;
      durationTimer = 0f;

      EndSiege(true);
      PrepareNextSiege();
      ResumeSiege();
    }

    #endregion

    #region EventHandlers

    private void OnPlayerDeathHandler() {
      PauseSiege();
    }

    private void OnPlayerRespawnHandler() {
      ResumeWithSkipSiege();
    }

    private void OnGameResumedHandler() {
      siegeTimelineUI.gameObject.SetActive(true);
      ResumeSiege();
    }

    private void OnGamePausedHandler() {
      PauseSiege();
      siegeTimelineUI.gameObject.SetActive(false);
    }

    #endregion

    public DifficultyEntry GetDifficultyProfilesByWeight() {
      return zombieDifficultyDatabase.GetProfileByWeight(gameManager.PlayerInventory.Weight);
    }

    private ActiveSiegeTemplate GetActiveTemplate(SiegeTemplate template) {
      var weight = gameManager.PlayerInventory.Weight;
      return calculateWithWeightAndCycle
        ? new ActiveSiegeTemplate(template, weight, currentSiegeCycle)
        : new ActiveSiegeTemplate(template);
    }
  }
}