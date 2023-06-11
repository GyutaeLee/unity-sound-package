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
                this.effectSoundAudioSources ??= new List<AudioSource>();
                return this.effectSoundAudioSources;
            }
        }

        private Dictionary<string, AudioClip> effectSoundAudioClipDictionary;
        private Dictionary<string, AudioClip> EffectSoundAudioClipDictionary
        {
            get
            {
                this.effectSoundAudioClipDictionary ??= new Dictionary<string, AudioClip>();
                return this.effectSoundAudioClipDictionary;
            }
        }

        /// <summary>
        /// If true, the effect sound is played.
        /// if false, the effect sound is not played. 
        /// </summary>
        private bool isEffectSoundEnabled;
        public bool IsEffectSoundEnabled
        {
            get
            {
                return isEffectSoundEnabled;
            }
            private set
            {
                isEffectSoundEnabled = value;
            }
        }
        #endregion

        #region Monobehaviour functions
        private void Start()
        {
            this.IsEffectSoundEnabled = PlayerPrefs.GetInt(IS_EFFECT_SOUND_ON, 1) == 1;
            this.effectSoundVolume = PlayerPrefs.GetFloat(EFFECT_SOUND_VOLUME, 1.0f);
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
            if (this.IsEffectSoundEnabled == false)
            {
                Debug.Log("IsEffectSoundEnabled is false");
                return null;
            }

            var effectSoundResourcePath = GetEffectSoundResourceDirectoryPath(effectSoundResourceName);
            if (this.effectSoundAudioClipDictionary.ContainsKey(effectSoundResourcePath) == false)
            {
                var audioClip = Resources.Load<AudioClip>(effectSoundResourcePath);
                if (audioClip == null)
                {
                    Debug.LogError($"{effectSoundResourceName} is does not exist.");
                    return null;
                }

                this.effectSoundAudioClipDictionary[effectSoundResourcePath] = audioClip;
            }

            var audioSourceIndex = GetAvailableAudioSourceIndex();

            this.effectSoundAudioSources[audioSourceIndex].clip = this.effectSoundAudioClipDictionary[effectSoundResourcePath];
            this.effectSoundAudioSources[audioSourceIndex].Play();
            this.effectSoundAudioSources[audioSourceIndex].volume = effectSoundVolume;

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

            this.effectSoundAudioSources[audioSourceIndex.Value].loop = true;

            return audioSourceIndex;
        }

        /// <summary>
        /// Stop the effect sound.
        /// </summary>
        /// <param name="audioSourceIndex">Resource name of the effect sound to stop</param>
        public void Stop(int? audioSourceIndex)
        {
            if (audioSourceIndex == null || audioSourceIndex < 0 || audioSourceIndex >= this.effectSoundAudioSources.Count)
            {
                Debug.LogError($"audioSourceIndex({audioSourceIndex}) is not in range.");
                return;
            }

            this.effectSoundAudioSources[audioSourceIndex.Value].loop = false;
            this.effectSoundAudioSources[audioSourceIndex.Value].Stop();
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
            this.IsEffectSoundEnabled = enable;
            PlayerPrefs.SetInt(IS_EFFECT_SOUND_ON, enable ? 1 : 0);

            if (enable == false)
            {
                foreach (var effectSoundAudioSource in this.effectSoundAudioSources)
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
            this.effectSoundVolume = volume;
            PlayerPrefs.SetFloat(EFFECT_SOUND_VOLUME, volume);

            foreach (var effectSoundAudioSource in this.effectSoundAudioSources)
            {
                effectSoundAudioSource.volume = volume;
            }
        }
        #endregion

        #region Priavte functions
        private int GetAvailableAudioSourceIndex()
        {
            for (var i = 0; i < this.effectSoundAudioSources.Count; i++)
            {
                if (this.effectSoundAudioSources[i].isPlaying == false)
                    return i;
            }

            return IncreaseAudioPool();
        }

        private int IncreaseAudioPool()
        {
            for (var i = 0; i < DEFAULT_AUDIO_SOURCE_COUNT; i++)
            {
                var audioSource = this.gameObject.AddComponent<AudioSource>();
                this.effectSoundAudioSources.Add(audioSource);
            }

            return this.effectSoundAudioSources.Count - DEFAULT_AUDIO_SOURCE_COUNT;
        }

        private void DecreaseAudioPool()
        {
            if (this.effectSoundAudioSources.Count <= DEFAULT_AUDIO_SOURCE_COUNT)
            {
                Debug.LogError("The number of Effect sound audio sources is less than the default audio source count.");
                return;
            }

            var count = this.effectSoundAudioSources.Count;

            for (var i = 0; i < count; i++)
            {
                if (this.effectSoundAudioSources[i].isPlaying == false)
                {
                    this.effectSoundAudioSources.RemoveAt(i);
                    i--;
                    count--;

                    if (count <= DEFAULT_AUDIO_SOURCE_COUNT)
                        break;
                }
            }
        }

        private IEnumerator CoroutinePlaySoundSeveralTimes(string effectSoundResourceName, int times)
        {
            if (this.IsEffectSoundEnabled == false)
            {
                Debug.Log("IsEffectSoundEnabled is false");
                yield break;
            }

            var effectSoundResourcePath = GetEffectSoundResourceDirectoryPath(effectSoundResourceName);
            if (this.effectSoundAudioClipDictionary.ContainsKey(effectSoundResourcePath) == false)
            {
                var audioClip = Resources.Load<AudioClip>(effectSoundResourcePath);
                if (audioClip == null)
                {
                    Debug.LogError($"{effectSoundResourceName} is does not exist.");
                    yield break;
                }
                this.effectSoundAudioClipDictionary[effectSoundResourcePath] = audioClip;
            }

            var waitForSeconds = new WaitForSeconds(this.effectSoundAudioClipDictionary[effectSoundResourcePath].length);

            for (var i = 0; i < times; i++)
            {
                Play(effectSoundResourceName);
                yield return waitForSeconds;
            }
        }

        private string GetEffectSoundResourceDirectoryPath(string effectSoundType)
        {
            var soundResourceDirectoryPath = EFFECT_SOUND_RESOURCE_ROOT_PATH + effectSoundType.ToString() + "/";

            return soundResourceDirectoryPath;
        }
        #endregion
    }
}