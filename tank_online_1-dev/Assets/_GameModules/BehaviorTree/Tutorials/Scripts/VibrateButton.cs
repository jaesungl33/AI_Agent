using GDOLib.UI;
using UnityEngine;

namespace GDOLib.Components
{
    public class VibrateButton : UIButton
    {
        [Tooltip("Enable or disable vibration feedback on button press [todo: can use usersetting to set this value].")]
        [SerializeField] private bool enableVibrate = true;
        public void DoLightFeedback()
        {
            if (!enableVibrate) return;
            Debug.Log("HapticFeedback.LightFeedback");
            //HapticFeedback.LightFeedback();
        }
        public void DoMediumFeedback()
        {
            if (!enableVibrate) return;
            Debug.Log("HapticFeedback.MediumFeedback");
            //HapticFeedback.MediumFeedback();
        }
        public void DoHeavyFeedback()
        {
            if (!enableVibrate) return;
            Debug.Log("HapticFeedback.HeavyFeedback");
            //HapticFeedback.HeavyFeedback();
        }
    }
}