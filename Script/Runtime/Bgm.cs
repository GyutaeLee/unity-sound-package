using System.Diagnostics;
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
                if (this.bgmAudioSource == null)
                {
                    this.bgmAudioSource = this.gameObject.AddComponent<AudioSource>();
                    this.bgmAudioSource.loop = true;
                    this.bgmAudioSource.volume = this.bgmVolume;
                }

                return this.bgmAudioSource;
            }
        }

        private bool isBgmEnabled;
        public bool IsBgmEnabled
        {
            get
            {
                return isBgmEnabled;
            }
            private set
            {
                isBgmEnabled = value;
            }
        }
        #endregion

        #region MonoBehaviour functions
        private void Awake()
        {
            this.IsBgmEnabled = PlayerPrefs.GetInt(IS_BGM_ENABLED, 1) == 1;
            this.bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME, 1.0f);
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
            if (this.IsBgmEnabled == false)
            {
                Debug.Log("IsBgmEnabled is false.");
                return true;
            }

            var bgmResourcePath = GetBgmResourceDirectoryPath(bgmResourceName);
            var bgmAudioClip = Resources.Load<AudioClip>(bgmResourcePath);

            if (bgmAudioClip == null)
            {
                Debug.LogError("There is no Bgm audio clip.");
                return false;
            }

            if (this.BgmAudioSource.clip == bgmAudioClip && this.BgmAudioSource.isPlaying == true)
            {
                Debug.Log("The same Bgm audio clip is already playing.");
                return false;
            }

            this.bgmAudioClip = bgmAudioClip;
            this.BgmAudioSource.clip = this.bgmAudioClip;
            this.BgmAudioSource.volume = this.bgmVolume;
            this.BgmAudioSource.Play();

            return true;

        }

        /// <summary>
        /// Stop the currently playing BGM.
        /// </summary>
        public void Stop()
        {
            this.BgmAudioSource.Stop();
        }

        /// <summary>
        /// Pauses the currently playing BGM.
        /// </summary>
        public void Pause()
        {
            this.BgmAudioSource.Pause();
        }

        /// <summary>
        /// Continue playing the currently stopped BGM.
        /// </summary>
        public void Resume()
        {
            if (this.IsBgmEnabled == false)
            {
                Debug.Log("IsBgmEnabled is false.");
                return;
            }

            this.BgmAudioSource.Play();
        }

        /// <summary>
        /// Enable or disable Bgm.
        /// </summary>
        /// <param name="enable">Enable if true Disable if false</param>
        /// <param name="playBgmAfterEnable">Whether to play BGM after activation</param>
        public void EnableBgm(bool enable, bool playBgmAfterEnable = false)
        {
            if (this.IsBgmEnabled == enable)
                return;

            this.IsBgmEnabled = enable;
            PlayerPrefs.SetInt(IS_BGM_ENABLED, enable ? 1 : 0);

            if (enable == true)
            {
                this.BgmAudioSource.volume = this.bgmVolume;

                if (playBgmAfterEnable)
                {
                    this.BgmAudioSource.Play();
                }
            }
            else
            {
                this.BgmAudioSource.volume = 0;
                this.BgmAudioSource.Stop();
            }
        }

        /// <summary>
        /// Set the volume of the Bgm.
        /// </summary>
        /// <param name="volume">The volume of the bgm to be set</param>
        public void SetVolume(float volume)
        {
            this.BgmAudioSource.volume = volume;
            PlayerPrefs.SetFloat(BGM_VOLUME, volume);
        }
        #endregion

        #region Private functions
        private string GetBgmResourceDirectoryPath(string bgmResourcename)
        {
            string bgmResourceDirectoryPath = BGM_RESOURCE_ROOT_PATH + bgmResourcename;
            return bgmResourceDirectoryPath;
        }
        #endregion
    }
}