using UnityEngine;

public class AnimationParticleSpawner : MonoBehaviour
{
    [Header("Particle Settings")]
    public GameObject particlePrefab;   // Prefab particle
    public Transform spawnPoint;        // Vị trí spawn, mặc định = transform

    // Hàm này sẽ được gọi từ Animation Event
    public void SpawnParticle()
    {
        if (particlePrefab == null) return;

        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint ? spawnPoint.rotation : transform.rotation;

        GameObject particle = Instantiate(particlePrefab, pos, rot);

        // Nếu particle có hệ thống ParticleSystem thì tự hủy sau khi chạy xong
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(particle, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(particle, 3f); // fallback
        }
    }
}
