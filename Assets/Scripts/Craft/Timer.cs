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
    private double totalTime;
    private double timeLeft;
    private double lastCheckTime;
    private bool isStarted;

    public Action onTimerStop;
    public Action<int> onItemTimerEnd;

    public void OnEnable() {
      Debug.Log("Timer OnEnable");
      if (!isStarted) {
        return;
      }

      UpdateTimer();
    }

    public void OnDisable() {
      Debug.Log("Timer OnDisable");
    }

    public void Update() {
      if (!isStarted) {
        return;
      }

      TimerTick();
    }

    public void StartTimer(int count, int time) {
      Debug.Log("Timer StartTimer");
      startTime = DateTime.Now.ToUniversalTime();
      totalTime = count * time;
      timeLeft = totalTime;
      totalItems = count;
      itemsLeft = totalItems;
      timeForOne = time;
      lastCheckTime = totalTime;

      isStarted = true;

      PrintTime();
    }

    private void CheckItemCompletion() {
      var count = 0;
      while (itemsLeft > 0 && timeLeft <= lastCheckTime - timeForOne) {
        itemsLeft--;
        lastCheckTime -= timeForOne;
        count++;
      }

      if (count > 0) {
        onItemTimerEnd?.Invoke(count);
      }
    }

    private void PrintTime() {
      timerText.text = Helper.SecondsToTimeString((int)timeLeft);
    }

    private void UpdateTimer() {
      Debug.Log("Timer UpdateTimer");
      var currentTime = DateTime.Now.ToUniversalTime();
      double elapsedTime = (currentTime - startTime).TotalSeconds;
      timeLeft = totalTime - elapsedTime;

      CheckItemCompletion();
    }

    private void TimerTick() {
      timeLeft -= Time.deltaTime;
      
      CheckItemCompletion();

      if (timeLeft > 0) {
        PrintTime();
        return;
      }

      StopTimer();
    }

    private void StopTimer() {
      Debug.Log("Timer StopTimer");
      onTimerStop?.Invoke();
      Reset();
      enabled = false;
    }

    private void Reset() {
      Debug.Log("Timer Reset");
      isStarted = false;
      timerText.text = string.Empty;
    }
  }
}