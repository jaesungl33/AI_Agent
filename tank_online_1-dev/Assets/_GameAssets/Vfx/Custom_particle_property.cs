using UnityEngine;
using System.Collections.Generic;

public class MultiParticleController : MonoBehaviour
{
    public enum LoopMode
    {
        Off,        // chạy 1 lần rồi dừng
        Infinite,   // loop mãi
        Custom      // loop N lần
    }

    [System.Serializable]
    public class ParticleSettings
    {
        public ParticleSystem system;

        [Header("Start Size Settings")]
        public bool overrideSize = false;
        public float minStartSize = 0.5f;
        public float maxStartSize = 1.5f;

        [Header("Start Speed Settings")]
        public bool overrideSpeed = false;
        public float startSpeed = 10f;

        [Header("Lifetime Settings")]
        public bool overrideLifetime = false;
        public float minLifetime = 0.1f;
        public float maxLifetime = 5f;

        [Header("Playback Settings")]
        public bool overrideDuration = false;
        public float duration = 1f;  // mặc định 1s

        public LoopMode loopMode = LoopMode.Off;
        public int customLoopCount = 1;   // số lần lặp nếu loopMode = Custom

        [HideInInspector] public int playedLoops = 0;
    }

    public List<ParticleSettings> particleSystems = new List<ParticleSettings>();

    void Update()
    {
        foreach (var psSetting in particleSystems)
        {
            if (psSetting.system == null) continue;

            var main = psSetting.system.main;

            // Override values nếu bật
            if (psSetting.overrideSize)
                main.startSize = new ParticleSystem.MinMaxCurve(psSetting.minStartSize, psSetting.maxStartSize);

            if (psSetting.overrideSpeed)
                main.startSpeed = psSetting.startSpeed;

            if (psSetting.overrideLifetime)
                main.startLifetime = new ParticleSystem.MinMaxCurve(psSetting.minLifetime, psSetting.maxLifetime);

            if (psSetting.overrideDuration)
                main.duration = psSetting.duration;

            // Xử lý loop mode
            switch (psSetting.loopMode)
            {
                case LoopMode.Off:
                    main.loop = false;
                    break;

                case LoopMode.Infinite:
                    main.loop = true;
                    break;

                case LoopMode.Custom:
                    main.loop = false; // ta tự quản lý loop
                    if (psSetting.system.time >= main.duration)
                    {
                        psSetting.playedLoops++;
                        if (psSetting.playedLoops < psSetting.customLoopCount)
                        {
                            psSetting.system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                            psSetting.system.Play();
                        }
                        else
                        {
                            psSetting.system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        }
                    }
                    break;
            }
        }
    }
}
