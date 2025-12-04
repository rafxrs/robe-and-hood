using UnityEngine;

namespace _Scripts.FX
{
    public class Impact : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Destroy(this.gameObject, 0.4f);
        }

    }
}
