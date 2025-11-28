using Fusion.TankOnlineModule;
using Unity.VisualScripting;
using UnityEngine;

public class TankPreview : MonoBehaviour
{
    [SerializeField] private Camera mCamera;
    [SerializeField] private Transform mTankTransform;
    [SerializeField] private TankPartVisual mTankPartVisual;
    private float rotationSpeed = 60f;

    private void Start()
    {
        mCamera = GetComponentInChildren<Camera>();
        mTankTransform.AddComponent<MotorShake>();
    }

    public void SetRenderTexture(RenderTexture rt)
    {
        if (mCamera != null)
        {
            mCamera.targetTexture = rt;
        }
    }

    // User can rotate the tank preview by dragging the mouse or finger
    private Vector2 lastTouchPosition;
    private bool isDragging = false;

    private void Update()
    {
        // block interaction if pointer is over UI
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        TouchRotate();
    }

    public void TouchRotate()
    {
        // Mouse rotation
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            mTankTransform.Rotate(Vector3.up, -mouseX * rotationSpeed * Time.deltaTime, Space.World);
        }

        // Touch rotation
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                float deltaX = touch.position.x - lastTouchPosition.x;
                mTankTransform.Rotate(Vector3.up, -deltaX * rotationSpeed * Time.deltaTime * 0.1f, Space.World);
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
    }
    
    public void ReloadWrap(int wrapId, string tankId = "")
    {
        if (mTankPartVisual)
            mTankPartVisual.LoadWrap(wrapId, tankId);
    }
}