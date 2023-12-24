using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
#if UNITY_WEBGL
    public static class MicrophoneRecorder
    {
        public static void startRecord(int frequency = 8000, string deviceName = null)
        {
            throw new PlatformNotSupportedException();
        }
        public static AudioClip stopRecord()
        {
            throw new PlatformNotSupportedException();
        }
    }
#else
    public static class MicrophoneRecorder
    {
        private static List<AudioClip> _allCacheClips;
        private static AudioClip _micClip;
        private static int _lastMicPosition;
        private static float _startRecordTime;
        private static float[] _tempSample;
        private static AudioClip _tempClip;
        private static string _deviceName;

        public static bool IsRecording => Microphone.devices.Length > 0 && Microphone.IsRecording(null);

        public static void startRecord(int frequency = 8000, string deviceName = null)
        {
            if (Microphone.devices.Length <= 0)
            {
                throw new Exception("Microphone not connected");
            }
            if (Microphone.IsRecording(null))
            {
                return;
            }

            tryDisposeOldData();

            _allCacheClips ??= new List<AudioClip>();
            if (!(_micClip = Microphone.Start(_deviceName = deviceName, true, 10, frequency)))
            {
                throw new Exception("Failed to start record");
            }
            _startRecordTime = Time.time;
            _tempSample = new float[_micClip.channels];
            NUpdater.onUpdateEvent.add(update);
        }
        private static void update()
        {
            var pos = Microphone.GetPosition(_deviceName);
            if (_lastMicPosition > pos)
            {
                if (_tempClip != null)
                {
                    _allCacheClips.Add(_tempClip);
                    _tempClip = null;
                    _lastMicPosition = 0;
                }
            }
            while (pos > _lastMicPosition)
            {
                if (_tempClip == null)
                {
                    _tempClip ??= AudioClip.Create($"record_{_allCacheClips.Count}", _micClip.samples, _micClip.channels, _micClip.frequency, false);
                }

                _micClip.GetData(_tempSample, _lastMicPosition);
                _tempClip.SetData(_tempSample, _lastMicPosition);
                ++_lastMicPosition;
            }
        }
        public static AudioClip stopRecord()
        {
            if (!IsRecording)
            {
                throw new Exception("Microphone is not recording");
            }
            NUpdater.onUpdateEvent.remove(update);
            Microphone.End(_deviceName);
            if (_tempClip != null)
            {
                _allCacheClips.Add(_tempClip);
            }

            int maxClipIndex = _allCacheClips.Count - 1;
            if (maxClipIndex < 0)
            {
                throw new Exception("Microphone record cache is empty");
            }

            var totalTime = Time.time - _startRecordTime;
            var finalClip = AudioClip.Create($"record_final", (int)(totalTime * _micClip.frequency), _micClip.channels, _micClip.frequency, false);

            int currentOffset = 0;
            AudioClip clip;
            var span = _allCacheClips.asSpan();
            float[] sample = new float[span[0].samples];

            for (int i = 0; i < maxClipIndex; i++)
            {
                clip = span[i];
                clip.GetData(sample, 0);
                finalClip.SetData(sample, currentOffset);
                currentOffset += sample.Length;
            }

            clip = span[maxClipIndex];
            sample = new float[finalClip.samples - currentOffset];
            clip.GetData(sample, 0);
            finalClip.SetData(sample, currentOffset);

            tryDisposeOldData();
            return finalClip;
        }
        private static void tryDisposeOldData()
        {
            if (_allCacheClips != null)
            {
                foreach (var clip in _allCacheClips.asSpan())
                {
                    if (clip)
                    {
                        NUtils.destroy(clip);
                    }
                }
                _allCacheClips.Clear();
            }
            if (_tempClip)
            {
                NUtils.destroy(_tempClip);
                _tempClip = null;
            }
            if (_micClip)
            {
                NUtils.destroy(_micClip);
                _micClip = null;
            }
            _lastMicPosition = 0;
            _startRecordTime = 0;
            _tempSample = null;
        }

#if UNITY_EDITOR
        [EditorQuittingMethod]
        private static void tryStopAndDispose()
        {
            try
            {
                if (IsRecording)
                {
                    stopRecord();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                tryDisposeOldData();
            }
        }
#endif
    }
#endif
}