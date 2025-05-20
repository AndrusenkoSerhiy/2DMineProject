using System.Collections.Generic;
using PoolSound;
using Scriptables;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio {
  public class AudioController : MonoBehaviour {
    [SerializeField] private AudioListener listener;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Transform loopRoot;
    [SerializeField] private Transform shotRoot;
    [SerializeField] private SoundPooler soundPooler;
    
    public Transform GetRootTransform(AudioData.AudioTypeE type) {
      return type == AudioData.AudioTypeE.Looped ? loopRoot : shotRoot;
    }

    public void PlayAudio(AudioData audioData) {
      //Debug.LogError($"Audio data play {audioData.type}");
      soundPooler.SpawnFromPool(audioData, Vector3.zero, GetRootTransform(audioData.type));
    }

    public void SetMasterVolume(float volume) {
      mixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20f);
    }

    public void SetSoundFXVolume(float volume) {
      mixer.SetFloat("soundFXVolume", Mathf.Log10(volume) * 20f);
    }

    public void SetMusicVolume(float volume) {
      mixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20f);
    }
  }
}