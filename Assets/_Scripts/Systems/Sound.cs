using UnityEngine;

namespace _Scripts.Systems
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f,1f)]
        public float volume;
        [Range(0f, 1f)]
        public float volumeVariance = .1f;
        [Range(0.1f,3f)]
        public float pitch;
        [Range(0f, 1f)]
        public float pitchVariance = .1f;
        public bool loop;
        
        [HideInInspector]
        public AudioSource source;
    }
}
