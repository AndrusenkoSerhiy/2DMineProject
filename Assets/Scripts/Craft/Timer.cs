using System;
using TMPro;
using UnityEngine;

namespace Craft {
  public class Timer : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI timerText;
    private DateTime startTime;
    private DateTime endTime;
    private int timeForOne;
    private int totalItems;
    private int itemsLeft;
    private float totalTime;
    private float timeLeft;
    private float lastCheckTime;
    private bool isStarted;

    public Action onTimerStop;
    public Action<int> onItemTimerEnd;
    public bool IsStarted => isStarted;

    public void OnEnable() {
      UpdateTimer();
    }

    private void UpdateTimer() {
      //Debug.Log("Timer UpdateTimer");
      var currentTime = DateTime.Now.ToUniversalTime();
      var elapsedTime = (float)(currentTime - startTime).TotalSeconds;
      timeLeft = totalTime - elapsedTime;
    }

    public void Update() {
      if (!isStarted) {
        return;
      }

      TimerTick();
    }

    public void InitTimer(int count, int time, DateTime? start = null) {
      //Debug.Log("Timer StartTimer");
      startTime = start ?? DateTime.Now.ToUniversalTime();
      totalItems = count;
      timeForOne = time;
      totalTime = count * time;
      itemsLeft = totalItems;
      timeLeft = totalTime;
      lastCheckTime = totalTime;
      endTime = startTime.AddSeconds(totalTime);

      PrintTime();
    }

    public DateTime GetEndTime() => endTime;
    public DateTime GetStartTime() => startTime;

    public void StartTimer() {
      isStarted = true;
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
      //Debug.Log("Timer StopTimer");
      onTimerStop?.Invoke();
    }

    public void Reset() {
      //Debug.Log("Timer Reset");
      isStarted = false;
      timerText.text = string.Empty;
    }
  }
}