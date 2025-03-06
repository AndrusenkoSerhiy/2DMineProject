using System;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityServiceLocator;

namespace Craft {
  public class Timer : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI timerText;

    private Workstation station;
    private int timeForOneInMilliseconds;
    private int totalItems;
    private int itemsLeft;
    private int totalTimeInMilliseconds;

    private bool isStarted;

    public Action onTimerStop;
    public Action<int> onItemTimerEnd;
    public bool IsStarted => isStarted;

    public void Awake() {
      station = ServiceLocator.For(this).Get<Workstation>();
    }

    public void Update() {
      if (!isStarted) {
        return;
      }

      TimerTick();
    }

    public void InitTimer(int count, int time) {
      totalItems = count;
      timeForOneInMilliseconds = time * 1000;
      totalTimeInMilliseconds = count * time * 1000;
      itemsLeft = totalItems;

      PrintTime();
    }

    public void StartTimer() {
      isStarted = true;
      if (station.MillisecondsLeft <= 0) {
        station.MillisecondsLeft = totalTimeInMilliseconds;
        station.CraftStartTimestampMillis = Helper.GetCurrentTimestampMillis();
      }

      var timeLeftForCurrentInMilliseconds = Math.Min(station.MillisecondsLeft, timeForOneInMilliseconds);
      station.SetProgress(timeForOneInMilliseconds, timeLeftForCurrentInMilliseconds);
    }

    public void Reset() {
      isStarted = false;
      timerText.text = string.Empty;

      station.ResetMillisecondsLeft();
      station.ResetProgress();
    }

    private void CheckItemCompletion() {
      if (itemsLeft <= 0) {
        return;
      }

      var timeLeftWithoutCurrent = (itemsLeft - 1) * timeForOneInMilliseconds;

      if (station.MillisecondsLeft <= timeLeftWithoutCurrent) {
        itemsLeft--;
        onItemTimerEnd?.Invoke(1);
      }
    }

    private void PrintTime() {
      var roundedTimeLeft = Mathf.Round((float)station.MillisecondsLeft / 1000);
      timerText.text = Helper.SecondsToTimeString(roundedTimeLeft);
    }

    private void TimerTick() {
      station.MillisecondsLeft -= (long)(Time.deltaTime * 1000);

      var timeLeftForCurrentInMilliseconds = station.MillisecondsLeft - ((itemsLeft - 1) * timeForOneInMilliseconds);
      station.UpdateProgress(timeLeftForCurrentInMilliseconds);

      CheckItemCompletion();
      PrintTime();

      if (station.MillisecondsLeft <= 0) {
        StopTimer();
      }
    }

    private void StopTimer() {
      onTimerStop?.Invoke();
    }
  }
}