using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace CoreUtils {
    public class TriggerSound : MonoBehaviour {
        [SerializeField] private string m_SoundName = "slap";
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private float m_MinimumWait;

        [SerializeField, Tooltip("List of audio clips to play.")]
        private AudioClip[] m_AudioClips;

        [SerializeField, Tooltip("Stops the currently playing clip in the audioSource. Otherwise clips will overlap/mix.")]
        private bool m_StopOnPlay;

        [SerializeField, Tooltip("Start a wave file playing on awake, but after a delay.")]
        private bool m_PlayOnAwakeWithDelay;

        [Header("Random Volume"), SerializeField]
        private bool m_UseRandomVolume = true;

        [SerializeField, Tooltip("Minimum volume that will be used when randomly set."), Range(0.0f, 1.0f)]
        private float m_VolMin = 1.0f;

        [SerializeField, Tooltip("Maximum volume that will be used when randomly set."), Range(0.0f, 1.0f)]
        private float m_VolMax = 1.0f;

        [Header("Random Pitch"), SerializeField, Tooltip("Use min and max random pitch levels when playing sounds.")]
        private bool m_UseRandomPitch = true;

        [SerializeField, Tooltip("Minimum pitch that will be used when randomly set."), Range(-3.0f, 3.0f)]
        private float m_PitchMin = 1.0f;

        [SerializeField, Tooltip("Maximum pitch that will be used when randomly set."), Range(-3.0f, 3.0f)]
        private float m_PitchMax = 1.0f;

        [Header("Delay Time"), SerializeField, Tooltip("Time to offset playback of sound")]
        private float m_DelayOffsetTime;

        private AudioClip m_Clip;
        private float m_LastSound;

        public UnityEvent OnSoundTriggered;
        private float m_OriginalVolume;

        private void Awake() {
            m_AudioSource = m_AudioSource ? m_AudioSource : GetComponent<AudioSource>();
            m_OriginalVolume = m_AudioSource.volume;
            m_Clip = m_AudioSource.clip;

            // audio source play on awake is true, just play the PlaySound immediately
            if (m_AudioSource.playOnAwake) {
                PlaySound();
            }

            // if playOnAwake is false, but the playOnAwakeWithDelay on the PlaySound is true, play the sound on away but with a delay
            else if (!m_AudioSource.playOnAwake && m_PlayOnAwakeWithDelay) {
                PlayWithDelay(m_DelayOffsetTime);
            }

            // in the case where both playOnAwake and playOnAwakeWithDelay are both set to true, just to the same as above, play the sound but with a delay
            else if (m_AudioSource.playOnAwake && m_PlayOnAwakeWithDelay) {
                PlayWithDelay(m_DelayOffsetTime);
            }
        }

        public void PlayWithDelay(float delayTime) {
            Invoke("PlaySound", delayTime);
        }

        public void PlaySound() {
            PlayInternal();
        }

        [UsedImplicitly]
        public void PlaySoundType(string nam) {
            if (nam.Equals(m_SoundName, StringComparison.OrdinalIgnoreCase)) {
                PlayInternal();
            }
        }

        private void PlayInternal() {
            if (!m_AudioSource.isActiveAndEnabled || m_MinimumWait > 0 && Time.time < m_LastSound + m_MinimumWait) {
                return;
            }
            SetAudioSource();
            if (m_StopOnPlay) {
                m_AudioSource.Stop();
            }
            m_AudioSource.PlayOneShot(m_Clip);
            m_LastSound = Time.time;
            OnSoundTriggered?.Invoke();
        }

        public void Stop() {
            m_AudioSource.Stop();
        }

        private void SetAudioSource() {
            if (m_UseRandomVolume) {
                //randomly apply a volume between the volume min max
                m_AudioSource.volume = m_OriginalVolume * Random.Range(m_VolMin, m_VolMax);
            }

            if (m_UseRandomPitch) {
                //randomly apply a pitch between the pitch min max
                m_AudioSource.pitch = Random.Range(m_PitchMin, m_PitchMax);
            }

            if (m_AudioClips.Length > 0) {
                // randomly assign a wave file from the array into the audioSource clip property
                m_AudioSource.clip = m_AudioClips[Random.Range(0, m_AudioClips.Length)];
                m_Clip = m_AudioSource.clip;
            }
        }

        private void Reset() {
            m_AudioSource = GetComponentInChildren<AudioSource>();
        }
    }
}