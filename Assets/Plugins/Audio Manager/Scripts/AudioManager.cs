using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Audio_Manager
{
    public class AudioManager : PersistentSingleton<AudioManager>
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioSo audioSo;

        [Header("对象池配置")] [SerializeField, LabelText("基准容量")]
        private int basePoolCapacity = 5;

        [SerializeField, LabelText("闲置销毁时间")] private float idleDestroyTime = 10f;

        private Dictionary<string, AudioClip[]> musicDic;
        private Dictionary<string, AudioClip[]> soundDic;

        private List<AudioSourcePlus> musicAudioSources; //音乐的播放器列表
        private List<AudioSourcePlus> soundAudioSources; //音效的播放器列表

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

        private AudioSourcePlus GetAudioSource(AudioType type, string key)
        {
            var audioList = type == AudioType.Sound ? soundAudioSources : musicAudioSources;

            // 优先查找可复用的播放器
            foreach (var audioSourcePlus in audioList.Where(a => !a.audioSource.isPlaying))
            {
                audioSourcePlus.key = key;
                audioSourcePlus.lastUsedTime = Time.time;
                if (audioSourcePlus.destroyCoroutine != null)
                {
                    StopCoroutine(audioSourcePlus.destroyCoroutine);
                    audioSourcePlus.destroyCoroutine = null;
                }

                return audioSourcePlus;
            }

            // // 超出基准容量时尝试清理闲置项
            // if (audioList.Count >= basePoolCapacity)
            // {
            //     var oldest = audioList
            //         .Where(a => !a.audioSource.isPlaying)
            //         .OrderBy(a => a.lastUsedTime)
            //         .FirstOrDefault();
            //
            //     if (oldest != null)
            //     {
            //         Destroy(oldest.audioSource.gameObject);
            //         audioList.Remove(oldest);
            //     }
            // }

            // 创建新实例
            return AddAudioSource(type, key);
        }

        //添加播放器
        private AudioSourcePlus AddAudioSource(AudioType type, string key)
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
            return audioSourcePlus;
        }

        #endregion

        #region 对象池辅助

        private IEnumerator DelayedDestroyCheck(AudioSourcePlus audioSourcePlus)
        {
            yield return new WaitForSeconds(idleDestroyTime);

            // 再次检查是否仍然闲置
            if (!audioSourcePlus.audioSource.isPlaying && Time.time - audioSourcePlus.lastUsedTime >= idleDestroyTime)
            {
                // 根据类型移除并销毁
                if (soundAudioSources.Remove(audioSourcePlus))
                    Destroy(audioSourcePlus.audioSource.gameObject);
                else if (musicAudioSources.Remove(audioSourcePlus))
                    Destroy(audioSourcePlus.audioSource.gameObject);
            }

            audioSourcePlus.destroyCoroutine = null;
        }

        private void TryStartDestroyTimer(AudioSourcePlus audioSourcePlus)
        {
            if (audioSourcePlus.destroyCoroutine != null)
            {
                StopCoroutine(audioSourcePlus.destroyCoroutine);
            }

            audioSourcePlus.destroyCoroutine = StartCoroutine(DelayedDestroyCheck(audioSourcePlus));
        }

        #endregion

        #region 封装的播放逻辑，嵌套定时销毁回收

        // 修改播放逻辑，在播放完成后触发计时器
        private void SealedPlay(AudioType type, AudioSourcePlus plus)
        {
            var audioList = type == AudioType.Sound ? soundAudioSources : musicAudioSources;
            plus.audioSource.Play();
            // 超出容量，选择定时回收
            if (audioList.Count > basePoolCapacity)
            {
                StartCoroutine(PlayAndAutoRecycle(plus));
            }
        }

        private IEnumerator PlayAndAutoRecycle(AudioSourcePlus plus)
        {
            while (plus.audioSource.isPlaying)
            {
                yield return null;
            }

            TryStartDestroyTimer(plus);
        }

        #endregion

        #region 音效播放

        public void PlaySound(string mName, bool ifRandom = false)
        {
            var clip = GetASound(mName); //获取音频
            if (clip == null)
            {
                Debug.LogError($"音效缺失:{mName}");
                return;
            }

            //获取播放器
            var audioSourcePlus = GetAudioSource(AudioType.Sound, mName);
            //播放音频
            audioSourcePlus.audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
            audioSourcePlus.audioSource.clip = clip;
            audioSourcePlus.audioSource.loop = false;

            SealedPlay(AudioType.Sound, audioSourcePlus);
        }

        private AudioClip GetASound(string soundName)
        {
            return !soundDic.ContainsKey(soundName)
                ? null
                : soundDic[soundName][Random.Range(0, soundDic[soundName].Length)];
        }

        #endregion

        #region 音乐播放

        public void PlayMusic(string mName, bool ifLoop = false)
        {
            if (ifLoop) //如果循环播放的话需要先停止原先的再重新开始
            {
                StopMusic(mName);
            }

            var clip = GetAMusic(mName); //获取音频
            if (clip == null)
            {
                Debug.LogError($"音乐缺失:{mName}");
                return;
            }

            var audioSourcePlus = GetAudioSource(AudioType.Music, mName); //获取播放器
            audioSourcePlus.audioSource.clip = clip;
            audioSourcePlus.audioSource.loop = ifLoop;
            SealedPlay(AudioType.Music, audioSourcePlus);
        }

        private AudioClip GetAMusic(string musicName)
        {
            return !musicDic.ContainsKey(musicName)
                ? null
                : musicDic[musicName][Random.Range(0, musicDic[musicName].Length)];
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
}