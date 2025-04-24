using System.Collections.Generic;
using PoolSound;
using Scriptables;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio {
  public class AudioController : MonoBehaviour {
    [SerializeField] private AudioListener listener;
    [SerializeField] private AudioMixer mixer;
    //[SerializeField] private List<AudioData> audioReferences;
    [SerializeField] private Transform loopRoot;
    [SerializeField] private Transform shotRoot;
    //[SerializeField] private AudioSource audioSource;
    [SerializeField] private SoundPooler soundPooler;
    
    public Transform GetRootTransform(AudioData.AudioTypeE type) {
      return type == AudioData.AudioTypeE.Looped ? loopRoot : shotRoot;
    }

    public void PlayAudio(AudioData audioData) {
      //Debug.LogError($"Audio data play {audioData.type}");
      soundPooler.SpawnFromPool(audioData, Vector3.zero, GetRootTransform(audioData.type));
    }
  }
}