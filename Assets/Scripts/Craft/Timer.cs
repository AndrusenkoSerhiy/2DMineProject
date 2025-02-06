using System;
using TMPro;
using UnityEngine;

namespace Craft {
  public class Timer : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI timerText;
    private DateTime startTime;
    private int timeForOne;
    private int totalItems;
    private int itemsLeft;
    private float totalTime;
    private float timeLeft;
    private float lastCheckTime;
    private bool isStarted;

    public Action onTimerStop;
    public Action<int> onItemTimerEnd;

    public void OnEnable() {
      //Debug.Log("Timer OnEnable");
      if (!isStarted) {
        return;
      }

      UpdateTimer();
    }

    public void OnDisable() {
      //Debug.Log("Timer OnDisable");
    }

    public void Update() {
      if (!isStarted) {
        return;
      }

      TimerTick();
    }

    public void InitTimer(int count, int time) {
      //Debug.Log("Timer StartTimer");
      startTime = DateTime.Now.ToUniversalTime();
      totalItems = count;
      timeForOne = time;
      totalTime = count * time;
      itemsLeft = totalItems;
      timeLeft = totalTime;
      lastCheckTime = totalTime;

      PrintTime();
    }

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

    private void UpdateTimer() {
      //Debug.Log("Timer UpdateTimer");
      var currentTime = DateTime.Now.ToUniversalTime();
      var elapsedTime = (float)(currentTime - startTime).TotalSeconds;
      timeLeft = totalTime - elapsedTime;
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
      Reset();
    }

    private void Reset() {
      //Debug.Log("Timer Reset");
      isStarted = false;
      timerText.text = string.Empty;
    }
  }
}