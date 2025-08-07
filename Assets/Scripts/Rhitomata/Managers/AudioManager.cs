using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Rhitomata {
    /// <summary>
    /// Fuck Unity Audio
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour {
        public AudioState state { get; private set; }
        public float time;
        public UnityEvent onFinished;
        public AudioClip clip { get => source.clip; set => source.clip = value; }
        public bool isPlaying => state == AudioState.Playing;
        private AudioSource _source;
        private AudioSource source => _source = _source ? _source : GetComponent<AudioSource>();
        private double _elapsedTime;
        private double _initialTime;

        public bool testMode = false;
        
        // Mixing time
        private void OnAudioFilterRead(float[] data, int channels) {
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