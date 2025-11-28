using UnityEngine;
using UnityEngine.UI;

namespace GDOLib.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButton : UIPresenter
    {
        private Button _button;

        public Button Button
        {
            get
            {
                if (_button == null){
                    _button = GetComponent<Button>();
                    if (_button == null)
                    {
                        Debug.LogError("UIButton requires a Button component.");
                    }
                }
                return _button;
            }
        }
    }
}