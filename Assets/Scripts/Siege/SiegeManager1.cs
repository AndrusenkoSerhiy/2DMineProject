/*using System;
using System.Collections;
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
    private GameManager gameManager;

    [SerializeField] private bool siegesStarted = false;
    [SerializeField] private bool isSiegeInProgress;
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

    private Coroutine activeSiegeCoroutine;
    private AudioController audioController;

    private class PendingNotification {
      public float TriggerTime;
      public string Message;
      public bool Triggered;
    }

    private readonly List<PendingNotification> pendingNotifications = new();

    private void Start() {
      SaveLoadSystem.Instance.Register(this);
      gameManager = GameManager.Instance;
      audioController = gameManager.AudioController;
      ActorPlayer.OnPlayerDeath += OnPlayerDeathHandler;
      ActorPlayer.OnPlayerRespawn += OnPlayerRespawnHandler;

      gameManager.OnGamePaused += OnGamePausedHandler;
      gameManager.OnGameResumed += OnGameResumedHandler;
    }

    #region Save/Load

    public int Priority => LoadPriority.SIEGE;

    public void Save() {
      var data = SaveLoadSystem.Instance.gameData.SiegeData;

      data.CurrentSiegeCycle = currentSiegeCycle;
      data.CurrentSiegeIndex = currentSiegeIndex;
      data.DurationTimer = durationTimer;
      data.SiegeCycleElapsedTime = siegeCycleElapsedTime;
      data.SiegesStarted = siegesStarted;
      data.IsPaused = isPaused;
      data.SiegeQueue = siegeQueue;
      data.TotalCycleTime = totalCycleTime;
      data.IsSiegeInProgress = isSiegeInProgress;
      data.TimeToNextSegment = timeToNextSegment;
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
      siegeQueue = data.SiegeQueue;
      totalCycleTime = data.TotalCycleTime;
      isSiegeInProgress = data.IsSiegeInProgress;
      timeToNextSegment = data.TimeToNextSegment;

      if (siegesStarted) {
        siegeTimelineUI.SetupTimeline(siegeQueue);
        StopActiveCoroutine();
        activeSiegeCoroutine = StartCoroutine(RunNextSiege());
      }
    }

    public void Clear() {
      StopActiveCoroutine();

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
    }

    #endregion

    public DifficultyEntry GetDifficultyProfilesByWeight() {
      return zombieDifficultyDatabase.GetProfileByWeight(gameManager.PlayerInventory.Weight);
    }

    public void StartSieges() {
      if (siegesStarted) {
        return;
      }

      StopActiveCoroutine();

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
      siegeTimelineUI.SetupTimeline(siegeQueue);

      siegesStarted = true;
      activeSiegeCoroutine = StartCoroutine(RunNextSiege());
      /*foreach (var siege in siegeQueue) {
        Debug.LogWarning(
          $"TimeBeforeSiege {siege.TimeBeforeSiege} | duration {siege.Duration} | waves {siege.WavesOfZombies} | zombies {siege.ZombieCount}");
      }#1#
    }

    private void StopActiveCoroutine() {
      if (activeSiegeCoroutine == null) {
        return;
      }

      StopCoroutine(activeSiegeCoroutine);
      activeSiegeCoroutine = null;
    }

    private ActiveSiegeTemplate GetActiveTemplate(SiegeTemplate template) {
      var weight = gameManager.PlayerInventory.Weight;
      return calculateWithWeightAndCycle
        ? new ActiveSiegeTemplate(template, weight, currentSiegeCycle)
        : new ActiveSiegeTemplate(template);
    }

    private IEnumerator RunNextSiege() {
      if (currentSiegeIndex >= siegeQueue.Count) {
        // Debug.LogWarning("Run second Siege");
        siegesStarted = false;
        currentSiegeIndex = 0;
        currentSiegeCycle++;

        StartSieges();

        yield break;
      }

      currentSiege = siegeQueue[currentSiegeIndex];
      var waitTime = currentSiege.TimeBeforeSiege;
      pendingNotifications.Clear();

      foreach (var notif in siegesSettings.PreSiegeNotifications) {
        if (notif.SecondsBeforeStart < waitTime) {
          pendingNotifications.Add(new PendingNotification {
            TriggerTime = notif.SecondsBeforeStart,
            Message = notif.Message,
            Triggered = false
          });
        }
      }

      while (waitTime > 0f) {
        if (!isPaused) {
          var delta = Time.deltaTime;
          waitTime -= delta;
          timeToNextSegment = Mathf.Max(waitTime, 0f);
          siegeCycleElapsedTime += delta;

          foreach (var notif in pendingNotifications) {
            if (!notif.Triggered && waitTime <= notif.TriggerTime) {
              notif.Triggered = true;
              gameManager.MessagesManager.ShowSimpleMessage(notif.Message);
            }
          }
        }

        yield return null;
      }

      StartSiege();

      var wavesSpawned = 0;
      durationTimer = 0f;
      var nextWaveTime = 0f;
      var interval = currentSiege.Duration / currentSiege.WavesOfZombies;

      while (durationTimer < currentSiege.Duration) {
        if (!isPaused) {
          var delta = Time.deltaTime;
          durationTimer += delta;
          siegeCycleElapsedTime += delta;
          timeToNextSegment = Mathf.Max(currentSiege.Duration - durationTimer, 0f);

          if (durationTimer >= nextWaveTime && wavesSpawned < currentSiege.WavesOfZombies) {
            ZombieSpawn();
            wavesSpawned++;
            nextWaveTime += interval;
          }
        }

        yield return null;
      }

      EndSiege();
      currentSiegeIndex++;

      StopActiveCoroutine();

      activeSiegeCoroutine = StartCoroutine(RunNextSiege());
    }

    private void StartSiege() {
      StratSiegeAudio();
      isSiegeInProgress = true;
      OnSiegeStarted?.Invoke(currentSiege);
      gameManager.MessagesManager.ShowSimpleMessage("Siege started!");
    }

    private void EndSiege() {
      EndSiegeAudio();
      isSiegeInProgress = false;
      OnSiegeEnded?.Invoke(currentSiege);
      gameManager.MessagesManager.ShowSimpleMessage("Siege ended!");
    }

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

    private void ZombieSpawn() {
      OnZombieSpawn?.Invoke(currentSiege);
    }

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

      StopActiveCoroutine();
      EndSiege();

      var remainingTime = Mathf.Max(currentSiege.Duration - durationTimer, 0f);
      siegeCycleElapsedTime += remainingTime;
      timeToNextSegment = 0f;
      durationTimer = 0f;

      currentSiegeIndex++;
      activeSiegeCoroutine = StartCoroutine(RunNextSiege());

      ResumeSiege();
    }

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
  }
}*/