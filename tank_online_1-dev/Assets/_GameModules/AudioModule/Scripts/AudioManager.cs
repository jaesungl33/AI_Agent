using System;
using System.Collections.ObjectModel;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace GDO.Audio
{

    public class AudioManager : Singleton<AudioManager>, IInitializableManager
    {
        [SerializeField] private AudioSource backgroundMusicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private SoundCollection soundCollection;
        [SerializeField] private GameAssetCollection2 gameAssetCollection2;

        public UnityAction<bool> OnInitialized { get; set; }

        protected override void Awake()
        {
            base.Awake();
        }

        public void Initialize()
        {
            if (backgroundMusicSource == null || sfxSource == null)
            {
                Debug.LogError("AudioSources are not assigned in the AudioManager.");
            }
            soundCollection = CollectionExtensions.GetCollection<SoundCollection>();
            gameAssetCollection2 = CollectionExtensions.GetCollection<GameAssetCollection2>();

            if (soundCollection == null || gameAssetCollection2 == null)
            {
                Debug.LogError("SoundCollection or GameAssetCollection is not assigned in the AudioManager.");
                return;
            }
            // Subscribe to sound events
            EventManager.Register<AudioEvent>(OnSoundEvent);
            OnInitialized?.Invoke(true);
        }

        private void OnSoundEvent(AudioEvent audioEvent)
        {
            // Retrieve the AudioClip from the GameAssetCollection using the sound ID
            // AudioClip audioClip = gameAssetCollection.GetGameAssetDocumentById(audioEvent.AudioID)?.mainAsset as AudioClip;
            AudioClip audioClip = gameAssetCollection2.GetGameAssetDocumentById(audioEvent.GroupID, audioEvent.AudioID)?.mainAsset as AudioClip;
            if (audioClip == null)
            {
                Debug.LogWarning($"AudioClip with ID {audioEvent.AudioID} not found in GameAssetCollection.");
                return;
            }

            switch (audioEvent.SoundType)
            {
                case SoundType.SoundEffect:
                    PlaySFX(audioClip);
                    break;

                case SoundType.BackgroundMusic:
                    PlayBackgroundMusic(audioClip);
                    break;
            }
        }

        public void PlayBackgroundMusic(AudioClip audioClip)
        {
            if (backgroundMusicSource == null)
            {
                Debug.LogError("Background music AudioSource is not assigned: " + audioClip.name);
                return;
            }

            if (backgroundMusicSource.isPlaying)
            {
                backgroundMusicSource.Stop();
            }

            backgroundMusicSource.DOFade(1f, 1f);
            backgroundMusicSource.clip = audioClip;
            backgroundMusicSource.loop = true;
            backgroundMusicSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip);
                Debug.Log("Playing sound effect: " + clip.name);
            }
            else
            {
                Debug.LogError("Cannot play sound effect. Check AudioSource and AudioClip.");
            }
        }
    }

    public struct AudioEvent
    {
        public SoundType SoundType { get; }
        public string GroupID { get; }
        public string AudioID { get; }

        public AudioEvent(SoundType soundType, string eventID = "", string groupID = GameAssetGroups.AUDIO)
        {
            SoundType = soundType;
            GroupID = groupID;
            AudioID = eventID;
        }
    }
}