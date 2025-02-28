using System;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityServiceLocator;

namespace Craft {
  public class Timer : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI timerText;

    private Workstation station;
    private int timeForOne;
    private int totalItems;
    private int itemsLeft;
    private float totalTime;
    private float timeLeft;
    private float lastCheckTime;
    private bool isStarted;
    private bool isPaused;

    public Action onTimerStop;
    public Action<int> onItemTimerEnd;
    public bool IsStarted => isStarted;

    public void Awake() {
      station = ServiceLocator.For(this).Get<Workstation>();
    }

    public void Update() {
      if (!isStarted || isPaused) {
        return;
      }

      TimerTick();
    }

    public void InitTimer(int count, int time) {
      totalItems = count;
      timeForOne = time;
      totalTime = count * time;
      itemsLeft = totalItems;
      lastCheckTime = totalTime;

      if (station.SecondsLeft > 0) {
        timeLeft = Mathf.Clamp(station.SecondsLeft, 0, totalTime);
        station.SecondsLeft = 0;
      }
      else {
        timeLeft = totalTime;
      }

      PrintTime();
    }
    
    public void Pause() => isPaused = true;

    public void StartTimer() {
      isStarted = true;
      var currentTime = Helper.GetCurrentTime();
      var timePassed = totalTime - timeLeft;
      station.CraftStartTime = timePassed <= 0 ? currentTime : currentTime.AddSeconds(-timePassed);
    }

    private void CheckItemCompletion() {
      var count = 0;
      while (itemsLeft > 0 && timeLeft <= (lastCheckTime - timeForOne)) {
        itemsLeft--;
        lastCheckTime -= timeForOne;
        count++;
      }

      if (count > 0) {
        onItemTimerEnd?.Invoke(count);
      }
    }

    private void PrintTime() {
      var roundedTimeLeft = (float)Math.Round(timeLeft);
      timerText.text = Helper.SecondsToTimeString(roundedTimeLeft);
    }

    private void TimerTick() {
      timeLeft -= Time.deltaTime;

      CheckItemCompletion();
      PrintTime();

      if (timeLeft <= 0) {
        StopTimer();
      }
    }

    private void StopTimer() {
      onTimerStop?.Invoke();
    }

    public void Reset() {
      isStarted = false;
      timerText.text = string.Empty;
    }
  }
}