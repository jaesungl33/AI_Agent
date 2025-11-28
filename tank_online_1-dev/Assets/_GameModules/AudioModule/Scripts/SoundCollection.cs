using System.Collections.Generic;
using UnityEngine;

namespace GDO.Audio
{
    [CreateAssetMenu(fileName = "SoundCollection", menuName = "ScriptableObjects/SoundCollection", order = 1)]
    public class SoundCollection : CollectionBase<SoundDocument>
    {
        public SoundDocument GetSoundDocumentById(string soundID)
        {
            if (string.IsNullOrEmpty(soundID))
            {
                Debug.LogWarning("Sound ID is null or empty.");
                return null;
            }

            foreach (var soundDoc in documents)
            {
                if (soundDoc.soundID == soundID)
                {
                    return soundDoc;
                }
            }

            Debug.LogWarning($"Sound document with ID {soundID} not found in collection.");
            return null;
        }

        public SoundDocument GetSoundDocumentByType(SoundType soundType)
        {
            foreach (var soundDoc in documents)
            {
                if (soundDoc.soundType == soundType)
                {
                    return soundDoc;
                }
            }

            Debug.LogWarning($"Sound document with type {soundType} not found in collection.");
            return null;
        }
    }
}
