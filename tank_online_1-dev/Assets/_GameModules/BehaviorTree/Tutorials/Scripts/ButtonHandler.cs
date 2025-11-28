using System;
using DG.Tweening;
using GDO.Audio;
using GDOLib.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum BtnAnim
{
    None = 0,
    Zoom = 1,
    MoveY = 2,
}

public sealed class ButtonHandler : VibrateButton, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler,
    IPointerExitHandler, IHoldButtonTarget
{
    [SerializeField] private float _default_TimePlaySound = 0.1f;
    [SerializeField] private RectTransform _root;
    [SerializeField] private float _activeScale = 1f;
    [SerializeField] private float _normalScale = 1;
    [SerializeField] private bool usePointerEnter = false;
    [SerializeField] private bool useHold = false;
    [SerializeField] private bool useHold_Sound = false;
    [SerializeField] private bool useGrayscale = false;

    [SerializeField] private SoundType clickSoundType = SoundType.SoundEffect;
    [SerializeField] private string clickSoundID = "button_click";
    [SerializeField] private AudioClip soundClickAC;
    [SerializeField] private GameObject[] disableGOs;
    [SerializeField] private GameObject disableGO;
    
    public float moveYOfsset = 5;
    public BtnAnim btnAnim = BtnAnim.Zoom;

    public UnityEvent<PointerEventData> OnButtonDownEvent = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> OnButtonUpEvent = new UnityEvent<PointerEventData>();

    TouchData _touchData = null;

    private Action<bool> _touch;
    private Action<bool> _callbackEnter;
    private Action _onClick;

    private bool _in = false;
    private Sequence sequence = null;

    private Vector2 _scaleDefault;
    private bool _interactable = true;
    float defaultY;

    public void PlaySoundBySelected()
    {
        if (soundClickAC != null)
        {
            AudioManager.Instance.PlaySFX(soundClickAC);
            return;
        }
        AudioHelper.PlaySFX(clickSoundID);
    }

    public void PlayAnim()
    {
        _root.localScale = _normalScale * _scaleDefault;
        _root.DOScale(_activeScale * _scaleDefault, 0.1f);
    }

    private void ActiveUi()
    {
        if (btnAnim == BtnAnim.Zoom)
        {
            sequence = DOTween.Sequence();
            sequence.Append(_root.DOScale(_activeScale * _scaleDefault, 0.1f));
        }
        else if (btnAnim == BtnAnim.MoveY)
        {
            sequence = DOTween.Sequence();
            sequence.Append(_root.DOAnchorPosY(defaultY - moveYOfsset, 0.1f));
        }
    }

    private void DeactiveUi()
    {
        sequence = DOTween.Sequence();
        if (btnAnim == BtnAnim.Zoom)
        {
            sequence.Append(_root.DOScale(_normalScale * _scaleDefault, 0.1f));
        }
        else if (btnAnim == BtnAnim.MoveY)
        {
            sequence.Append(_root.DOAnchorPosY(defaultY, 0.1f));
        }
    }

    private void Awake()
    {
        Button.onClick.AddListener(() =>
        {
            DoLightFeedback();

            PlaySoundBySelected();
        });
        if (useHold && GetComponent<HoldButton>() == null)
        {
            gameObject.AddComponent<HoldButton>();
        }

        if (_root == null)
        {
            _root = GetComponent<RectTransform>();
        }

        if (btnAnim == BtnAnim.MoveY)
        {
            defaultY = _root.anchoredPosition.y;
        }

        _scaleDefault = _root.transform.localScale;
        _touch = (r) =>
        {
            //Debug.Log("_touch: " + r);
            if (sequence != null)
            {
                sequence.Kill();
            }

            if (r)
            {
                ActiveUi();
            }
            else
            {
                DeactiveUi();
            }
        };
        _callbackEnter = (r) =>
        {
            if (!usePointerEnter) return;
            //Debug.Log("_callbackEnter: " + r);
            if (sequence != null)
            {
                sequence.Kill();
            }

            if (r)
            {
                ActiveUi();
            }
            else
            {
                DeactiveUi();
            }
        };
        _onClick = () =>
        {
            if (sequence != null)
            {
                sequence.Kill();
            }

            DeactiveUi();
        };
    }

    GrayScaleImage[] cacheGrayscale;

    private void Update()
    {
        if (_interactable != Button.interactable)
        {
            _interactable = Button.interactable;
            if (btnAnim == BtnAnim.MoveY)
            {
                if (sequence == null || !sequence.IsActive())
                {
                    sequence = DOTween.Sequence();
                }
            }

            UpdateGrayscale();
        }

        if (disableGO != null)
            disableGO.SetActive(!_interactable);

        foreach (var item in disableGOs)
        {
            item.SetActive(!_interactable);
        }
    }

    private void UpdateGrayscale()
    {
        if (useGrayscale)
        {
            if (cacheGrayscale == null)
                cacheGrayscale = GetComponentsInChildren<GrayScaleImage>();

            foreach (var grayScaleImage in cacheGrayscale)
            {
                grayScaleImage.enabled = !_interactable;
                grayScaleImage.UpdateGrayScale();
            }
        }
    }

    private void OnDisable()
    {
        Clear();
    }

    private void OnDestroy()
    {
        Clear();
    }

    private void OnEnable()
    {
        if (btnAnim == BtnAnim.MoveY)
        {
            _root.anchoredPosition = new Vector2(_root.anchoredPosition.x, defaultY);
        }

        UpdateGrayscale();
    }

    private void Clear()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }

        sequence = null;
        _touchData = null;
        _in = false;

        if (btnAnim == BtnAnim.Zoom)
        {
            _root.transform.localScale = Vector3.one * _scaleDefault;
        }
        else if (btnAnim == BtnAnim.MoveY)
        {
            var tmp = new Vector3(_root.anchoredPosition.x, defaultY, 0);
            _root.anchoredPosition = tmp;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnButtonDownEvent?.Invoke(eventData);
        if (!Button.enabled)
        {
            _touchData = null;
            return;
        }

        _touchData = new TouchData { PointerEventData = eventData, PrePosition = eventData.pressPosition };
        _in = true;
        _touch(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnButtonUpEvent?.Invoke(eventData);
        if (_touchData != null)
        {
            if (_in)
            {
                if (_onClick != null)
                {
                    _onClick();
                }
            }

            if (_touch != null)
            {
                _touch(false);
            }

            _in = false;
            _touchData = null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_touchData != null)
        {
            _in = true;
            _callbackEnter(_in);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_touchData != null)
        {
            _in = false;
            _callbackEnter(_in);
        }
    }

    public void OnClick()
    {
    }

    public void OnHoldClick()
    {
        if (useHold)
        {
            Button.onClick?.Invoke();
        }
    }
}