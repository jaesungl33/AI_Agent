using Fusion.TankOnlineModule;
using FusionHelpers;
using UnityEngine;

public class CameraFollowIsometric : MonoBehaviour
{
    [Header("Target")]
    public Player target;
    public Vector3 worldOffset = Vector3.zero;   // Dịch chuyển target trong world (nếu muốn lệch khung)

    [Header("Isometric Settings")]
    public Vector3 isoEuler = new Vector3(35f, 135f, 0f); // Góc cố định
    public float distance = 12f;                          // Khoảng cách từ camera tới target dọc theo hướng nhìn

    [Header("Smoothing")]
    [Tooltip("= 0 để bám cứng, >0 để mượt hơn")]
    public float smoothTime = 0.15f;

    private Vector3 _vel;

    public void Initialize(Player target)
    {
        this.target = target;
        if (target == null) return;

        // Đặt vị trí ngay lập tức khi khởi tạo
        Quaternion rot = Quaternion.Euler(isoEuler);
        Vector3 fwd = rot * Vector3.forward;
        Vector3 focus = target.transform.position + worldOffset;
        transform.position = focus - fwd * distance;
        transform.rotation = rot;
        isoEuler = new Vector3(35f, target.Direction == 1 ? 315 : 135, 0f);
    }

    void LateUpdate()
    {
        if (target == null) return;
        isoEuler = new Vector3(35f, target.Direction == 1 ? 315 : 135, 0f);
        // Góc quay cố định cho isometric
        Quaternion rot = Quaternion.Euler(isoEuler);
        transform.rotation = rot;

        // Hướng nhìn (forward) theo góc trên
        Vector3 fwd = rot * Vector3.forward;

        // Đặt vị trí sao cho target nằm trên trục nhìn của camera ở khoảng cách 'distance'
        Vector3 focus = target.transform.position + worldOffset;
        Vector3 desiredPos = focus - fwd * distance;

        if (smoothTime <= 0f)
            transform.position = desiredPos;
        else
            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _vel, smoothTime);
    }

#if UNITY_EDITOR
    // Gizmo nhỏ để nhìn thấy điểm focus trong Scene view
    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(target.transform.position + worldOffset, 0.2f);
    }
#endif
}