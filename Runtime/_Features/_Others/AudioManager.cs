using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public class AudioManager : NSingleton<AudioManager>
    {
        private struct PlayingData
        {
            public AudioSource audioSource;
            public bool isBgm;
            public float volume;
        }
        [SerializeField] private int _audioSourcePoolCapacity = 8;
        [SerializeField, Range(0, 1)] private float _masterVolume = 1;
        [SerializeField, Range(0, 1)] private float _sfxMasterVolume = 1;
        [SerializeField, Range(0, 1)] private float _bgmMasterVolume = 1;

        private List<AudioSource> _audioSourcePool = new();
        private SimpleDictionary<int, PlayingData> _playingData = new();

        private int _audioSourceTotalCount;

        private void OnValidate()
        {
            updateVolumeOfPlayingAudioSources(default);
        }

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                value = Mathf.Clamp01(value);
                if (_masterVolume == value) return;
                _masterVolume = value;
                updateVolumeOfPlayingAudioSources(false);
            }
        }
        public float SfxMasterVolume
        {
            get => _sfxMasterVolume;
            set 
            {
                value = Mathf.Clamp01(value);
                if (_sfxMasterVolume == value) return;
                _sfxMasterVolume = value;
                updateVolumeOfPlayingAudioSources(false);
            }
        }
        public float BgmMasterVolume
        {
            get => _bgmMasterVolume;
            set
            {
                value = Mathf.Clamp01(value);
                if (_bgmMasterVolume == value) return;
                _bgmMasterVolume = value;
                updateVolumeOfPlayingAudioSources(true);
            }
        }

        private AudioSource getNext()
        {
            AudioSource audioSource;
            if (_audioSourcePool.Count > 0)
            {
                audioSource = _audioSourcePool.takeAndRemoveLast();
                audioSource.gameObject.SetActive(true);
            }
            else
            {
                audioSource = new GameObject($"AudioSource_{++_audioSourceTotalCount}").AddComponent<AudioSource>();
                audioSource.transform.SetParent(transform, false);
                audioSource.playOnAwake = false;
            }
            return audioSource;
        }
        private void updateVolumeOfPlayingAudioSources(bool? isBgm)
        {
            foreach ((_, var playingData) in _playingData)
            {
                if (isBgm.HasValue && playingData.isBgm != isBgm) continue;
                var masterVolume = (playingData.isBgm ? _bgmMasterVolume : _sfxMasterVolume) * _masterVolume;
                playingData.audioSource.volume = playingData.volume * masterVolume;
            }
        }
        private void release(AudioSource audioSource)
        {
            if (_audioSourcePool.Count > _audioSourcePoolCapacity)
            {
                Destroy(audioSource);
            }
            else
            {
                audioSource.clip = null;
                audioSource.gameObject.SetActive(false);
                _audioSourcePool.Add(audioSource);
            }
        }
        private void addPlayingData(AudioSource audioSource, bool isBgm, float volume)
        {
            _playingData.Add(audioSource.GetInstanceID(), new PlayingData
            {
                audioSource = audioSource,
                isBgm = isBgm,
                volume = volume
            });
        }
        private AudioPlayHandler innerPlayClip(bool isBgm, AudioClip clip, float volume, float startTime = 0, float duration = 0, bool isLoop = false)
        {
            var audioSrc = getNext();
            AudioPlayHandler handler = new AudioPlayHandler(audioSrc.GetInstanceID());
            addPlayingData(audioSrc, isBgm, volume);
            audioSrc.clip = clip;
            audioSrc.volume = volume * (isBgm ? _bgmMasterVolume : _sfxMasterVolume) * _masterVolume;
            audioSrc.loop = isLoop;
            audioSrc.Play();
            if (startTime > 0)
            {
                audioSrc.time = startTime;
            }

            if (duration > 0)
            {
                stopById(handler.id, duration);
            }
            else
            {
                if (!isLoop)
                {
                    stopById(handler.id, clip.length - startTime);
                }
            }
            return handler;
        }
        
        public void stopById(int id)
        {
            if (_playingData.tryTakeAndRemove(id, out var data))
            {
                var audioSrc = data.audioSource;
                if (audioSrc.isPlaying)
                {
                    audioSrc.Stop();
                }
                release(audioSrc);
            }
        }
        public async void stopById(int id, float delay)
        {
            await new NWaitSecond(delay);
            stopById(id);
        }
        public void stopByAudioClip(AudioClip clip, bool isBgm, bool isStopAll)
        {
            using var audioSources = NPUArray<int>.get();
            foreach ((_, var data) in _playingData)
            {
                if (data.isBgm == isBgm && data.audioSource.clip == clip)
                {
                    audioSources.Add(data.audioSource.GetInstanceID());
                    if (!isStopAll) break;
                }
            }

            foreach (var audioSrc in audioSources)
            {
                stopById(audioSrc);
            }
        }
        public void stopAllByType(bool isBgm)
        {
            using var audioSources = NPUArray<int>.get();
            foreach ((_, var data) in _playingData)
            {
                if (data.isBgm == isBgm)
                {
                    audioSources.Add(data.audioSource.GetInstanceID());
                }
            }

            foreach (var audioSrc in audioSources)
            {
                stopById(audioSrc);
            }
        }
        public void stopAll()
        {
            if (_playingData.Count > 0)
            {
                foreach ((_, var data) in _playingData)
                {
                    if (data.audioSource.isPlaying)
                    {
                        data.audioSource.Stop();
                        release(data.audioSource);
                    }
                }
                _playingData.Clear();
            }
        }

        public AudioPlayHandler playSfx(AudioClip clip)
        {
            return innerPlayClip(false, clip, 1);
        }
        public AudioPlayHandler playSfx(AudioClip clip, float volume)
        {
            return innerPlayClip(false, clip, volume);
        }
        public AudioPlayHandler playSfx(AudioClip clip, float volume, float startTime, float duration, bool isLoop)
        {
            return innerPlayClip(false, clip, volume, startTime, duration, isLoop);
        }
        public void stopSfx(AudioClip clip)
        {
            stopByAudioClip(clip, false, false);
        }
        public void stopAllSfx()
        {
            stopAllByType(false);
        }

        public AudioPlayHandler playBgm(AudioClip clip) 
        {
            return innerPlayClip(true, clip, 1, isLoop: true);
        }
        public AudioPlayHandler playBgm(AudioClip clip, float volume)
        {
            return innerPlayClip(true, clip, volume, isLoop: true);
        }
        public AudioPlayHandler playBgm(AudioClip clip, float volume, float startTime, float duration)
        {
            return innerPlayClip(true, clip, volume, startTime, duration, true);
        }
        public void stopBgm(AudioClip clip)
        {
            stopByAudioClip(clip, true, false);
        }
        public void stopAllBgm()
        {
            stopAllByType(true);
        }
    }
    public struct AudioPlayHandler
    {
        public readonly int id;
        public AudioPlayHandler(int id)
        {
            this.id = id;
        }
        public void stop()
        {
            AudioManager.Instance.stopById(id);
        }
    }
}
