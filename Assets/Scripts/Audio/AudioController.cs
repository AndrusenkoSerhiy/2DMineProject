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
    [SerializeField] private AudioData shoot;
    [SerializeField] private AudioData placeBuilding;
    [SerializeField] private AudioData takeBuilding;
    [SerializeField] private AudioData placeBuildingBlock;
    [SerializeField] private AudioData playerJump;
    [SerializeField] private AudioData playerJumpLanding;
    [SerializeField] private AudioData playerLeftStep;
    [SerializeField] private AudioData playerRightStep;

    public async Task PreloadAsync(AudioData audioData) {
      await soundPooler.PreloadAudioAsync(audioData, GetRootTransform(audioData.type));
    }

    public async Task PreloadSiegeThemeAsync() => await PreloadAsync(siegeTheme);

    public Transform GetRootTransform(AudioData.AudioTypeE type) {
      return type == AudioData.AudioTypeE.Looped ? loopRoot : shotRoot;
    }

    public void PlayAudio(AudioData audioData) {
      if (!audioData) {
        return;
      }

      var position = audioData.is3D ? listener.transform.position : Vector3.zero;
      soundPooler.SpawnFromPool(audioData, position, GetRootTransform(audioData.type));
    }

    public void PlayAudio(AudioData audioData, Vector3 position) {
      if (!audioData) {
        return;
      }

      soundPooler.SpawnFromPool(audioData, position, GetRootTransform(audioData.type));
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
    public void PlayShoot() => PlayAudio(shoot);
    public void PlayPlaceBuilding() => PlayAudio(placeBuilding);
    public void PlayTakeBuilding() => PlayAudio(takeBuilding);
    public void PlayPlaceBuildingBlock() => PlayAudio(placeBuildingBlock);
    public void PlayPlayerJump() => PlayAudio(playerJump);
    public void PlayPlayerJumpLanding() => PlayAudio(playerJumpLanding);
    public void PlayPlayerLeftStep() => PlayAudio(playerLeftStep);
    public void PlayPlayerRightStep() => PlayAudio(playerRightStep);

    public void StopAudio(AudioData data) => soundPooler.StopAudio(data);
    public void StopAllAudio() => soundPooler.StopAllAudio();
    public void PauseAudio(AudioData data) => soundPooler.PauseAudio(data);
    public void PauseAllAudio() => soundPooler.PauseAllAudio();
    public void ResumeAudio(AudioData data) => soundPooler.ResumeAudio(data);
    public void ResumeAllAudio() => soundPooler.ResumeAllAudio();

    public void SetMasterVolume(float v) => mixer.SetFloat("masterVolume", Mathf.Log10(v) * 20f);
    public void SetSoundFXVolume(float v) => mixer.SetFloat("soundFXVolume", Mathf.Log10(v) * 20f);
    public void SetMusicVolume(float v) => mixer.SetFloat("musicVolume", Mathf.Log10(v) * 20f);
  }
}