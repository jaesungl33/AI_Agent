using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace GDOLib.UI
{
    [RequireComponent(typeof(Canvas))]
    public class ChildCanvasUtility : MonoBehaviour
    {
        [FormerlySerializedAs("sortOrderOffset")] public int sortingOrderOffset;
        
        private Canvas parentCanvas;
        private Canvas canvas;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            parentCanvas = GetComponentsInParent<Canvas>(true).FirstOrDefault(c => c != canvas);
        }

        private void OnEnable()
        {
            UpdateSortingOrder();
        }

        public void UpdateSortingOrder()
        {
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentsInParent<Canvas>(true).FirstOrDefault(c => c != canvas);
                if (parentCanvas == null)
                    return;
            }

            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = parentCanvas.sortingOrder + sortingOrderOffset;
            }
            else
            {
                Debug.LogWarning("[Warning] Canvas not found]");
            }
        }

        private void Start()
        {
            OnEnable();
        }
    }
}