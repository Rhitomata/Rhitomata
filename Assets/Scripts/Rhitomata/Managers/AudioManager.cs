using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Rhitomata {
    /// <summary>
    /// Fuck Unity Audio
    /// </summary>
    public class AudioManager : MonoBehaviour {
        [Header("References")]
        [SerializeField]
        private AudioSource source;
        
        [Header("States")]
        public AudioState state;

        public float time => (float)(_elapsedTime + (_stopwatch.Elapsed.TotalSeconds - _lastMixTime));
        
        [Header("Events")]
        public UnityEvent onFinished;
        public AudioClip clip { get => source.clip; set => source.clip = value; }
        public bool isPlaying => state == AudioState.Playing;

        [EditorButton("StartMe")]
        public bool testMode = false;
        
        private readonly Stopwatch _stopwatch = new();
        private double _lastMixTime;
        private double _elapsedTime;
        private double _initialTime;
        private double _pauseTime;

        public static void StartMe() {
            References.Instance.music.Play();
        }

        private void OnEnable() {
            _stopwatch.Start();
        }

        private void OnDisable() {
            _stopwatch.Stop();
            _stopwatch.Reset();
        }

        // Mixing time
        private void OnAudioFilterRead(float[] data, int channels) {
            _lastMixTime = _stopwatch.Elapsed.TotalSeconds;
            _elapsedTime = AudioSettings.dspTime - _initialTime;
            
        }

        private void Update() {
            if (testMode) {
                
            }
        }

        #region Common Functions
        public void Play() {
            _initialTime = AudioSettings.dspTime;
            source.Play();
            state = AudioState.Playing;
        }

        public void Pause() {
            _pauseTime = AudioSettings.dspTime;
            source.Pause();
            state = AudioState.Paused;
        }

        public void Stop() {
            source.Stop();
            state = AudioState.Stopped;
        }
        #endregion
    }
        
    public enum AudioState {
        Stopped,
        Paused,
        Playing
    }
}