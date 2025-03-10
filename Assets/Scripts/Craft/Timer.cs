using System;
using System.Collections;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityServiceLocator;

namespace Craft {
  public class Timer : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI timerText;

    private Workstation station;
    private Recipe recipe;
    private int timeForOneInMilliseconds;
    private int totalItems;
    private int itemsLeft;
    private int totalTimeInMilliseconds;

    private bool isStarted;
    private Coroutine timerCoroutine;

    public Action onTimerStop;
    public Action<int> onItemTimerEnd;
    public bool IsStarted => isStarted;

    public void Awake() {
      station = ServiceLocator.For(this).Get<Workstation>();
    }

    private IEnumerator TimerCoroutine() {
      while (isStarted && station.MillisecondsLeft > 0) {
        yield return null;

        station.MillisecondsLeft -= (long)(Time.deltaTime * 1000);

        var timeLeftForCurrentInMilliseconds = station.MillisecondsLeft - ((itemsLeft - 1) * timeForOneInMilliseconds);
        station.UpdateProgress(timeLeftForCurrentInMilliseconds);

        CheckItemCompletion();
        PrintTime();
      }

      if (itemsLeft <= 0) {
        StopTimer();
      }
      else {
        SetTimerToCurrentItems();
      }
    }

    public void CheckTimer() {
      if (isStarted || recipe == null) {
        return;
      }

      StartTimer();
    }

    public void InitTimer(int count, Recipe recipe) {
      this.recipe = recipe;
      totalItems = count;
      timeForOneInMilliseconds = recipe.CraftingTime * 1000;
      totalTimeInMilliseconds = count * recipe.CraftingTime * 1000;
      itemsLeft = totalItems;
    }

    public void StartTimer() {
      isStarted = station.HaveFuelForCraft(recipe);
      
      if (station.MillisecondsLeft <= 0 || !isStarted) {
        SetTimerToCurrentItems();
      }

      var timeLeftForCurrentInMilliseconds = Math.Min(station.MillisecondsLeft, timeForOneInMilliseconds);
      station.SetProgress(timeForOneInMilliseconds, timeLeftForCurrentInMilliseconds);

      PrintTime();

      if (timerCoroutine != null) {
        StopCoroutine(timerCoroutine);
      }

      if (!isStarted) {
        return;
      }

      if (station.MillisecondsLeft != totalTimeInMilliseconds) {
        station.UpdateMillisecondsLeft(recipe, itemsLeft);
      }
      else {
        UpdateStartTimeByCurrent();
      }

      timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    public void Reset() {
      isStarted = false;
      timerText.text = string.Empty;

      // station.ResetMillisecondsLeft();
      station.ResetProgress();

      if (timerCoroutine != null) {
        StopCoroutine(timerCoroutine);
        timerCoroutine = null;
      }
    }

    private void CheckItemCompletion() {
      if (itemsLeft <= 0) {
        return;
      }

      var timeLeftWithoutCurrent = (itemsLeft - 1) * timeForOneInMilliseconds;

      if (station.MillisecondsLeft > timeLeftWithoutCurrent) {
        return;
      }

      itemsLeft--;
      SetTimerToCurrentItems();
      UpdateStartTimeByCurrent();
      onItemTimerEnd?.Invoke(1);

      if (!station.HaveFuelForCraft(recipe)) {
        isStarted = false;
      }
    }

    private void PrintTime() {
      var roundedTimeLeft = Mathf.Round((float)station.MillisecondsLeft / 1000);
      timerText.text = Helper.SecondsToTimeString(roundedTimeLeft);
    }

    private void StopTimer() {
      onTimerStop?.Invoke();
    }

    private void SetTimerToCurrentItems() {
      station.MillisecondsLeft = itemsLeft * recipe.CraftingTime * 1000;
    }

    private void UpdateStartTimeByCurrent() {
      station.CraftStartTimestampMillis = Helper.GetCurrentTimestampMillis();
    }
  }
}