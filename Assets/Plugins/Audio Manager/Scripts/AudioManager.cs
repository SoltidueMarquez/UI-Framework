using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Audio_Manager
{
    public class AudioManager : PersistentSingleton<AudioManager>
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioSo audioSo;

        private Dictionary<string, AudioClip[]> musicDic;
        private Dictionary<string, AudioClip[]> soundDic;

        [Tooltip("音乐的播放器列表")] private List<AudioSourcePlus> musicAudioSources;
        [Tooltip("音效的播放器列表")] private List<AudioSourcePlus> soundAudioSources;

        private Vector2 pitchRange = Vector2.one;

        private void Start()
        {
            Init();
        }

        //初始化
        public void Init()
        {
            musicDic = audioSo.musicClips;
            soundDic = audioSo.soundClips;
            pitchRange = audioSo.pitchRange;
            musicAudioSources = new List<AudioSourcePlus>();
            soundAudioSources = new List<AudioSourcePlus>();

            SetMasterVolume(audioSo.masterVolume);
            SetMusicVolume(audioSo.musicVolume);
            SetSfxVolume(audioSo.soundVolume);
        }
    
        #region 音量设置
        //AudioMixer音量设置
        public void SetMasterVolume(float value)
        {
            //PlayerPrefs.SetFloat("master",value);
            audioSo.masterVolume = value;
            var tmp = value * 40 - 40;
            audioMixer.SetFloat("vMaster", tmp);
        }
        public void SetMusicVolume(float value)
        {
            //PlayerPrefs.SetFloat("music",value);
            audioSo.musicVolume = value;
            var tmp = value * 40 - 40;
            audioMixer.SetFloat("vMusic", tmp);
        }
        public void SetSfxVolume(float value)
        {
            //PlayerPrefs.SetFloat("sound",value);
            audioSo.soundVolume = value;
            var tmp = value * 40 - 40;
            audioMixer.SetFloat("vSound", tmp);
        }
        #endregion
        
        #region 获取AudioResource
        private AudioSource GetAudioSource(AudioType type, string key)
        {
            var audioList = new List<AudioSourcePlus>();
            switch (type)
            {
                case AudioType.Sound:
                    audioList = soundAudioSources;
                    break;
                case AudioType.Music:
                    audioList = musicAudioSources;
                    break;
            }
            foreach (var audioSourcePlus in audioList.Where(audioSourcePlus => !audioSourcePlus.audioSource.isPlaying))
            {
                audioSourcePlus.audioSource.loop = false;
                audioSourcePlus.key = key;
                return audioSourcePlus.audioSource;
            }
            return AddAudioSource(type, key);
        }
        //添加播放器
        private AudioSource AddAudioSource(AudioType type, string key)
        {
            var audioList = (type == AudioType.Sound)
                ? soundAudioSources
                : musicAudioSources;
            var prefab = (type == AudioType.Sound)
                ? audioSo.soundAudioSourcePrefab
                : audioSo.musicAudioSourcePrefab;
            var tmp = Instantiate(prefab, this.transform).GetComponent<AudioSource>();
            var audioSourcePlus = new AudioSourcePlus(key, tmp);
            audioList.Add(audioSourcePlus);
            return tmp;
        }
        #endregion
    
        #region 音效播放
        public void PlaySound(string mName, bool ifRandom = false)
        {
            var clip = GetASound(mName);//获取音频
            if (clip == null)
            {
                Debug.LogError($"音效缺失:{mName}");
                return;
            }
            var audioSource = GetAudioSource(AudioType.Sound, mName);//获取播放器
            //播放音频
            audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.Play();
        }
        private AudioClip GetASound(string soundName)
        {
            return !soundDic.ContainsKey(soundName) ? null : soundDic[soundName][Random.Range(0, soundDic[soundName].Length)];
        }
        #endregion
        
        #region 音乐播放
        public void PlayMusic(string mName, bool ifLoop = false)
        {
            if (ifLoop)//如果循环播放的话需要先停止原先的再重新开始
            {
                StopMusic(mName);
            }
            
            var clip = GetAMusic(mName);//获取音频
            if (clip == null)
            {
                Debug.LogError($"音乐缺失:{mName}");
                return;
            }

            var audioSource = GetAudioSource(AudioType.Music, mName);//获取播放器
            audioSource.clip = clip;
            audioSource.loop = ifLoop;
            audioSource.Play();
        }
        private AudioClip GetAMusic(string musicName)
        {
            return !musicDic.ContainsKey(musicName) ? null : musicDic[musicName][Random.Range(0, musicDic[musicName].Length)];
        }
        #endregion

        #region 音乐停止与暂停
        public void StopMusic(string mName)
        {
            foreach (var audioSourcePlus in musicAudioSources.Where(audioSourcePlus => audioSourcePlus.key == mName))
            {
                audioSourcePlus.audioSource.Stop();
            }
        }

        public void PauseMusic(string mName)
        {
            foreach (var audioSourcePlus in musicAudioSources.Where(audioSourcePlus => audioSourcePlus.key == mName))
            {
                audioSourcePlus.audioSource.Pause();
            }
        }
        
        public void ContinueMusic(string mName)
        {
            foreach (var audioSourcePlus in musicAudioSources.Where(audioSourcePlus => audioSourcePlus.key == mName))
            {
                audioSourcePlus.audioSource.Play();
            }
        }

        public void StopSound(string mName)
        {
            foreach (var audioSourcePlus in soundAudioSources.Where(audioSourcePlus => audioSourcePlus.key == mName))
            {
                audioSourcePlus.audioSource.Stop();
            }
        }

        public void StopAllMusic()
        {
            foreach (var audioSourcePlus in musicAudioSources)
            {
                audioSourcePlus.audioSource.Stop();
            }
        }
        #endregion
    }

    public enum AudioType
    {
        Sound,
        Music
    }
    
    [Serializable]
    public class AudioSourcePlus
    {
        public AudioSource audioSource;
        public string key;

        public AudioSourcePlus(string key, AudioSource audioSource)
        {
            this.audioSource = audioSource;
            this.key = key;
        }
    }
}