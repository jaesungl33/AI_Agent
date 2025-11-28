using UnityEngine;
using System;

namespace GDOLib.UI
{
    public class PopupAnimation : MonoBehaviour
    {
        public virtual void OnShowAnim(Action callback)
        {
            callback?.Invoke();
        }

        public virtual void OnHideAnim(Action callback)
        {
            callback?.Invoke();
        }
    }
}