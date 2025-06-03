using System.Collections;
using Audio;
using Inventory;
using Scriptables;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class Window : MonoBehaviour, IInventoryDropZoneUI {
    [SerializeField] private Button takeAllButton;
    [SerializeField] private bool preventItemDrop;
    [SerializeField] private UserInterface outputInterface;

    private Workstation station;

    public bool PreventItemDropIn => preventItemDrop;
    public Workstation Station => station;

    private AudioData music;
    private float timeBetweenMusic;
    private AudioEmmiter currentEmitter;

    public void Setup(Workstation station) {
      this.station = station;
      outputInterface.Setup(station.OutputInventoryType, station.Id);
    }

    public void Awake() {
      ServiceLocator.For(this).Register(station);
    }

    public void OnEnable() {
      AddEvents();
      PlayMusic();
    }

    private void OnDisable() {
      RemoveEvents();
      StopMusic();
    }

    private void AddEvents() {
      //craft output slots
      takeAllButton?.onClick.AddListener(OnTakeAllButtonClickHandler);
    }

    private void RemoveEvents() {
      //craft output slots
      takeAllButton?.onClick.RemoveAllListeners();
    }

    private void OnTakeAllButtonClickHandler() {
      station.MoveAllFromOutput();
      GameManager.Instance.AudioController.PlayUIClick();
    }

    private void PlayMusic() {
      if (!HasMusic()) {
        return;
      }

      music = GetRandomMusic();
      timeBetweenMusic = GetTimeBetweenMusic();

      currentEmitter = GameManager.Instance.AudioController.PlayAudio(music);
      currentEmitter.OnAudioEnd += OnMusicEndHandler;
    }

    private void OnMusicEndHandler() {
      currentEmitter.OnAudioEnd -= OnMusicEndHandler;

      StartCoroutine(PlayNextMusicAfterDelay());
    }

    private IEnumerator PlayNextMusicAfterDelay() {
      yield return new WaitForSeconds(timeBetweenMusic);
      PlayMusic();
    }

    private void StopMusic() {
      if (!HasMusic()) {
        return;
      }

      if (currentEmitter) {
        currentEmitter.OnAudioEnd -= OnMusicEndHandler;
        GameManager.Instance.AudioController.StopAudio(music);
        currentEmitter = null;
      }

      music = null;
      timeBetweenMusic = 0f;
    }

    private bool HasMusic() {
      return station?.WorkstationObject?.MusicAudioDatas?.Count > 0;
    }

    private float GetTimeBetweenMusic() {
      var secondsRange = station.WorkstationObject.SecondsBetweenMusic;
      return Random.Range(secondsRange.x, secondsRange.y);
    }

    private AudioData GetRandomMusic() {
      var musicList = station.WorkstationObject.MusicAudioDatas;
      return musicList[Random.Range(0, musicList.Count)];
    }
  }
}