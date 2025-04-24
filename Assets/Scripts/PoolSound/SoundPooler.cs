using System.Collections.Generic;
using Audio;
using Pool;
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

    void Awake() {
      poolDictionary = new Dictionary<AudioData, Queue<AudioEmmiter>>();
    }

    public AudioEmmiter SpawnFromPool(AudioData data, Vector3 position, Transform parent) {
      AudioEmmiter objectToSpawn = null;
      Queue<AudioEmmiter> objectPool = new();
      if (!poolDictionary.ContainsKey(data)) {
        //Debug.LogError("Pool with tag " + data + " doesn't exist.");
        PoolSound poolConfig = pooledObjects.Find(p => p.data == data);
        //Debug.LogError("poolConfig " + poolConfig);
        if (poolConfig != null) {
          objectPool = new Queue<AudioEmmiter>();
          GameObject newObj = Instantiate(poolConfig.audioEmmiter.gameObject, parent);
          objectToSpawn = newObj.GetComponent<AudioEmmiter>();
          objectPool.Enqueue(objectToSpawn);
          poolDictionary.Add(data, objectPool);
        }
      }
      
      objectPool = poolDictionary[data];
      
      for (int i = 0; i < objectPool.Count; i++) {
        AudioEmmiter obj = objectPool.Dequeue();

        if (!obj.gameObject.activeInHierarchy) {
          objectToSpawn = obj;
          break;
        }

        objectPool.Enqueue(obj);
      }

      if (objectToSpawn == null) {
        PoolSound poolConfig = pooledObjects.Find(p => p.data == data);

        if (poolConfig != null) {
          GameObject newObj = Instantiate(poolConfig.audioEmmiter.gameObject, parent);
          objectToSpawn = newObj.GetComponent<AudioEmmiter>();
        }
      }

      objectToSpawn.gameObject.SetActive(true);
      objectToSpawn.transform.position = position;
      objectToSpawn.audioSource.clip = data.AudioClips[0];
      objectToSpawn.audioSource.outputAudioMixerGroup = data.mixerGroup;
      objectToSpawn.audioSource.volume = data.DecibelToLinear(data.volume);
      objectToSpawn.audioSource.Play();
      poolDictionary[data].Enqueue(objectToSpawn);

      return objectToSpawn;
    }
  }
}