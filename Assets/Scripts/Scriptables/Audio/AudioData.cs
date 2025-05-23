using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Scriptables {
  [CreateAssetMenu(menuName = "ScriptableData/Audio/AudioData", fileName = "AudioData")]
  public class AudioData : ScriptableObject {
    public enum AudioTypeE {
      OneShot = 0,
      Looped = 1,
    }
    
    public AudioTypeE type = AudioTypeE.OneShot;
    public AudioMixerGroup mixerGroup;
    [SerializeField] private List<AudioClip> audioClips = new();
    [Tooltip("set volume in dB")]
    public float volume;
    
    public float DecibelToLinear(float dB) {
      return Mathf.Pow(10f, dB / 20f);
    }
    public List<AudioClip> AudioClips => audioClips;
    public bool is3D = true;
  }
}