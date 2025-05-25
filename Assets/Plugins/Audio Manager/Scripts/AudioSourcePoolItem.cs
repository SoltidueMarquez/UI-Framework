using System;
using UnityEngine;

namespace Audio_Manager
{
    [Serializable]
    public class AudioSourcePoolItem
    {
        public AudioSource audioSource;
        public string key;
        public float lastUsedTime;
        public Coroutine destroyCoroutine;

        public AudioSourcePoolItem(string key, AudioSource audioSource)
        {
            this.audioSource = audioSource;
            this.key = key;
            this.lastUsedTime = Time.time;
        }
    }
}