using UnityEngine;

public class LookAtMainCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private int _retryCount = 0;
    [SerializeField] private Vector3 positionOffset = Vector3.up;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_retryCount > 10)
        {
            return;
        }

        if (_camera == null)
        {
            _retryCount++;
            _camera = Camera.main;

            if (_retryCount >= 10)
            {
                Debug.LogError("LookAtMainCamera: Camera.main is not set after multiple attempts.");
                return;
            }
        }
        // Make the name tag face away from the camera
        Vector3 direction = transform.position - _camera.transform.position;
        transform.position = transform.parent.position + positionOffset; // Adjust height if needed

        // Only rotate on Y axis, keep X rotation at 45 degrees
        float yRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        
        // Snap Y rotation to 0 or 180 based on which is closer
        if (Mathf.Abs(yRotation) < 90f)
        {
            yRotation = -45f;
        }
        else
        {
            yRotation = 135;
        }
        
        transform.rotation = Quaternion.Euler(45f, yRotation, 0f);
    }
}