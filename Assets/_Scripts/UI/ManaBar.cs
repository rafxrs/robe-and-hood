using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class ManaBar : MonoBehaviour
    {
        public Slider slider;
        public void SetMaxMana(float health)
        {
            slider.maxValue = health;
            slider.value = health;
        }
    
        public void SetMana(float health)
        {
            slider.value = health;
        }
    }
}
