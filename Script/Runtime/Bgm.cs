using UnityEngine;
using Debug = UnityEngine.Debug;

namespace qbot.Sound
{
    public class Bgm : MonoBehaviour
    {
        private const string BGMResourceRootPath = "Sound/Bgm/";
        private const string IsBGMEnabled = nameof(IsBGMEnabled);
        private const string BGMVolume = nameof(BGMVolume);

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

        private float _bgmVolume;

        private AudioClip _bgmAudioClip;
        private AudioSource _bgmAudioSource;

        public static Bgm Instance { get; private set; }

        /// <summary>
        /// If true, the bgm is played.
        /// if false, the bgm is not played. 
        /// </summary>
        public bool IsBgmEnabled { get; private set; }

        private AudioSource BgmAudioSource
        {
            get
            {
                if (_bgmAudioSource == null)
                {
                    _bgmAudioSource = gameObject.AddComponent<AudioSource>();
                    _bgmAudioSource.loop = true;
                    _bgmAudioSource.volume = _bgmVolume;
                }

                return _bgmAudioSource;
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

            IsBgmEnabled = PlayerPrefs.GetInt(IsBGMEnabled, 1) == 1;
            _bgmVolume = PlayerPrefs.GetFloat(BGMVolume, 1.0f);
        }

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

            _bgmAudioClip = audioClip;
            BgmAudioSource.clip = _bgmAudioClip;
            BgmAudioSource.volume = _bgmVolume;
            BgmAudioSource.Play();

            return true;
        }

        /// <summary>
        /// Stop the currently playing BGM.
        /// </summary>
        public void Stop()
        {
            BgmAudioSource.Stop();
            BgmAudioSource.clip = null;
            _bgmAudioClip = null;
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
            PlayerPrefs.SetInt(IsBGMEnabled, enable ? 1 : 0);

            if (enable)
            {
                BgmAudioSource.volume = _bgmVolume;

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
            PlayerPrefs.SetFloat(BGMVolume, volume);
        }

        private string GetBgmResourceDirectoryPath(string bgmResourceName)
        {
            var bgmResourceDirectoryPath = BGMResourceRootPath + bgmResourceName;
            return bgmResourceDirectoryPath;
        }
    }
}