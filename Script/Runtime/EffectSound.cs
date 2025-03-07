using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace qbot.Sound
{
    public class EffectSound : MonoBehaviour
    {
        private const string EffectSoundResourceRootPath = "Sound/EffectSound/";
        private const string IsEffectSoundOn = nameof(IsEffectSoundOn);
        private const string EffectSoundVolume = nameof(EffectSoundVolume);
        private const int DefaultAudioSourceCount = 4;

        /// <summary>
        /// Decide whether to use the class as a singleton pattern.
        /// </summary>
        [Header("[Singleton]")]
        [SerializeField]
        private bool _isSingleton;

        /// <summary>
        /// Deciding whether to make the class a DontDestroyOnLoad GameObject.
        /// </summary>
        [SerializeField]
        private bool _isDontDestroyOnLoad;

        private float _effectSoundVolume;

        private List<AudioSource> _effectSoundAudioSources;
        private List<AudioSource> _pausedEffectSoundAudioSources;
        private Dictionary<string, AudioClip> _effectSoundAudioClips;

        public static EffectSound Instance { get; private set; }

        /// <summary>
        /// If true, the effect sound is played.
        /// if false, the effect sound is not played. 
        /// </summary>
        public bool IsEffectSoundEnabled { get; private set; }

        private List<AudioSource> EffectSoundAudioSources
        {
            get
            {
                _effectSoundAudioSources ??= new List<AudioSource>();
                return _effectSoundAudioSources;
            }
        }

        private List<AudioSource> PausedEffectSoundAudioSources
        {
            get
            {
                _pausedEffectSoundAudioSources ??= new List<AudioSource>();
                return _pausedEffectSoundAudioSources;
            }
        }
        
        private Dictionary<string, AudioClip> EffectSoundAudioClips
        {
            get
            {
                _effectSoundAudioClips ??= new Dictionary<string, AudioClip>();
                return _effectSoundAudioClips;
            }
        }
        
        private void Awake()
        {
            if (_isSingleton)
            {
                if (Instance == null)
                {
                    if (_isDontDestroyOnLoad)
                    {
                        DontDestroyOnLoad(gameObject);
                    }

                    Instance = this;
                }
                else
                {
                    Destroy(this);
                }
            }

            IsEffectSoundEnabled = PlayerPrefs.GetInt(IsEffectSoundOn, 1) == 1;
            _effectSoundVolume = PlayerPrefs.GetFloat(EffectSoundVolume, 1.0f);
        }

        /// <summary>
        /// Clear all audio
        /// </summary>
        public void ClearAllAudioSources()
        {
            foreach (var audioSource in _effectSoundAudioSources)
            {
                Destroy(audioSource);
            }
            
            _effectSoundAudioSources.Clear();
        }

        /// <summary>
        /// Play the effect sound.
        /// </summary>
        /// <param name="effectSoundResourceName">Resource name of the effect sound to play</param>
        /// <returns></returns>
        public int? Play(string effectSoundResourceName)
        {
            if (IsEffectSoundEnabled == false)
                return null;

            var effectSoundResourcePath = GetEffectSoundResourceDirectoryPath(effectSoundResourceName);
            if (EffectSoundAudioClips.ContainsKey(effectSoundResourcePath) == false)
            {
                var audioClip = Resources.Load<AudioClip>(effectSoundResourcePath);
                if (audioClip == null)
                {
                    Debug.LogError($"{effectSoundResourceName} is does not exist.");
                    return null;
                }

                EffectSoundAudioClips[effectSoundResourcePath] = audioClip;
            }

            var audioSourceIndex = GetAvailableAudioSourceIndex();

            EffectSoundAudioSources[audioSourceIndex].clip = EffectSoundAudioClips[effectSoundResourcePath];
            EffectSoundAudioSources[audioSourceIndex].Play();
            EffectSoundAudioSources[audioSourceIndex].volume = _effectSoundVolume;

            return audioSourceIndex;
        }

        /// <summary>
        /// Play the effect sound repeatedly.
        /// </summary>
        /// <param name="effectSoundResourceName">Resource name of the effect sound to play</param>
        /// <returns></returns>
        public int? PlayLoop(string effectSoundResourceName)
        {
            var audioSourceIndex = Play(effectSoundResourceName);
            if (audioSourceIndex == null)
                return null;

            EffectSoundAudioSources[audioSourceIndex.Value].loop = true;

            return audioSourceIndex;
        }

        /// <summary>
        /// Pause the effect sound.
        /// </summary>
        /// <param name="audioSourceIndex">Resource name of the effect sound to stop</param>
        public void Pause(int? audioSourceIndex)
        {
            if (audioSourceIndex == null || audioSourceIndex < 0 || audioSourceIndex >= EffectSoundAudioSources.Count)
            {
                Debug.LogError($"audioSourceIndex({audioSourceIndex}) is not in range.");
                return;
            }
            
            EffectSoundAudioSources[audioSourceIndex.Value].Pause();
            PausedEffectSoundAudioSources.Add(EffectSoundAudioSources[audioSourceIndex.Value]);
        }
        
        /// <summary>
        /// Pause all effect sound.
        /// </summary>
        public void PauseAll()
        {
            foreach (var audioSource in EffectSoundAudioSources.Where(audioSource => audioSource.isPlaying))
            {
                audioSource.Pause();
                PausedEffectSoundAudioSources.Add(audioSource);
            }
        }

        /// <summary>
        /// Resume the effect sound.
        /// </summary>
        public void Resume(int? audioSourceIndex)
        {
            if (audioSourceIndex == null || audioSourceIndex < 0 || audioSourceIndex >= EffectSoundAudioSources.Count)
            {
                Debug.LogError($"audioSourceIndex({audioSourceIndex}) is not in range.");
                return;
            }
            
            EffectSoundAudioSources[audioSourceIndex.Value].Play();
            PausedEffectSoundAudioSources.Remove(EffectSoundAudioSources[audioSourceIndex.Value]);
        }

        /// <summary>
        /// Resume all effect sound.
        /// </summary>
        public void ResumeAll()
        {
            foreach (var pausedAudioSource in PausedEffectSoundAudioSources)
            {
                pausedAudioSource.Play();
            }

            _pausedEffectSoundAudioSources = null;
        }
        
        /// <summary>
        /// Stop the effect sound.
        /// </summary>
        /// <param name="audioSourceIndex">Resource name of the effect sound to stop</param>
        public void Stop(int? audioSourceIndex)
        {
            if (audioSourceIndex == null || audioSourceIndex < 0 || audioSourceIndex >= EffectSoundAudioSources.Count)
            {
                Debug.LogError($"audioSourceIndex({audioSourceIndex}) is not in range.");
                return;
            }

            EffectSoundAudioSources[audioSourceIndex.Value].loop = false;
            EffectSoundAudioSources[audioSourceIndex.Value].Stop();
            EffectSoundAudioSources[audioSourceIndex.Value].clip = null;
        }

        /// <summary>
        /// Stop all effect sound.
        /// </summary>
        public void StopAll()
        {
            foreach (var audioSource in EffectSoundAudioSources)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
        }

        /// <summary>
        /// Play the effect sound several times.
        /// Play the effect sound all the way through and then immediately play it again.
        /// </summary>
        /// <param name="effectSoundResourceName">Resource name of the effect sound to play</param>
        /// <param name="times">Number of plays</param>
        public void PlaySeveralTimes(string effectSoundResourceName, int times)
        {
            StartCoroutine(CoroutinePlaySoundSeveralTimes(effectSoundResourceName, times));
        }

        /// <summary>
        /// Enable or disable effect sound.
        /// </summary>
        /// <param name="enable">Enable if true Disable if false</param>
        public void EnableEffectSound(bool enable)
        {
            IsEffectSoundEnabled = enable;
            PlayerPrefs.SetInt(IsEffectSoundOn, enable ? 1 : 0);

            if (enable == false)
            {
                foreach (var effectSoundAudioSource in EffectSoundAudioSources)
                {
                    effectSoundAudioSource.Stop();
                }
            }
        }

        /// <summary>
        /// Set the volume of the effect sound.
        /// </summary>
        /// <param name="volume">The volume of the effect sound to be set</param>
        public void SetVolume(float volume)
        {
            _effectSoundVolume = volume;
            PlayerPrefs.SetFloat(EffectSoundVolume, volume);

            foreach (var effectSoundAudioSource in EffectSoundAudioSources)
            {
                effectSoundAudioSource.volume = volume;
            }
        }

        private int GetAvailableAudioSourceIndex()
        {
            for (var i = 0; i < EffectSoundAudioSources.Count; i++)
            {
                if (EffectSoundAudioSources[i].isPlaying == false)
                    return i;
            }

            return IncreaseAudioPool();
        }

        private int IncreaseAudioPool()
        {
            for (var i = 0; i < DefaultAudioSourceCount; i++)
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                EffectSoundAudioSources.Add(audioSource);
            }

            return EffectSoundAudioSources.Count - DefaultAudioSourceCount;
        }

        private void DecreaseAudioPool()
        {
            if (EffectSoundAudioSources.Count <= DefaultAudioSourceCount)
            {
                Debug.LogError("The number of Effect sound audio sources is less than the default audio source count.");
                return;
            }

            var count = EffectSoundAudioSources.Count;

            for (var i = 0; i < count; i++)
            {
                if (EffectSoundAudioSources[i].isPlaying == false)
                {
                    EffectSoundAudioSources.RemoveAt(i);
                    i--;
                    count--;

                    if (count <= DefaultAudioSourceCount)
                        break;
                }
            }
        }

        private IEnumerator CoroutinePlaySoundSeveralTimes(string effectSoundResourceName, int times)
        {
            if (IsEffectSoundEnabled == false)
            {
                Debug.Log("IsEffectSoundEnabled is false");
                yield break;
            }

            var effectSoundResourcePath = GetEffectSoundResourceDirectoryPath(effectSoundResourceName);
            if (EffectSoundAudioClips.ContainsKey(effectSoundResourcePath) == false)
            {
                var audioClip = Resources.Load<AudioClip>(effectSoundResourcePath);
                if (audioClip == null)
                {
                    Debug.LogError($"{effectSoundResourceName} is does not exist.");
                    yield break;
                }

                EffectSoundAudioClips[effectSoundResourcePath] = audioClip;
            }

            var waitForSeconds = new WaitForSeconds(EffectSoundAudioClips[effectSoundResourcePath].length);

            for (var i = 0; i < times; i++)
            {
                Play(effectSoundResourceName);
                yield return waitForSeconds;
            }
        }

        private string GetEffectSoundResourceDirectoryPath(string effectSoundName)
        {
            var soundResourceDirectoryPath = EffectSoundResourceRootPath + effectSoundName;

            return soundResourceDirectoryPath;
        }
    }
}