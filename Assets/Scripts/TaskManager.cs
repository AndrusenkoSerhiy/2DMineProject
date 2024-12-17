using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class TaskManager : MonoBehaviour {
  public async void DelayAsync(Action action, float seconds) {
    await Task.Delay((int)(seconds * 1000));
    action?.Invoke();
  }

  public void DelayCoroutine(Action action, float seconds) {
    StartCoroutine(DelayCoroutineTask(action, seconds));
  }

  IEnumerator DelayCoroutineTask(Action action, float seconds) {
    yield return new WaitForSeconds(seconds);
    action?.Invoke();
  }
}