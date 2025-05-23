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
    [SerializeField] private AudioData mainTheme;

    public Transform GetRootTransform(AudioData.AudioTypeE type) {
      return type == AudioData.AudioTypeE.Looped ? loopRoot : shotRoot;
    }

    public void PlayAudio(AudioData audioData) {
      //Debug.LogError($"Audio data play {audioData.type}");
      soundPooler.SpawnFromPool(audioData, Vector3.zero, GetRootTransform(audioData.type));
    }

    public void PlayMainTheme() {
      PlayAudio(mainTheme);
    }

    public void StopMainTheme() {
      StopAudio(mainTheme);
    }

    public void PauseMainTheme() {
      PauseAudio(mainTheme);
    }

    public void ResumeMainTheme() {
      ResumeAudio(mainTheme);
    }

    public void StopAudio(AudioData audioData) {
      soundPooler.StopAudio(audioData);
    }

    public void StopAllAudio() {
      soundPooler.StopAllAudio();
    }

    public void PauseAudio(AudioData audioData) {
      soundPooler.PauseAudio(audioData);
    }

    public void PauseAllAudio() {
      soundPooler.PauseAllAudio();
    }

    public void ResumeAudio(AudioData audioData) {
      soundPooler.ResumeAudio(audioData);
    }

    public void ResumeAllAudio() {
      soundPooler.ResumeAllAudio();
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