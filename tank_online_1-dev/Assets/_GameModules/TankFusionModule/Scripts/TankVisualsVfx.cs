using UnityEngine;
using Fusion;
using Fusion.TankOnlineModule;

public class TankVisualsVfx : NetworkBehaviour
{
    public enum VfxType
    {
        None = 0,
        SlowStorm = 1,
        SpawnVfxBlue = 2,
        SpawnVfxRed = 3,
    }
    [SerializeField] private AssetContainer assetContainer;
    private GameObject[] vfxInstances;
    private float[] vfxDurations;
    private Player player;
    private void Awake()
    {
        player = GetComponentInParent<Player>();
        int vfxCount = System.Enum.GetValues(typeof(VfxType)).Length;
        vfxInstances = new GameObject[vfxCount];
        vfxDurations = new float[vfxCount];
    }
    private void Update()
    {
        for (int i = 0; i < vfxDurations.Length; i++)
        {
            if (vfxDurations[i] > 0f)
            {
                vfxDurations[i] -= Time.deltaTime;
                if (vfxDurations[i] <= 0f)
                {
                    SetVfx((VfxType)i, false);
                }
            }
        }
    }
    public void SetVfx(VfxType type, bool enable, float duration = 0f)
    {
        int idx = (int)type;
        if (enable)
        {
            // Nếu đã có duration lớn hơn thì giữ lại, chỉ set lại nếu duration mới lớn hơn
            if (duration > 0f)
            {
                if (vfxDurations[idx] < duration)
                    vfxDurations[idx] = duration;
            }

            if (vfxInstances[idx] == null)
            {
                var prefab = assetContainer.Get<GameObject>(type.ToString());
                if (prefab != null)
                    vfxInstances[idx] = Instantiate(prefab, transform);
            }
            if (vfxInstances[idx] != null)
                vfxInstances[idx].SetActive(true);
        }
        else
        {
            if (vfxInstances[idx] != null)
                vfxInstances[idx].SetActive(false);
            vfxDurations[idx] = 0f;
        }
    }
}