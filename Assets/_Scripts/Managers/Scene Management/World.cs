using UnityEngine;

namespace _Scripts.Managers.Scene_Management
{
    public class World : SceneSwitch
    {
        public Transform player;
        public float posX;
        public float posY;
        public string previous;
        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
            if (player == null)
            {
                player = GameObject.Find("Player").GetComponent<Transform>();

            }
            if (previousScene == previous)
            {
                player.position = new Vector2(posX, posY);
            }
        }
    }
}
