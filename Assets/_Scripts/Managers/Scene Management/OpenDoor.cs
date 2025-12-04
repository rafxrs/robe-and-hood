using UnityEngine;

namespace _Scripts.Managers.Scene_Management
{
    /// <summary>
    /// This code is attached to any object that can be entered or exited (house, mine, etc...)
    /// </summary>
    public class OpenDoor : MonoBehaviour
    {
        public bool playerDetected;
        public Transform doorPos;
        public float width;
        public float height;
        public LayerMask whatIsPlayer;
        public Transform teleportTo;
        Transform _player;
    
        // Start is called before the first frame update
        void Start()
        {
            _player = GameObject.Find("Player").GetComponent<Transform>();
        }

        // Update is called once per frame
        void Update()
        {
            playerDetected = Physics2D.OverlapBox(doorPos.position, new Vector2(width, height),0,whatIsPlayer);

            if (playerDetected)
            {
                // Debug.Log("Player detected");
                // show button E
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("Opening door");
                    // sceneSwitch.SwitchScene(sceneName);
                    _player.position = teleportTo.position;
                }
            }
        }
        /// <summary>
        /// Draws a blue gizmos of the door frame
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(doorPos.position, new Vector3(width,height,1));
        }

    }
}
