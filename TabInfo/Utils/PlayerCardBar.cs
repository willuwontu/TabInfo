using UnityEngine;

namespace TabInfo.Utils
{
    public class PlayerCardBar : MonoBehaviour
    {
        public PlayerFrame playerFrame;
        public Player player;

        private void Start()
        {
            this.playerFrame = this.gameObject.GetComponentInParent<PlayerFrame>();
        }
    }
}
