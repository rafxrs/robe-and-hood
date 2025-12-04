using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class FloatingHealthBar : MonoBehaviour
    {
        private Slider _slider;
        [SerializeField] Transform target;
        // Start is called before the first frame update
        void Start()
        {
            target = transform.parent.parent;
            if (target==null)
            {
                Debug.Log("Root is null");
            }
            _slider = GetComponent<Slider>();
            if (_slider == null)
            {
                // Debug.LogError("slider of health bar from "+target.name+ " is null");
            }
        
        }

        public void SetMax(int max)
        {
            if (_slider == null)
            {
                // Debug.LogError("slider of health bar from "+target.name+ " is null");
            }
            else 
            {
                _slider.maxValue = max;
                _slider.value = max;
            }
        
        }
        public void Set(int current)
        {
            _slider.value = current;
        }

        // Update is called once per frame
        void Update()
        {
            transform.localScale = target.localScale; 
        }
    }
}
