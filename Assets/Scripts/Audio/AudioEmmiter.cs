using System;
using UnityEngine;

namespace Audio {
  public class AudioEmmiter : MonoBehaviour {
    public AudioSource audioSource;

    private void Update() {
      //check sound doesnt playing
      if (!audioSource.isPlaying && !audioSource.loop) {
        gameObject.SetActive(false);
      }
    }
  }
}