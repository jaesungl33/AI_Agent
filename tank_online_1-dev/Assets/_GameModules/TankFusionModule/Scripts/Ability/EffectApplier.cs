using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.TankOnlineModule;

public class EffectApplier : NetworkBehaviour
{
    private Player player;
    private bool initialized = false;

    // Track active effects
    private readonly List<EffectData> _activeEffects = new();

    public void Initialize(Player player)
    {
        if (initialized) return;
        this.player = player;
        initialized = true;

        ClearEffects();
    }

    /// <summary>
    /// Apply a new effect to the player (networked).
    /// </summary>
    public EffectData ApplyEffect(EffectData effect)
    {
        if (effect == null) return null;

        // Non-stackable effect logic
        if (EffectData.IsNonStackable(effect.effectId))
        {
            var existing = _activeEffects.Find(e => e.effectId == effect.effectId);
            if (existing != null)
            {
                existing.duration = effect.duration;
                existing.lifeTime = effect.lifeTime;
                existing.value = effect.value;
                existing.dir = effect.dir;
                return existing;
            }
        }

        _activeEffects.Add(effect);
        OnEffectApplied(effect);
        return effect;
    }

    /// <summary>
    /// Remove a specific effect instance.
    /// </summary>
    public void RemoveEffect(EffectData effect)
    {
        if (effect == null) return;
        if (_activeEffects.Remove(effect))
        {
            OnEffectRemoved(effect);
        }
    }

    /// <summary>
    /// Update all active effects (call in FixedUpdateNetwork).
    /// </summary>
    public void UpdateEffects(float deltaTime)
    {
        //Debug.LogErrorFormat("UpdateEffects: {0} active effects", _activeEffects.Count);
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];
            effect.duration -= deltaTime;
            if (effect.duration <= 0)
            {
                OnEffectRemoved(effect);
                _activeEffects.RemoveAt(i);
            }
            else
            {
                OnEffectTick(effect, deltaTime);
            }
        }
    }

    /// <summary>
    /// Remove all effects.
    /// </summary>
    public void ClearEffects()
    {
        foreach (var effect in _activeEffects)
        {
            OnEffectRemoved(effect);
        }
        _activeEffects.Clear();
    }

    protected virtual void OnEffectApplied(EffectData effect)
    {
        switch (effect.effectId)
        {
            // Debuffs
            case (int)EffectData.EffectType.Slow:
                player.MoveSpeedModifyRatio -= effect.value;
                break;
            case (int)EffectData.EffectType.DecreaseFireRate:
                player.FireRateModifyRatio -= effect.value;
                break;

            //Effect custom by Ability
            case (int)EffectData.EffectType.SlowStorm:
                player.MoveSpeedModifyRatio -= effect.value;
                player.FireRateModifyRatio -= effect.value;
                player.TankVisualsVfx.SetVfx(TankVisualsVfx.VfxType.SlowStorm, true, effect.duration);
                break;
        }
    }
    protected virtual void OnEffectRemoved(EffectData effect)
    {
        switch (effect.effectId)
        {
            case (int)EffectData.EffectType.Slow:
                player.MoveSpeedModifyRatio += effect.value;
                break;
            case (int)EffectData.EffectType.DecreaseFireRate:
                player.FireRateModifyRatio += effect.value;
                break;

            //Effect custom by Ability
            case (int)EffectData.EffectType.SlowStorm:
                player.MoveSpeedModifyRatio += effect.value;
                player.FireRateModifyRatio += effect.value;
                player.TankVisualsVfx.SetVfx(TankVisualsVfx.VfxType.SlowStorm, false);
                break;
        }
    }

    protected virtual void OnEffectTick(EffectData effect, float deltaTime)
    {
        // Damage over time: Burn (stackable)
        switch (effect.effectId)
        {
            case (int)EffectData.EffectType.Burn:
                if (player.IsDead)
                {
                    //player is not found, remove the effect
                    RemoveEffect(effect);
                    break;
                }
                float damageThisTick = effect.value * deltaTime;
                player.RaiseEvent(new DamageEvent { targetPlayerRef = effect.srcPlayerId, damage = (int)damageThisTick });
                //_player.TakeDamage(damageThisTick);
                //// Optionally: show burn visual effect here
                break;
            // Add other periodic logic here
            case (int)EffectData.EffectType.Hook:
                {
                    Player srcPlayer = GameServer.Instance.GetPlayer<Player>(effect.srcPlayerId);
                    //Debug.LogErrorFormat("srcPlayer: {0}, {1}, {2}, {3}", srcPlayer.PlayerName, srcPlayer == null, srcPlayer.IsDead, player.IsDead);
                    if (srcPlayer == null || srcPlayer.IsDead || player.IsDead)
                    {
                        RemoveEffect(effect);
                        break;
                    }

                    // Vector3 hookDir = effect.dir;
                    // float hookDistance = effect.value;
                    // // Gán posStart ở tick đầu tiên
                    // if (effect.posStart == default)
                    //     effect.posStart = _player.transform.position;
                    // Vector3 start = effect.posStart;
                    // Vector3 end = start + hookDir * hookDistance;
                    // float step = 1f - (effect.duration / Mathf.Max(effect.lifeTime, 0.01f));
                    // step = Mathf.Clamp01(step);
                    // Vector3 nextPos = Vector3.Lerp(start, end, step);
                    // _player.NCC.ForceMove(nextPos);

                    if (effect.posEnd == default)
                        effect.posEnd = srcPlayer.transform.position;
                    Vector3 hookTo = effect.posEnd;
                    float duration = effect.duration;
                    //Debug.LogErrorFormat("hookTo: {0}, {1}, {2}", hookTo, duration, deltaTime);
                    player.NCC.ForceMovePhysic(hookTo, duration, deltaTime);

                    break;
                }

            case (int)EffectData.EffectType.Knockback:
                {
                    Player srcPlayer = GameServer.Instance.GetPlayer<Player>(effect.srcPlayerId);
                    if (srcPlayer == null || srcPlayer.IsDead || player.IsDead)
                    {
                        RemoveEffect(effect);
                        break;
                    }

                    Vector3 knockDir = effect.dir.normalized;

                    // Chỉ gán posStart ở tick đầu tiên
                    if (effect.posStart == default)
                        effect.posStart = player.transform.position;

                    Vector3 start = effect.posStart;
                    Vector3 end = start + knockDir * effect.value;

                    float step = 1f - (effect.duration / Mathf.Max(effect.lifeTime, 0.01f));
                    step = Mathf.Clamp01(step);

                    Vector3 nextPos = Vector3.Lerp(start, end, step);
                    player.NCC.ForceMoveInstant(nextPos);

                    break;
                }
        }
    }

    public override void FixedUpdateNetwork()
    {
        UpdateEffects(Runner.DeltaTime);
    }

    public bool HasEffect(int effectId)
    {
        //Debug.LogWarningFormat("HasEffect check for {0}: {1}", effectId, _activeEffects.Exists(e => e.effectId == effectId));
        return _activeEffects.Exists(e => e.effectId == effectId);
    }
}

public class EffectData
{
    public enum EffectType
    {
        //Buff Effect stackable
        SpeedBoost,
        DamageBoost,
        //buff Effect non-stackable
        Shield,
        ImmuneDamage,
        ImmuneCC,

        //Debuff Effect stackable
        Slow,
        DecreaseFireRate,

        //CC Effect non-stackable
        Stun,
        Root,
        Silence,
        Hook,
        Knockback,

        //effect damage over time, stackable
        Burn,

        //effect custom by Ability
        SlowStorm,
    }
    public byte effectId;
    public byte srcPlayerId;
    public float duration;
    public float value;

    //special properties  convert from angle
    public Vector3 dir = default;

    //handle properties not sync with raise event
    public bool forceCC;
    public float lifeTime;
    public Vector3 posStart = default;
    public Vector3 posEnd = default;

    public static bool IsCCEffect(int effectId)
    {
        return effectId == (int)EffectType.Stun ||
               effectId == (int)EffectType.Root ||
               effectId == (int)EffectType.Silence ||
               effectId == (int)EffectType.Hook ||
               effectId == (int)EffectType.Knockback;
    }
    public static bool IsBuffEffect(int effectId)
    {
        return effectId == (int)EffectType.SpeedBoost ||
               effectId == (int)EffectType.DamageBoost ||
               effectId == (int)EffectType.Shield ||
               effectId == (int)EffectType.ImmuneDamage ||
               effectId == (int)EffectType.ImmuneCC;
    }
    public static bool IsDebuffEffect(int effectId)
    {
        return effectId == (int)EffectType.Slow ||
               effectId == (int)EffectType.DecreaseFireRate ||
               effectId == (int)EffectType.SlowStorm;
    }
    public static bool IsNonStackable(int effectId)
    {
        return effectId == (int)EffectData.EffectType.Shield ||
               effectId == (int)EffectData.EffectType.Stun ||
               effectId == (int)EffectData.EffectType.Root ||
               effectId == (int)EffectData.EffectType.Silence ||
               effectId == (int)EffectData.EffectType.Hook ||
               effectId == (int)EffectData.EffectType.Knockback;
    }
}