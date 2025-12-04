using UnityEngine;

namespace _Scripts.Units.Objects
{
    public class Smasher : MonoBehaviour
    {
        //adjust this to change speed
        [SerializeField]
        float speed = 5f;
        //adjust this to change how high it goes
        [SerializeField]
        float height = 0.5f;

        Vector3 _pos;

        private void Start()
        {
            _pos = transform.position;
        }
        void Update()
        {

            //calculate what the new Y position will be
            float newY = Mathf.Sin(Time.time * speed) * height + _pos.y;
            //set the object's Y to the new calculated Y
            transform.position = new Vector3(transform.position.x, newY, transform.position.z) ;
        }

        void GoUp()
        {
            
        }

        void Smash()
        {
        
        }
    
    }
}
