using GDO.Audio;
using UnityEngine;

public static class AudioHelper
{
    public static void PlaySFX(string assetID)
    {
        PlayAudio(SoundType.SoundEffect, assetID);
    }

    public static void PlayBGM(string assetID)
    {
         PlayAudio(SoundType.BackgroundMusic, assetID);
    }

    private static void PlayAudio(SoundType soundType, string assetID)
    {
        EventManager.TriggerEvent<AudioEvent>(new AudioEvent(soundType, assetID));
    }
}