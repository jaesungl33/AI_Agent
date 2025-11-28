using Fusion.TankOnlineModule;
using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [Header("Target (nhân vật chính)")]
    public Transform target;

    [Header("Độ cao của camera")]
    public float height = 50f;

    [Header("Độ mượt khi theo dõi")]
    public float smoothSpeed = 5f;

    public void Initialize(Player target)
    {
        this.target = target.transform;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Camera chỉ follow theo X, Z, giữ Y = height
        Vector3 desiredPosition = new Vector3(target.position.x, height, target.position.z);

        // Nội suy để mượt
        Vector3 smoothed = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothed;

        // Hướng camera thẳng xuống dưới
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
