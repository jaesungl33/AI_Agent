using UnityEngine;

namespace GDO.Audio
{
    [System.Serializable]
    public class SoundDocument
    {
        public string soundID;
        public SoundType soundType = SoundType.None;
        public bool isLooping = false;
        public float volume = 1.0f;
    }

    public enum SoundType
    {
        None,
        BackgroundMusic,
        SoundEffect,
    }
}
