using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Audio_Manager
{
    [CreateAssetMenu]
    public class AudioSo : SerializedScriptableObject
    {
        [Header("资源列表")] 
        [LabelText("音乐字典")] public Dictionary<string, AudioClip[]> musicClips;
        [LabelText("音效字典")] public Dictionary<string, AudioClip[]> soundClips;


        [Header("设置")] 
        [LabelText("随机音调范围")] public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

        public GameObject musicAudioSourcePrefab;
        public GameObject soundAudioSourcePrefab;

        [Header("音量")] [LabelText("主音量")] public float masterVolume;
        [LabelText("音乐音量")] public float musicVolume;
        [LabelText("音效音量")] public float soundVolume;
    }
}