using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace qbot.Sound
{
    public class EffectSound : MonoBehaviour
    {
        #region Fields
        private const string EFFECT_SOUND_RESOURCE_ROOT_PATH = "Sound/EffectSound/";
        private const string IS_EFFECT_SOUND_ON = "IS_EFFECT_SOUND_ON";
        private const string EFFECT_SOUND_VOLUME = "EFFECT_SOUND_VOLUME";
        private const int DEFAULT_AUDIO_SOURCE_COUNT = 4;

        private float effectSoundVolume;
        #endregion

        #region Properties
        private List<AudioSource> effectSoundAudioSources;
        private List<AudioSource> EffectSoundAudioSources
        {
            get
            {
                effectSoundAudioSources ??= new List<AudioSource>();
                return effectSoundAudioSources;
            }
        }

        private Dictionary<string, AudioClip> effectSoundAudioClips;
        private Dictionary<string, AudioClip> EffectSoundAudioClips
        {
            get
            {
                effectSoundAudioClips ??= new Dictionary<string, AudioClip>();
                return effectSoundAudioClips;
            }
        }

        /// <summary>
        /// If true, the effect sound is played.
        /// if false, the effect sound is not played. 
        /// </summary>
        public bool IsEffectSoundEnabled { get; private set; }

        #endregion

        #region Monobehaviour functions
        private void Awake()
        {
            IsEffectSoundEnabled = PlayerPrefs.GetInt(IS_EFFECT_SOUND_ON, 1) == 1;
            effectSoundVolume = PlayerPrefs.GetFloat(EFFECT_SOUND_VOLUME, 1.0f);
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Play the effect sound.
        /// </summary>
        /// <param name="effectSoundResourceName">Resource name of the effect sound to play</param>
        /// <returns></returns>
        public int? Play(string effectSoundResourceName)
        {
            if (IsEffectSoundEnabled == false)
            {
                Debug.Log("IsEffectSoundEnabled is false");
                return null;
            }

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
            EffectSoundAudioSources[audioSourceIndex].volume = effectSoundVolume;

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
            PlayerPrefs.SetInt(IS_EFFECT_SOUND_ON, enable ? 1 : 0);

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
            effectSoundVolume = volume;
            PlayerPrefs.SetFloat(EFFECT_SOUND_VOLUME, volume);

            foreach (var effectSoundAudioSource in EffectSoundAudioSources)
            {
                effectSoundAudioSource.volume = volume;
            }
        }
        #endregion

        #region Priavte functions
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
            for (var i = 0; i < DEFAULT_AUDIO_SOURCE_COUNT; i++)
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                EffectSoundAudioSources.Add(audioSource);
            }

            return EffectSoundAudioSources.Count - DEFAULT_AUDIO_SOURCE_COUNT;
        }

        private void DecreaseAudioPool()
        {
            if (EffectSoundAudioSources.Count <= DEFAULT_AUDIO_SOURCE_COUNT)
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

                    if (count <= DEFAULT_AUDIO_SOURCE_COUNT)
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
            var soundResourceDirectoryPath = EFFECT_SOUND_RESOURCE_ROOT_PATH + effectSoundName;

            return soundResourceDirectoryPath;
        }
        #endregion
    }
}