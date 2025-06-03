using System;
using UnityEngine;

namespace Audio {
  public class AudioEmmiter : MonoBehaviour {
    public AudioSource audioSource;
    public event Action OnAudioEnd;

    private bool wasPlaying = false;

    private void Update() {
      if (!audioSource) {
        return;
      }

      if (audioSource.loop) {
        return;
      }

      if (audioSource.isPlaying && !wasPlaying) {
        wasPlaying = true;
      }
      else if (!audioSource.isPlaying && wasPlaying) {
        wasPlaying = false;
        gameObject.SetActive(false);
        OnAudioEnd?.Invoke();
      }
    }
  }
}