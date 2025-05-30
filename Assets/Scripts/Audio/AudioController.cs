using System.Collections.Generic;
using PoolSound;
using Scriptables;
using UnityEngine;
using UnityEngine.Audio;
using System.Threading.Tasks;

namespace Audio {
  public class AudioController : MonoBehaviour {
    [SerializeField] private AudioListener listener;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Transform loopRoot;
    [SerializeField] private Transform shotRoot;
    [SerializeField] private SoundPooler soundPooler;

    [SerializeField] private AudioData mainTheme;
    [SerializeField] private AudioData siegeTheme;
    [SerializeField] private AudioData uiClick;
    [SerializeField] private AudioData craftClick;
    [SerializeField] private AudioData newRecipe;
    [SerializeField] private AudioData placeBuilding;
    [SerializeField] private AudioData takeBuilding;
    [SerializeField] private AudioData placeBuildingBlock;
    [SerializeField] private AudioData playerJump;
    [SerializeField] private AudioData playerJumpLanding;
    [SerializeField] private AudioData playerLeftStep;
    [SerializeField] private AudioData playerRightStep;
    [SerializeField] private List<AudioData> playerDamaged;
    [SerializeField] private AudioData playerDeath;

    public async Task PreloadAsync(AudioData audioData) {
      await soundPooler.PreloadAudioAsync(audioData, GetRootTransform(audioData.type));
    }

    public async Task PreloadSiegeThemeAsync() => await PreloadAsync(siegeTheme);

    public Transform GetRootTransform(AudioData.AudioTypeE type) {
      return type == AudioData.AudioTypeE.Looped ? loopRoot : shotRoot;
    }

    /// <summary>
    /// Plays the specified audio at the listener's position or at the origin if it's not 3D.
    /// </summary>
    /// <param name="audioData">The audio data to play.</param>
    public void PlayAudio(AudioData audioData) {
      if (!audioData) {
        return;
      }

      var position = audioData.is3D ? listener.transform.position : Vector3.zero;
      soundPooler.SpawnFromPool(audioData, position, GetRootTransform(audioData.type));
    }

    /// <summary>
    /// Plays the specified audio at the given position.
    /// </summary>
    /// <param name="audioData">The audio data to play.</param>
    /// <param name="position">The world position where the audio should be played.</param>
    public void PlayAudio(AudioData audioData, Vector3 position) {
      if (!audioData) {
        return;
      }

      soundPooler.SpawnFromPool(audioData, position, GetRootTransform(audioData.type));
    }

    /// <summary>
    /// Plays the specified audio and makes it follow the given transform.
    /// </summary>
    /// <param name="audioData">The audio data to play.</param>
    /// <param name="followTransform">The transform that the audio should follow.</param>
    public void PlayAudio(AudioData audioData, Transform followTransform) {
      if (!audioData || !followTransform) {
        return;
      }

      soundPooler.SpawnFromPool(audioData, followTransform, GetRootTransform(audioData.type));
    }

    public void PlayMainTheme() => PlayAudio(mainTheme);
    public void StopMainTheme() => StopAudio(mainTheme);
    public void PauseMainTheme() => PauseAudio(mainTheme);
    public void ResumeMainTheme() => ResumeAudio(mainTheme);

    public void PlaySiegeTheme() => PlayAudio(siegeTheme);
    public void StopSiegeTheme() => StopAudio(siegeTheme);
    public void PauseSiegeTheme() => PauseAudio(siegeTheme);
    public void ResumeSiegeTheme() => ResumeAudio(siegeTheme);

    public void PlayUIClick() => PlayAudio(uiClick);
    public void PlayCraftClick() => PlayAudio(craftClick);
    public void PlayNewRecipe() => PlayAudio(newRecipe);
    public void PlayPlaceBuilding() => PlayAudio(placeBuilding);
    public void PlayTakeBuilding() => PlayAudio(takeBuilding);
    public void PlayPlaceBuildingBlock() => PlayAudio(placeBuildingBlock);
    public void PlayPlayerJump() => PlayAudio(playerJump);
    public void PlayPlayerJumpLanding() => PlayAudio(playerJumpLanding);
    public void PlayPlayerLeftStep() {
      if (!GameManager.Instance.PlayerController.Grounded)
        return;
      
      PlayAudio(playerLeftStep);
    }

    public void PlayPlayerRightStep() {
      if (!GameManager.Instance.PlayerController.Grounded)
        return;
      
      PlayAudio(playerRightStep);
    }

    public void PlayPlayerDamaged() => PlayAudio(playerDamaged[Random.Range(0, playerDamaged.Count)]);
    public void PlayPlayerDeath() => PlayAudio(playerDeath);
    public void StopPlayerDeath() => StopAudio(playerDeath);

    public void StopAudio(AudioData data) {
      if (!data) {
        return;
      }

      soundPooler.StopAudio(data);
    }

    public void StopAllAudio() => soundPooler.StopAllAudio();

    public void PauseAudio(AudioData data) {
      if (!data) {
        return;
      }

      soundPooler.PauseAudio(data);
    }

    public void PauseAllAudio() => soundPooler.PauseAllAudio();

    public void ResumeAudio(AudioData data) {
      if (!data) {
        return;
      }

      soundPooler.ResumeAudio(data);
    }

    public void ResumeAllAudio() => soundPooler.ResumeAllAudio();

    public void SetMasterVolume(float v) => mixer.SetFloat("masterVolume", Mathf.Log10(v) * 20f);
    public void SetSoundFXVolume(float v) => mixer.SetFloat("soundFXVolume", Mathf.Log10(v) * 20f);
    public void SetMusicVolume(float v) => mixer.SetFloat("musicVolume", Mathf.Log10(v) * 20f);
  }
}