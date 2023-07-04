using UnityEngine;
using Debug = UnityEngine.Debug;

namespace qbot.Sound
{
    public class Bgm : MonoBehaviour
    {
        #region Fields
        private const string BGM_RESOURCE_ROOT_PATH = "Sound/Bgm/";
        private const string IS_BGM_ENABLED = "IS_BGM_ENABLED";
        private const string BGM_VOLUME = "BGM_VOLUME";

        private float bgmVolume;

        private AudioClip bgmAudioClip;
        #endregion

        #region Properties
        private AudioSource bgmAudioSource;
        private AudioSource BgmAudioSource
        {
            get
            {
                if (bgmAudioSource == null)
                {
                    bgmAudioSource = gameObject.AddComponent<AudioSource>();
                    bgmAudioSource.loop = true;
                    bgmAudioSource.volume = bgmVolume;
                }

                return bgmAudioSource;
            }
        }

        public bool IsBgmEnabled { get; private set; }

        #endregion

        #region MonoBehaviour functions
        private void Awake()
        {
            IsBgmEnabled = PlayerPrefs.GetInt(IS_BGM_ENABLED, 1) == 1;
            bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME, 1.0f);
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Play the bgm.
        /// </summary>
        /// <param name="bgmResourceName">Resource name of the BGM to play</param>
        /// <returns>Results for successful BGM playback</returns>
        public bool Play(string bgmResourceName)
        {
            if (IsBgmEnabled == false)
            {
                Debug.Log("IsBgmEnabled is false.");
                return true;
            }

            var bgmResourcePath = GetBgmResourceDirectoryPath(bgmResourceName);
            var audioClip = Resources.Load<AudioClip>(bgmResourcePath);

            if (audioClip == null)
            {
                Debug.LogError("There is no Bgm audio clip.");
                return false;
            }

            if (BgmAudioSource.clip == audioClip && BgmAudioSource.isPlaying)
            {
                Debug.Log("The same Bgm audio clip is already playing.");
                return false;
            }

            bgmAudioClip = audioClip;
            BgmAudioSource.clip = bgmAudioClip;
            BgmAudioSource.volume = bgmVolume;
            BgmAudioSource.Play();

            return true;

        }

        /// <summary>
        /// Stop the currently playing BGM.
        /// </summary>
        public void Stop()
        {
            BgmAudioSource.Stop();
        }

        /// <summary>
        /// Pauses the currently playing BGM.
        /// </summary>
        public void Pause()
        {
            BgmAudioSource.Pause();
        }

        /// <summary>
        /// Continue playing the currently stopped BGM.
        /// </summary>
        public void Resume()
        {
            if (IsBgmEnabled == false)
            {
                Debug.Log("IsBgmEnabled is false.");
                return;
            }

            BgmAudioSource.Play();
        }

        /// <summary>
        /// Enable or disable Bgm.
        /// </summary>
        /// <param name="enable">Enable if true Disable if false</param>
        /// <param name="playBgmAfterEnable">Whether to play BGM after activation</param>
        public void EnableBgm(bool enable, bool playBgmAfterEnable = false)
        {
            if (IsBgmEnabled == enable)
                return;

            IsBgmEnabled = enable;
            PlayerPrefs.SetInt(IS_BGM_ENABLED, enable ? 1 : 0);

            if (enable)
            {
                BgmAudioSource.volume = bgmVolume;

                if (playBgmAfterEnable)
                {
                    BgmAudioSource.Play();
                }
            }
            else
            {
                BgmAudioSource.volume = 0;
                BgmAudioSource.Stop();
            }
        }

        /// <summary>
        /// Set the volume of the Bgm.
        /// </summary>
        /// <param name="volume">The volume of the bgm to be set</param>
        public void SetVolume(float volume)
        {
            BgmAudioSource.volume = volume;
            PlayerPrefs.SetFloat(BGM_VOLUME, volume);
        }
        #endregion

        #region Private functions
        private string GetBgmResourceDirectoryPath(string bgmResourceName)
        {
            var bgmResourceDirectoryPath = BGM_RESOURCE_ROOT_PATH + bgmResourceName;
            return bgmResourceDirectoryPath;
        }
        #endregion
    }
}