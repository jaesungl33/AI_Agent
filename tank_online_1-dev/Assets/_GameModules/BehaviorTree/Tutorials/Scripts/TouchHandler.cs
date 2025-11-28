using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GDOLib.Components
{
    public class TouchData
    {
        public PointerEventData PointerEventData;
        public Vector2 PrePosition;
    }

    public sealed class TouchHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        List<TouchData> _touches = new List<TouchData>();

        private Action<Vector2, Vector2> _callbackPreAndCurrentPosition;
        private Action<Vector2, Vector2> _callbackStartAndCurrentPosition;
        private Action<float, float> _callbackForZoom;
        private Action<bool> _touch;
        private Action<Vector2> _onClick;
        private float _limitDistanceToBecomeDrag = 0;
        private bool _isDraging = false;
        private bool _canUseOnClick = false;

        public bool IsDraging
        {
            get { return _isDraging; }
        }

        public void InitTouchHandler(Action<Vector2, Vector2> callbackPreAndCurrentPosition, Action<bool> touch = null,
            Action<Vector2> onClick = null, Action<float, float> callbackForZoom = null)
        {
            _callbackPreAndCurrentPosition = callbackPreAndCurrentPosition;
            _callbackStartAndCurrentPosition = null;
            _callbackForZoom = callbackForZoom;
            _touch = touch;
            _onClick = onClick;
            _limitDistanceToBecomeDrag = 10f;
        }

        public void InitTouchHandler(Action<Vector2, Vector2> callbackPreAndCurrentPosition,
            float limitDistanceToBecomeDrag, Action<bool> touch = null, Action<Vector2> onClick = null)
        {
            _callbackPreAndCurrentPosition = callbackPreAndCurrentPosition;
            _callbackStartAndCurrentPosition = null;
            _callbackForZoom = null;
            _touch = touch;
            _onClick = onClick;
            limitDistanceToBecomeDrag = limitDistanceToBecomeDrag <= 0 ? 0 : limitDistanceToBecomeDrag;
            _limitDistanceToBecomeDrag = limitDistanceToBecomeDrag;
        }

        public void InitTouchHandlerCallbackStartAndCurrent(Action<Vector2, Vector2> callbackStartAndCurrentPosition,
            Action<bool> touch = null, Action<Vector2> onClick = null)
        {
            _callbackForZoom = null;
            _callbackPreAndCurrentPosition = null;
            _callbackStartAndCurrentPosition = callbackStartAndCurrentPosition;
            _touch = touch;
            _onClick = onClick;
            _limitDistanceToBecomeDrag = 10f;
        }

        void Update()
        {
            if (_touches.Count == 1)
            {
                var touch = _touches[0];
                if (!_isDraging &&
                    Vector2.Distance(touch.PointerEventData.position, touch.PointerEventData.pressPosition) >
                    _limitDistanceToBecomeDrag)
                {
                    _canUseOnClick = false;
                    _isDraging = true;
                }

                if (_isDraging)
                {
                    if (_callbackPreAndCurrentPosition != null)
                    {
                        _callbackPreAndCurrentPosition(touch.PrePosition, touch.PointerEventData.position);
                    }
                    else if (_callbackStartAndCurrentPosition != null)
                    {
                        _callbackStartAndCurrentPosition(touch.PrePosition, touch.PointerEventData.position);
                    }
                }

                if (_callbackPreAndCurrentPosition != null)
                {
                    touch.PrePosition = touch.PointerEventData.position;
                }
            }
            else if (_touches.Count > 1)
            {
                if (_callbackForZoom != null)
                {
                    var touch1 = _touches[_touches.Count - 2];
                    var touch2 = _touches[_touches.Count - 1];
                    var d1 = Vector2.Distance(touch1.PrePosition, touch2.PrePosition);
                    var d2 = Vector2.Distance(touch1.PointerEventData.position, touch2.PointerEventData.position);
                    if (Mathf.Abs(d2 - d1) > 1)
                    {
                        _callbackForZoom(d1, d2);
                    }

                    for (int i = 0; i < _touches.Count; i++)
                    {
                        _touches[i].PrePosition = _touches[i].PointerEventData.position;
                    }
                }
            }
        }

        private void OnDisable()
        {
            _touches.Clear();
            _isDraging = false;
            _canUseOnClick = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_touches.Any(x => x.PointerEventData == eventData))
            {
                TouchData touchData = new TouchData();
                _touches.Add(new TouchData { PointerEventData = eventData, PrePosition = eventData.pressPosition });
                if (_touches.Count == 1)
                {
                    _canUseOnClick = true;
                    if (_touch != null)
                    {
                        _touch(true);
                    }
                }
                else
                {
                    _canUseOnClick = false;
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var touch = _touches.FirstOrDefault(x => x.PointerEventData == eventData);
            _touches.Remove(touch);
            if (_touches.Count == 0)
            {
                if (_canUseOnClick)
                {
                    if (_onClick != null)
                    {
                        _onClick(eventData.position);
                    }
                }

                if (_touch != null)
                {
                    _touch(false);
                }

                _isDraging = false;
                _canUseOnClick = false;
            }
        }
    }
}