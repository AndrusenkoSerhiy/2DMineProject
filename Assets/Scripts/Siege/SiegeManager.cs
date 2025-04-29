using System;
using System.Collections;
using System.Collections.Generic;
using Scriptables.Siege;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Siege {
  public class SiegeManager : MonoBehaviour {
    [SerializeField] private SiegesSettings siegesSettings;

    public event Action<ActiveSiegeTemplate> OnSiegeStarted;
    public event Action<ActiveSiegeTemplate> OnSiegeEnded;
    public event Action<ActiveSiegeTemplate> OnZombieSpawn;

    private List<SiegeTemplate> siegeQueue = new();
    private int currentSiegeСycle = 1;
    private int currentSiegeIndex = 0;
    private bool siegesStarted = false;
    private float siegeTimer = 0f;
    private ActiveSiegeTemplate currentSiege;

    public void StartSieges() {
      if (siegesStarted) {
        return;
      }

      siegeQueue.Clear();
      siegeQueue.Add(siegesSettings.FirstSiege);

      var maxRandomSiegesCount = siegesSettings.SiegesCountMax - 2; //max - first - final
      var randomSiegesCount = Random.Range(siegesSettings.SiegesCountMin, maxRandomSiegesCount + 1);

      for (var i = 0; i < randomSiegesCount; i++) {
        var randomTemplate =
          siegesSettings.RandomSiegeTemplates[Random.Range(0, siegesSettings.RandomSiegeTemplates.Count)];
        siegeQueue.Add(randomTemplate);
      }

      siegesStarted = true;
      StartCoroutine(RunNextSiege());
    }

    private IEnumerator RunNextSiege() {
      if (currentSiegeIndex >= siegeQueue.Count) {
        Debug.Log("All sieges completed. Restarting...");
        siegesStarted = false;
        currentSiegeIndex = 0;
        currentSiegeСycle++;
        StartSieges();
        yield break;
      }

      currentSiege = new ActiveSiegeTemplate(siegeQueue[currentSiegeIndex]);

      OnSiegeStarted?.Invoke(currentSiege);

      var spawnTimer = 0f;
      var siegeDurationTimer = 0f;

      while (siegeDurationTimer < currentSiege.Duration) {
        siegeDurationTimer += Time.deltaTime;
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= currentSiege.SpawnInterval) {
          spawnTimer = 0f;

          OnZombieSpawn?.Invoke(currentSiege);
        }

        yield return null;
      }

      OnSiegeEnded?.Invoke(currentSiege);

      currentSiegeIndex++;

      yield return new WaitForSeconds(currentSiege.TimeBetweenSiege);
      StartCoroutine(RunNextSiege());
    }

    private void SkipCurrentSiege() {
      if (currentSiege == null) {
        return;
      }

      StopAllCoroutines();
      currentSiege = null;
      OnSiegeEnded?.Invoke(currentSiege);
      currentSiegeIndex++;

      if (currentSiegeIndex < siegeQueue.Count) {
        StartCoroutine(RunNextSiege());
      }
    }
  }
}