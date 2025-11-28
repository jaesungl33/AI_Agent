using UnityEngine;
using UnityEngine.EventSystems;

namespace GDOLib.Components
{
    public interface IHoldButtonTarget 
    {
        void OnClick();

        void OnHoldClick();
    }

    
    [RequireComponent(typeof(UnityEngine.UI.Button))]
//[RequireComponent(typeof(IHoldButtonTarget))]
    public class HoldButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler,
        IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private const float HoldIntervalMin = 0.0001f;
        private const float HoldThreshold = 0.5f;
        [SerializeField] private float HoldInterval = 0.25f;
        private const float HoldSpeedAccelerate = 0.0015f;

        private float _holdTime;
        private float _holdInterval;
        private bool _isHolding = false;
        private bool _enter = false;
        bool _isDraging = false;


        private IHoldButtonTarget _target;

        void Start()
        {
            _target = GetComponentInParent<IHoldButtonTarget>();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isHolding = false;

            if (_holdTime < HoldThreshold && !_isDraging)
            {
                DispatchClickEvent(eventData);
            }

            _holdTime = 0;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isHolding = true;
            _holdTime = HoldThreshold;
            _holdInterval = HoldInterval;
        }

        // Update is called once per frame
        void Update()
        {
            if (_isHolding && _holdTime > 0)
            {
                _holdTime -= Time.deltaTime;
                if (_holdTime < 0)
                {
                    DispatchHoldEvent();
                    _holdInterval -= HoldSpeedAccelerate;
                    _holdInterval = Mathf.Max(_holdInterval, HoldIntervalMin);
                    _holdTime = _holdInterval;
                }
            }
        }

        protected void DispatchClickEvent(PointerEventData eventData)
        {
            _target.OnClick();
        }

        protected void DispatchHoldEvent()
        {
            _target.OnHoldClick();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _enter = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _enter = false;
            _isHolding = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDraging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDraging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _isDraging = true;
        }

        private void OnDisable()
        {
            _enter = false;
            _isDraging = false;
            _isHolding = false;
        }
    }
}