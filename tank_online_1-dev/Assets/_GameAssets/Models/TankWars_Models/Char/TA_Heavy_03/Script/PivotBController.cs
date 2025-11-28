using UnityEngine;

[ExecuteAlways]
public class ChainUpdater : MonoBehaviour
{
    [Header("References")]
    public ChainCreator chainCreator;
    public Transform pivotB;

    private Vector3 lastPivotBPos;

    void Update()
    {
        if (chainCreator == null || pivotB == null)
            return;

        // Kiểm tra xem pivotB có di chuyển không
        if (pivotB.position != lastPivotBPos)
        {
            lastPivotBPos = pivotB.position;

            // Gọi rebuild lại sợi xích
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
#endif

            // Nếu ChainCreator có hàm public để build lại chain
            if (chainCreator.enabled)
            {
                // Giả sử ChainCreator có hàm tên là AutoBuild hoặc BuildChain
                chainCreator.SendMessage("BuildChain", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
