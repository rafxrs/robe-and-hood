using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class Bar : MonoBehaviour
    {
        public Slider slider;
        //-------------------------------------------------------------------------------------------//
        public void SetMax(int maxValue)
        {
            slider.maxValue = maxValue;
            slider.value = maxValue;
        }
        public void Set(int value)
        {
            slider.value = value;
        }

    }
}
