using System;
using System.Collections.Generic;
using Actors;
using UnityEngine;

namespace Craft {
  public class TrapDamage : MonoBehaviour {
    [SerializeField] private Trap trapBase;
    [SerializeField] private int damagePerSecond;
    [SerializeField] private int durabilityPerDamage;
    private List<ActorEnemy> zombiesTraped = new List<ActorEnemy>();
    private bool damaging = false;
    private float damageTimer = 1f; // timer accumulator
    private float damageInterval = 1f; // 1 second interval

    private void Update() {
      if (zombiesTraped.Count > 0) {
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval) {
          damageTimer = 0f; // reset timer
          Damage();
        }
      }
      else {
        damageTimer = 0f;
      }
    }

    private void Damage() {
      damaging = false;
      for (int i = 0; i < zombiesTraped.Count; i++) {
        if (zombiesTraped[i] != null) {
          zombiesTraped[i].Damage(damagePerSecond, true);
          damaging = true;
        }
      }

      if (damaging) {
        trapBase.Damage(durabilityPerDamage, false);
      }

      zombiesTraped.RemoveAll(a => a == null);
    }

    public void OnTriggerEnter2D(Collider2D other) {
      var actor = other.GetComponent<ActorEnemy>();
      if (actor) {
        zombiesTraped.Add(actor);
      }
    }

    public void OnTriggerExit2D(Collider2D other) {
      var actor = other.GetComponent<ActorEnemy>();
      if (actor) {
        zombiesTraped.Remove(actor);
      }
    }
  }
}