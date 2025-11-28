using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public Transform target;
    public float smoothSpeed = 5f;

    private float fixedX;
    private float fixedY;
    private float zOffset;
    private Quaternion baseRotation;
    private bool isFlipped = false;
    private Vector3 currentVelocity;


    void Start()
    {
         if (target == null)
        {
            Debug.LogWarning("[CameraManager] target chưa được gán, chờ gán động từ TankSpawnTrigger.");
            return;
        }
        fixedX = transform.position.x;
        fixedY = transform.position.y;
        zOffset = transform.position.z - target.position.z;
        baseRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.rotation = isFlipped ? Quaternion.Euler(baseRotation.eulerAngles + new Vector3(0, 180, 0)) : baseRotation;

        float targetZ = target.position.z + zOffset;
        Vector3 desiredPosition = new Vector3(fixedX, fixedY, targetZ);

        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    public void FlipDirection()
    {
        isFlipped = !isFlipped;
        zOffset = -zOffset;
    }
}
