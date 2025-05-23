using System.Collections.Generic;
using Audio;
using Scriptables;
using UnityEngine;

namespace PoolSound {
  public class SoundPooler : MonoBehaviour {
    [System.Serializable]
    public class PoolSound {
      public AudioData data;
      public AudioEmmiter audioEmmiter;
    }

    public List<PoolSound> pooledObjects;
    private Dictionary<AudioData, Queue<AudioEmmiter>> poolDictionary;

    private void Awake() {
      poolDictionary = new Dictionary<AudioData, Queue<AudioEmmiter>>();
    }

    public AudioEmmiter SpawnFromPool(AudioData data, Vector3 position, Transform parent) {
      AudioEmmiter objectToSpawn = null;
      var objectPool = new Queue<AudioEmmiter>();
      if (!poolDictionary.ContainsKey(data)) {
        //Debug.LogError("Pool with tag " + data + " doesn't exist.");
        var poolConfig = pooledObjects.Find(p => p.data == data);
        //Debug.LogError("poolConfig " + poolConfig);
        if (poolConfig != null) {
          objectPool = new Queue<AudioEmmiter>();
          var newObj = Instantiate(poolConfig.audioEmmiter.gameObject, parent);
          objectToSpawn = newObj.GetComponent<AudioEmmiter>();
          objectPool.Enqueue(objectToSpawn);
          poolDictionary.Add(data, objectPool);
        }
      }

      objectPool = poolDictionary[data];

      for (var i = 0; i < objectPool.Count; i++) {
        var obj = objectPool.Dequeue();

        if (!obj.gameObject.activeInHierarchy) {
          objectToSpawn = obj;
          break;
        }

        objectPool.Enqueue(obj);
      }

      if (!objectToSpawn) {
        var poolConfig = pooledObjects.Find(p => p.data == data);

        if (poolConfig != null) {
          var newObj = Instantiate(poolConfig.audioEmmiter.gameObject, parent);
          objectToSpawn = newObj.GetComponent<AudioEmmiter>();
        }
      }

      if (!objectToSpawn) {
        return null;
      }

      objectToSpawn.gameObject.SetActive(true);
      objectToSpawn.transform.position = position;

      var audioSource = objectToSpawn.audioSource;
      audioSource.clip = data.AudioClips[0];
      audioSource.outputAudioMixerGroup = data.mixerGroup;
      audioSource.volume = data.DecibelToLinear(data.volume);
      audioSource.loop = data.type == AudioData.AudioTypeE.Looped;

      audioSource.spatialBlend = data.is3D ? 1f : 0f;

      audioSource.Play();
      poolDictionary[data].Enqueue(objectToSpawn);

      return objectToSpawn;
    }

    public void PauseAudio(AudioData data) {
      if (!poolDictionary.ContainsKey(data)) {
        return;
      }

      foreach (var emitter in poolDictionary[data]) {
        if (emitter && emitter.audioSource.isPlaying) {
          emitter.audioSource.Pause();
        }
      }
    }

    public void PauseAllAudio() {
      foreach (var kvp in poolDictionary) {
        foreach (var emitter in kvp.Value) {
          if (emitter && emitter.audioSource.isPlaying) {
            emitter.audioSource.Pause();
          }
        }
      }
    }

    public void ResumeAudio(AudioData data) {
      if (!poolDictionary.ContainsKey(data)) {
        return;
      }

      foreach (var emitter in poolDictionary[data]) {
        if (emitter && emitter.audioSource && !emitter.audioSource.isPlaying && emitter.audioSource.time > 0f) {
          emitter.audioSource.UnPause();
        }
      }
    }

    public void ResumeAllAudio() {
      foreach (var kvp in poolDictionary) {
        foreach (var emitter in kvp.Value) {
          if (emitter && emitter.audioSource && !emitter.audioSource.isPlaying && emitter.audioSource.time > 0f) {
            emitter.audioSource.UnPause();
          }
        }
      }
    }

    public void StopAudio(AudioData data) {
      if (!poolDictionary.ContainsKey(data)) {
        return;
      }

      foreach (var emitter in poolDictionary[data]) {
        if (!emitter || !emitter.audioSource.isPlaying) {
          continue;
        }

        emitter.audioSource.Stop();
        emitter.gameObject.SetActive(false);
      }
    }

    public void StopAllAudio() {
      foreach (var kvp in poolDictionary) {
        foreach (var emitter in kvp.Value) {
          if (!emitter || !emitter.audioSource.isPlaying) {
            continue;
          }

          emitter.audioSource.Stop();
          emitter.gameObject.SetActive(false);
        }
      }
    }
  }
}